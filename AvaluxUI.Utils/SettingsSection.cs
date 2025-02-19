using System.Text.Json;
using System.Xml;

namespace AvaluxUI.Utils;

public class SettingsSection : ISettingsSection
{
    internal Dictionary<string, string?> Values { get; }
    internal Dictionary<string, SettingsSection> Sections { get; }
    public string? Name { get; }

    public event Action? Changed;

    internal SettingsSection(string? name = null, Dictionary<string, string?>? dictionary = null,
        Dictionary<string, SettingsSection>? sections = null)
    {
        Name = name;
        Values = dictionary ?? [];
        Sections = sections ?? [];
    }

    public ISettingsSection GetSection(string key)
    {
        if (Sections.TryGetValue(key, out var section))
            return section;
        section = new SettingsSection(key, [], []);
        section.Changed += Changed;
        Sections.Add(key, section);
        Changed?.Invoke();
        return section;
    }

    public bool RemoveSection(string key)
    {
        if (!Sections.Remove(key, out var section))
            return false;
        section.Changed -= Changed;
        Changed?.Invoke();
        return true;
    }

    private void Set(string? key, string? value)
    {
        if (key == null)
            return;
        Values[key] = value;
        Changed?.Invoke();
    }

    public void Set(string? key, object? obj)
    {
        Set(key, JsonSerializer.Serialize(obj));
    }

    public bool Remove(string key)
    {
        var res = Values.Remove(key);
        Changed?.Invoke();
        return res;
    }

    private string? Get(string key)
    {
        return Values.GetValueOrDefault(key);
    }

    public T Get<T>(string key, T defaultValue)
    {
        var res = Get<T>(key);
        return res ?? defaultValue;
    }

    public T? Get<T>(string key)
    {
        var str = Get(key);
        if (str == null)
            return default;
        try
        {
            return JsonSerializer.Deserialize<T>(str);
        }
        catch (JsonException)
        {
            return default;
        }
    }

    internal XmlElement ToXml(XmlDocument document)
    {
        var root = document.CreateElement("section");
        root.SetAttribute("name", Name);

        var global = document.CreateElement("global");
        foreach (var item in Values)
        {
            if (item.Value != null)
            {
                var tag = document.CreateElement(item.Key);
                tag.InnerText = item.Value;
                global.AppendChild(tag);
            }
        }

        root.AppendChild(global);

        foreach (var section in Sections.Values)
        {
            root.AppendChild(section.ToXml(document));
        }

        return root;
    }

    internal static SettingsSection FromXml(XmlNode root)
    {
        var name = root.Attributes?["name"]?.Value;
        var values = root.SelectSingleNode("global")?.ChildNodes
            .Cast<XmlNode>()
            .Select(n => new KeyValuePair<string, string?>(n.Name, n.InnerText))
            .ToDictionary();
        var sections = root.SelectNodes("section")?
            .Cast<XmlNode>()
            .Select(FromXml)
            .Select(s =>
                new KeyValuePair<string, SettingsSection>(s.Name ?? throw new Exception("Section name can not be null"),
                    s))
            .ToDictionary();
        return new SettingsSection(name, values, sections);
    }
}
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace AvaluxUI.Utils;

public class SettingsSection : ISettingsSection
{
    internal Dictionary<string, string?> Values { get; private set; }
    internal Dictionary<string, SettingsSection> Sections { get; }
    public string? Name { get; }
    private byte[]? SecretKeyHash { get; }

    public event Action? Changed;

    internal SettingsSection(string? name = null, Dictionary<string, string?>? dictionary = null,
        Dictionary<string, SettingsSection>? sections = null, byte[]? secretKeyHash = null)
    {
        Name = name;
        Values = dictionary ?? [];
        Sections = sections ?? [];
        SecretKeyHash = secretKeyHash;
    }

    public ISettingsSection GetSection(string key)
    {
        if (Sections.TryGetValue(key, out var section))
        {
            if (section.SecretKeyHash != null)
                throw new Exception("Section is encrypted");
            return section;
        }

        section = new SettingsSection(key, [], []);
        section.Changed += Changed;
        section.RereadEvent += RereadEvent;
        Sections.Add(key, section);
        Changed?.Invoke();
        return section;
    }

    public ISettingsSection GetSection(string key, string secretKey)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(secretKey));
        if (Sections.TryGetValue(key, out var section))
        {
            if (section.SecretKeyHash?.SequenceEqual(hash) == false)
                throw new Exception("Secret key hash does not match secretKey hash");
            if (section is EncryptedSettingsSection encryptedSection)
                return encryptedSection;

            section.RereadEvent -= RereadEvent;
            section.Changed -= Changed;

            var newEncryptedSection =
                new EncryptedSettingsSection(secretKey, section.Name, section.Values, section.Sections);
            Sections[key] = newEncryptedSection;

            newEncryptedSection.Changed += Changed;
            newEncryptedSection.RereadEvent += RereadEvent;

            return newEncryptedSection;
        }

        section = new EncryptedSettingsSection(secretKey, key, [], []);
        section.Changed += Changed;
        section.RereadEvent += RereadEvent;
        Sections.Add(key, section);
        Changed?.Invoke();
        return section;
    }

    public bool RemoveSection(string key)
    {
        if (!Sections.Remove(key, out var section))
            return false;
        section.Changed -= Changed;
        section.RereadEvent -= RereadEvent;
        Changed?.Invoke();
        return true;
    }

    private void Set(string? key, string? value)
    {
        if (key == null)
            return;
        Values[key] = Encrypt(value);
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
        RereadEvent?.Invoke();
        return Decrypt(Values.GetValueOrDefault(key));
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

    protected IEnumerable<XmlElement> ToXmlElements(XmlDocument document)
    {
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

        yield return global;

        foreach (var section in Sections.Values)
        {
            yield return section.ToXml(document);
        }
    }

    private XmlElement ToXml(XmlDocument document)
    {
        var root = document.CreateElement("section");
        root.SetAttribute("name", Name);
        if (SecretKeyHash != null)
            root.SetAttribute("encrypt", Convert.ToBase64String(SecretKeyHash));
        foreach (var tag in ToXmlElements(document))
        {
            root.AppendChild(tag);
        }

        return root;
    }

    internal static SettingsSection FromXml(XmlNode root)
    {
        var name = root.Attributes?["name"]?.Value;
        var hashString = root.Attributes?["encrypt"]?.Value;
        var hash = hashString == null ? null : Convert.FromBase64String(hashString);

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

        return new SettingsSection(name, values, sections, hash);
    }

    protected event Action? RereadEvent;

    protected void Update(SettingsSection section)
    {
        Values = section.Values;
        foreach (var settingsSection in Sections.Values)
        {
            settingsSection.Update((SettingsSection)section.GetSection(settingsSection.Name ?? ""));
        }
    }

    public static SettingsSection Empty(string? name = null) => new SettingsSection(name);

    protected virtual string? Encrypt(string? value) => value;
    protected virtual string? Decrypt(string? value) => value;
}
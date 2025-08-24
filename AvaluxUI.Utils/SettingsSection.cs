using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Xml;

namespace AvaluxUI.Utils;

public class SettingsSection : ISettingsSection
{
    internal Dictionary<string, string?> Values { get; private set; }
    internal Dictionary<string, SettingsSection> Sections { get; private set; }
    public string? Name { get; }
    private string? SecretKeyHash { get; }
    protected SettingsSection? Parent { get; }

    internal SettingsSection(SettingsSection? parent, string? name = null, Dictionary<string, string?>? dictionary = null,
        Dictionary<string, SettingsSection>? sections = null, string? secretKeyHash = null)
    {
        Parent = parent;
        Name = name;
        Values = dictionary ?? [];
        Sections = sections ?? [];
        SecretKeyHash = secretKeyHash;
    }

    private SettingsSection GetPublicSection(string key)
    {
        if (Sections.TryGetValue(key, out var section))
        {
            if (section.SecretKeyHash != null)
                throw new Exception("Section is encrypted");
            return section;
        }

        section = new SettingsSection(this, key, [], []);
        Sections.Add(key, section);
        return section;
    }

    public SettingsSection GetProtectedSection(string key, string secretKey)
    {
        if (Sections.TryGetValue(key, out var section))
        {
            if (!BCrypt.Net.BCrypt.Verify(secretKey, section.SecretKeyHash))
                throw new Exception("Secret key hash does not match secretKey hash");
            if (section is EncryptedSettingsSection encryptedSection)
                return encryptedSection;

            var newEncryptedSection =
                new EncryptedSettingsSection(this, secretKey, section.Name, section.Values, section.Sections);
            Sections[key] = newEncryptedSection;

            return newEncryptedSection;
        }

        section = new EncryptedSettingsSection(this, secretKey, key, [], []);
        Sections.Add(key, section);
        return section;
    }

    public async Task<ISettingsSection> GetSection(string key, string? secretKey = null)
    {
        await Reread();
        if (secretKey != null)
            return GetProtectedSection(key, secretKey);
        return GetPublicSection(key);
    }

    public async Task<bool> RemoveSection(string key)
    {
        var res = Sections.Remove(key);
        if (res)
            await Save();
        return res;
    }

    private async Task Set(string? key, string? value)
    {
        await Reread();
        if (key == null)
            return;
        Values[key] = Encrypt(value);
        await Save();
    }

    public Task Set(string? key, object? obj)
    {
        return Set(key, JsonSerializer.Serialize(obj));
    }

    public async Task<bool> Remove(string key)
    {
        await Reread();
        var res = Values.Remove(key);
        await Save();
        return res;
    }

    private string? Get(string key)
    {
        return Decrypt(Values.GetValueOrDefault(key));
    }

    public async Task<T> Get<T>(string key, T defaultValue)
    {
        var res = await Get<T>(key);
        return res ?? defaultValue;
    }

    public async Task<T?> Get<T>(string key)
    {
        await Reread();
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
            root.SetAttribute("encrypt", SecretKeyHash);
        foreach (var tag in ToXmlElements(document))
        {
            root.AppendChild(tag);
        }

        return root;
    }

    internal static SettingsSection FromXml(SettingsSection? parent, XmlNode root)
    {
        var name = root.Attributes?["name"]?.Value;
        var hash = root.Attributes?["encrypt"]?.Value;

        var values = root.SelectSingleNode("global")?.ChildNodes
            .Cast<XmlNode>()
            .Select(n => new KeyValuePair<string, string?>(n.Name, n.InnerText))
            .ToDictionary();

        var result = new SettingsSection(parent, name, values, [], hash);

        var sections = root.SelectNodes("section")?
            .Cast<XmlNode>()
            .Select(s => FromXml(result, s))
            .Select(s =>
                new KeyValuePair<string, SettingsSection>(s.Name ?? throw new Exception("Section name can not be null"),
                    s))
            .ToDictionary();
        result.Sections = sections ?? [];

        return result;
    }

    protected async Task Update(SettingsSection section)
    {
        Values = section.Values;
        foreach (var settingsSection in Sections.Values)
        {
            await settingsSection.Update((SettingsSection)(await section.GetSection(settingsSection.Name ?? "")));
        }
    }

    public static SettingsSection Empty(SettingsSection? parent = null, string? name = null) => new(parent, name);

    protected virtual string? Encrypt(string? value) => value;
    protected virtual string? Decrypt(string? value) => value;

    protected virtual async Task Save()
    {
        if (Parent != null)
            await Parent.Save();
    }

    protected virtual async Task Reread()
    {
        if (Parent != null)
            await Parent.Reread();
    }
}
using System.Xml;

namespace AvaluxUI.Utils;

public class SettingsFile : SettingsSection
{
    private readonly string _path;
    private bool _deleted;
    private DateTime _lastReadTime = DateTime.Now;

    private SettingsFile(string path, Dictionary<string, string?> global,
        Dictionary<string, SettingsSection> sections) : base(null, "Global", global, sections)
    {
        _path = path;
    }

    public static SettingsFile Open(string path)
    {
        var document = new XmlDocument();
        try
        {
            document.Load(path);
            var tmp = FromXml(null, document.SelectSingleNode("settings") ?? throw new XmlException());
            return new SettingsFile(path, tmp.Values, tmp.Sections);
        }
        catch (FileNotFoundException)
        {
        }
        catch (DirectoryNotFoundException)
        {
        }
        catch (XmlException)
        {
        }

        return new SettingsFile(path, [], []);
    }

    public void Delete()
    {
        _deleted = true;
        Sections.Clear();
        try
        {
            File.Delete(_path);
        }
        catch (DirectoryNotFoundException)
        {
        }
        catch (FileNotFoundException)
        {
        }
    }

    protected override async Task Save()
    {
        if (_deleted)
            return;
        var document = new XmlDocument();
        var xmlDeclaration = document.CreateXmlDeclaration("1.0", "UTF-8", null);
        document.AppendChild(xmlDeclaration);

        var root = document.CreateElement("settings");
        document.AppendChild(root);

        foreach (var tag in ToXmlElements(document))
        {
            root.AppendChild(tag);
        }

        if (!string.IsNullOrEmpty(Path.GetDirectoryName(_path)))
            Directory.CreateDirectory(Path.GetDirectoryName(_path) ?? ".");
        await File.WriteAllTextAsync(_path, document.OuterXml);
    }

    protected override async Task Reread()
    {
        if (_deleted || !File.Exists(_path) || File.GetLastWriteTime(_path) <= _lastReadTime)
            return;
        _lastReadTime = DateTime.Now;
        var document = new XmlDocument();
        try
        {
            var tmp = FromXml(null, document.SelectSingleNode("settings") ?? throw new XmlException());
            await Update(tmp);
        }
        catch (FileNotFoundException)
        {
        }
        catch (DirectoryNotFoundException)
        {
        }
        catch (XmlException)
        {
        }
    }
}
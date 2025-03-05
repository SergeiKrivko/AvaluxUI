using AvaluxUI.Utils;
using NUnit.Framework;

namespace AvaluxUI.Utils.Tests;

[TestFixture]
[TestOf(typeof(SettingsSection))]
public class SettingsSectionTest
{
    [Test]
    public void TestEncrypt()
    {
        var file = SettingsFile.Open("test.xml");
        var section = file.GetSection("Test", "12345678");
        section.Set("key", "value");

        Assert.That(section.Get<string>("key") == "value");
    }

    private record TestStruct(string Key, string Value, int Number);

    [Test]
    public void TestEncryptStruct()
    {
        var file = SettingsFile.Open("test.xml");
        var section = file.GetSection("Test", "12345678");
        section.Set("key", new TestStruct("KEY", "VALUE", 123));

        var result = section.Get<TestStruct>("key");
        Assert.That(result.Key == "KEY");
        Assert.That(result.Value == "VALUE");
        Assert.That(result.Number == 123);
    }
}
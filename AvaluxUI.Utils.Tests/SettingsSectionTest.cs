using System.Threading.Tasks;
using AvaluxUI.Utils;
using NUnit.Framework;

namespace AvaluxUI.Utils.Tests;

[TestFixture]
[TestOf(typeof(SettingsSection))]
public class SettingsSectionTest
{
    [Test]
    public async Task TestEncrypt()
    {
        var file = SettingsFile.Open("test.xml");
        var section = await file.GetSection("Test", "12345678");
        await section.Set("key", "value");

        Assert.That(await section.Get<string>("key") == "value");
    }

    private record TestStruct(string Key, string Value, int Number);

    [Test]
    public async Task TestEncryptStruct()
    {
        var file = SettingsFile.Open("test.xml");
        var section = await file.GetSection("Test", "12345678");
        await section.Set("key", new TestStruct("KEY", "VALUE", 123));

        var result = await section.Get<TestStruct>("key");
        Assert.That(result.Key == "KEY");
        Assert.That(result.Value == "VALUE");
        Assert.That(result.Number == 123);
    }
}
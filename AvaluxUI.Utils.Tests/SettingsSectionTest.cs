using System.IO;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AvaluxUI.Utils.Tests;

[TestFixture]
[TestOf(typeof(SettingsSection))]
public class SettingsSectionTest
{
    private static SettingsFile OpenFile(bool clearIfExists = true)
    {
        if (clearIfExists && File.Exists("test.xml"))
            File.Delete("test.xml");
        return SettingsFile.Open("test.xml");
    }

    [Test]
    public async Task TestSimple()
    {
        var file = OpenFile();
        await file.Set("key", "value");

        Assert.That(await file.Get<string>("key") == "value");
    }

    [Test]
    public async Task TestSection()
    {
        var file = OpenFile();
        var section = await file.GetSection("Test");
        await section.Set("key", "value");

        Assert.That(await section.Get<string>("key") == "value");
    }

    [Test]
    public async Task TestReopen()
    {
        var file = OpenFile();
        await file.Set("key", "value");

        file = OpenFile(false);
        Assert.That(await file.Get<string>("key") == "value");
    }

    [Test]
    public async Task TestReopenWithSection()
    {
        var file = OpenFile();
        var section = await file.GetSection("Test");
        await section.Set("key", "value");

        file = OpenFile(false);
        section = await file.GetSection("Test");
        Assert.That(await section.Get<string>("key") == "value");
    }

    [Test]
    public async Task TestEncrypt()
    {
        var file = OpenFile();
        var section = await file.GetSection("Test", "12345678");
        await section.Set("key", "value");

        Assert.That(await section.Get<string>("key") == "value");
    }

    private record TestStruct(string Key, string Value, int Number);

    [Test]
    public async Task TestSimpleStruct()
    {
        var file = OpenFile();
        var section = await file.GetSection("Test");
        await section.Set("key", new TestStruct("KEY", "VALUE", 123));

        var result = await section.Get<TestStruct>("key");
        Assert.That(result.Key == "KEY");
        Assert.That(result.Value == "VALUE");
        Assert.That(result.Number == 123);
    }

    [Test]
    public async Task TestEncryptStruct()
    {
        var file = OpenFile();
        var section = await file.GetSection("Test", "12345678");
        await section.Set("key", new TestStruct("KEY", "VALUE", 123));

        var result = await section.Get<TestStruct>("key");
        Assert.That(result.Key == "KEY");
        Assert.That(result.Value == "VALUE");
        Assert.That(result.Number == 123);
    }

    [Test]
    public async Task TestReopenEncrypt()
    {
        var file = OpenFile();
        var section = await file.GetSection("Test", "12345678");
        await section.Set("key", "value");

        file = OpenFile(false);
        section = await file.GetSection("Test", "12345678");
        Assert.That(await section.Get<string>("key") == "value");
    }

    [Test]
    public async Task TestUpdateWithSection()
    {
        var file = OpenFile();
        var section = await file.GetSection("Test");
        await section.Set("key", "value1");

        file = OpenFile(false);
        section = await file.GetSection("Test");
        await section.Set("key", "value2");

        file = OpenFile(false);
        section = await file.GetSection("Test");
        Assert.That(await section.Get<string>("key") == "value2");
    }

    [Test]
    public async Task TestCopyInMemory()
    {
        var file = OpenFile();
        await file.Set("key", "value");

        var memoryCopy = file.Clone();

        Assert.That(await memoryCopy.Get<string>("key") == "value");
    }

    [Test]
    public async Task TestCopyBack()
    {
        var file = OpenFile();
        await file.Set("key", "value1");

        var memoryCopy = file.Clone();
        await memoryCopy.Set("key", "value2");
        await file.Update(memoryCopy);

        Assert.That(await memoryCopy.Get<string>("key") == "value2");
    }

    [Test]
    public async Task TestCopyInMemoryWithSection()
    {
        var file = OpenFile();
        var section = await file.GetSection("Test");
        await section.Set("key", "value");

        var memoryCopy = file.Clone();
        section = await memoryCopy.GetSection("Test");

        Assert.That(await section.Get<string>("key") == "value");
    }

    [Test]
    public async Task TestCopyBackWithSection()
    {
        var file = OpenFile();
        var section = await file.GetSection("Test");
        await section.Set("key", "value1");

        var memoryCopy = file.Clone();
        section = await memoryCopy.GetSection("Test");
        await section.Set("key", "value2");
        await file.Update(memoryCopy);

        section = await file.GetSection("Test");
        Assert.That(await section.Get<string>("key") == "value2");
    }
}
namespace AvaluxUI.Utils.Sqlite;

public class SqliteSettings : ISettingsSection
{
    public string FilePath { get; set; }

    private SqliteSettings(string filePath)
    {
        FilePath = filePath;
    }

    public string? Name { get; }
    public event Action? Changed;
    public async Task<ISettingsSection> GetSection(string key, string? secretKey = null)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> RemoveSection(string key)
    {
        throw new NotImplementedException();
    }

    public async Task Set(string? key, object? obj)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> Remove(string key)
    {
        throw new NotImplementedException();
    }

    public async Task<T> Get<T>(string key, T defaultValue)
    {
        throw new NotImplementedException();
    }

    public async Task<T?> Get<T>(string key)
    {
        throw new NotImplementedException();
    }
}
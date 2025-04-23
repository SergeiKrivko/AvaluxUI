namespace AvaluxUI.Utils;

public interface ISettingsSection
{
    public string? Name { get; }

    public event Action? Changed;
    public Task<ISettingsSection> GetSection(string key, string? secretKey = null);
    public Task<bool> RemoveSection(string key);

    public Task Set(string? key, object? obj);
    public Task<bool> Remove(string key);

    public Task<T> Get<T>(string key, T defaultValue);
    public Task<T?> Get<T>(string key);
}
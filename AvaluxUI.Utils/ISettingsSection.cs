namespace AvaluxUI.Utils;

public interface ISettingsSection
{
    public string? Name { get; }

    public event Action? Changed;

    public ISettingsSection GetSection(string key);
    public ISettingsSection GetSection(string key, string secretKey);
    public bool RemoveSection(string key);

    public void Set(string? key, object? obj);
    public bool Remove(string key);

    public T Get<T>(string key, T defaultValue);
    public T? Get<T>(string key);
}
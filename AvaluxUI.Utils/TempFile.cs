namespace AvaluxUI.Utils;

public sealed class TempFile : IDisposable
{
    public string FilePath { get; private set; }

    public TempFile()
    {
        FilePath = Path.GetTempFileName();
    }

    public TempFile(string extension)
    {
        FilePath = Path.GetTempFileName() + extension;
    }

    ~TempFile()
    {
        Delete();
    }

    public void Dispose()
    {
        Delete();
        GC.SuppressFinalize(this);
    }

    private void Delete()
    {
        if (File.Exists(FilePath))
            File.Delete(FilePath);
    }

    public Stream OpenRead() => File.OpenRead(FilePath);
    public Stream OpenWrite() => File.OpenWrite(FilePath);

    public Task<string> ReadAllTextAsync() => File.ReadAllTextAsync(FilePath);
    public Task WriteAllTextAsync(string data) => File.WriteAllTextAsync(FilePath, data);
}
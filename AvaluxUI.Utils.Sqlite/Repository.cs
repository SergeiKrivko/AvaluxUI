using AvaluxUI.Utils.Sqlite.Entities;
using SQLite;

namespace AvaluxUI.Utils.Sqlite;

internal class Repository
{
    private readonly SQLiteAsyncConnection _database;

    public Repository(SQLiteAsyncConnection database)
    {
        _database = database;
    }

    public async Task InitAsync()
    {
        await _database.CreateTableAsync<SectionEntity>();
        await _database.CreateTableAsync<ValueEntity>();
    }

    public Task<SectionEntity> GetSection(string name)
    {
        return _database.Table<SectionEntity>().Where(e => e.Name == name).FirstOrDefaultAsync();
    }

    public async Task<Guid> AddSection(string name, string? secretKey)
    {
        var id = Guid.NewGuid();
        await _database.InsertOrReplaceAsync(new SectionEntity { Id = id, Name = name, SecretKey = secretKey });
        return id;
    }

    public async Task RemoveSection(string name)
    {
        await _database.DeleteAsync<SectionEntity>((await GetSection(name)).Id);
    }
}
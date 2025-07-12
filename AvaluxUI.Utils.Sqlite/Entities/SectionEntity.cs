namespace AvaluxUI.Utils.Sqlite.Entities;

internal class SectionEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? SecretKey { get; set; }
}
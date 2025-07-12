namespace AvaluxUI.Utils.Sqlite.Entities;

internal class ValueEntity
{
    public Guid Id { get; set; }
    public Guid SectionId { get; set; }
    public string Key { get; set; } = null!;
    public string? Value { get; set; }
}
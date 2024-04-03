namespace Sidekick.Apis.Poe.Metadatas.Models
{
    /// <summary>
    /// Items from /trade/data/items.
    /// </summary>
    public class ApiCategory
    {
        public string? Id { get; set; }
        public string? Label { get; set; }
        public List<ApiItem> Entries { get; set; } = new();

        public override string ToString() => $"{Label} - {Entries.Count} entries";
    }
}

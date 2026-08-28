namespace LostAndFound.Models
{
    public abstract class Item
    {
        public int ID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
        public string? ImagePath { get; set; }
        public ItemReportStatus Status { get; set; } = ItemReportStatus.Submitted;
    }

    public enum ItemReportStatus { Submitted, UnderReview, Matched, Claimed, Returned, Closed }
}

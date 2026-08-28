namespace LostAndFound.Models
{
    public class StatusHistory
    {
        public int ID { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int EntityID { get; set; }
        public string PreviousStatus { get; set; } = string.Empty;
        public string NewStatus { get; set; } = string.Empty;
        public int ChangedByUserID { get; set; }
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
        public string? Note { get; set; }
    }
}

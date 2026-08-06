

namespace LostAndFound.Models
{
    public class FoundItemReport
    {
        public int ReportId { get; set; }

        public Item Item { get; set; } = new();

        public DateTime DateFound { get; set; }

        public TimeSpan? ApproximateTimeFound { get; set; }

        public string LocationFound { get; set; } = string.Empty;

        public string StorageLocation { get; set; } = string.Empty;

        public string RegisteredByStaffId { get; set; } = string.Empty;

        public string Status { get; set; } = "Available";

        public DateTime DateReported { get; set; } = DateTime.Now;
    }
}

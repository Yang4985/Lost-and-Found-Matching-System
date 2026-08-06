
namespace LostAndFound.Models
{
    public class LostItemReport
    {
        public int ReportId { get; set; }

        public Item Item { get; set; } = new();

        public DateTime DateLost { get; set; }

        public TimeSpan? ApproximateTimeLost { get; set; }

        public string LocationLost { get; set; } = string.Empty;

        public string ReporterName { get; set; } = string.Empty;

        public string ContactEmail { get; set; } = string.Empty;

        public string Status { get; set; } = "Open";

        public DateTime DateReported { get; set; } = DateTime.Now;
    }
}

namespace LostAndFound.Models
{
    public class MatchResult
    {
        public int ID { get; set; }
        public int LostItemReportID { get; set; }
        public int FoundItemReportID { get; set; }
        public double Score { get; set; }
        public string Explanation { get; set; } = string.Empty;
        public MatchDecision Decision { get; set; } = MatchDecision.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? ReviewedByStaffID { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }

    public enum MatchDecision { Pending, Approved, Rejected }
}

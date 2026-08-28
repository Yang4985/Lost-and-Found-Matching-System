namespace LostAndFound.Models
{
    public class LostItemReport : Item
    {
        public DateTime DateLost { get; set; }
        public string DistinguishingFeatures { get; set; } = string.Empty;
        public int ReportedByStudentID { get; set; }
    }
}

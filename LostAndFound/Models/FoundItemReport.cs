namespace LostAndFound.Models
{
    public class FoundItemReport : Item
    {
        public DateTime DateFound { get; set; }
        public string StorageLocation { get; set; } = string.Empty;
        public int RegisteredByStaffID { get; set; }
    }
}

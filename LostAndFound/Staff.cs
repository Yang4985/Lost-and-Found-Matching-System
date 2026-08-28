namespace LostAndFound.Models
{
    public class Staff : User
    {
        public string StaffNumber { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
    }
}

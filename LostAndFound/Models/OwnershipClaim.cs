namespace LostAndFound.Models
{
    public class OwnershipClaim
    {
        public int ID { get; set; }
        public int MatchResultID { get; set; }
        public int ClaimantStudentID { get; set; }
        public string Evidence { get; set; } = string.Empty;
        public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public int? ReviewedByStaffID { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }

    public enum ClaimStatus { Submitted, UnderReview, Approved, Rejected }
}

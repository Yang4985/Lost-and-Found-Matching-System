namespace LostAndFound.Core.Models
{
    public class FoundItemReport
    {
        public int ReportId { get; set; }

        public Item Item { get; set; } = new Item();

        public DateTime DateFound { get; set; }

        public TimeSpan? ApproximateTimeFound { get; set; }

        public string LocationFound { get; set; } = string.Empty;

        public string StorageLocation { get; set; } = string.Empty;

        public int RegisteredByStaffID { get; set; }

        public ItemReportStatus Status { get; set; }
            = ItemReportStatus.Submitted;

        public DateTime DateReported { get; set; }
            = DateTime.Now;

        // Compatibility properties for existing project code

        public int ID
        {
            get => ReportId;
            set => ReportId = value;
        }

        public string Name
        {
            get => Item.GetName();
            set => Item.SetName(value);
        }

        public string Category
        {
            get => Item.GetCategory();
            set => Item.SetCategory(value);
        }

        public string Description
        {
            get => Item.GetDescription();
            set => Item.SetDescription(value);
        }

        public string Location
        {
            get => LocationFound;
            set => LocationFound = value;
        }

        public string DistinguishingFeatures
        {
            get => Item.GetDistinguishingFeatures();
            set => Item.SetDistinguishingFeatures(value);
        }

        public DateTime ReportedAt
        {
            get => DateReported;
            set => DateReported = value;
        }
    }
}
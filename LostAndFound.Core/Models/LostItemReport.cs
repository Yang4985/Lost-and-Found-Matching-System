namespace LostAndFound.Core.Models
{
    public class LostItemReport
    {
        public int ReportId { get; set; }

        public Item Item { get; set; } = new Item();

        public DateTime DateLost { get; set; }

        public TimeSpan? ApproximateTimeLost { get; set; }

        public string LocationLost { get; set; } = string.Empty;

        public string ReporterName { get; set; } = string.Empty;

        public string ContactEmail { get; set; } = string.Empty;

        public int ReportedByStudentID { get; set; }

        public ItemReportStatus Status { get; set; }
            = ItemReportStatus.Submitted;

        public DateTime DateReported { get; set; }
            = DateTime.Now;


        // Compatibility properties for existing code

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
            get => LocationLost;
            set => LocationLost = value;
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
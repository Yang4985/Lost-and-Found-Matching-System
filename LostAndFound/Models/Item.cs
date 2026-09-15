
﻿

namespace LostAndFound.Models
{
    public class Item
    {
        private string brand;
        private string name;
        private string description;
        private string color;

        public Item()
        {
            brand = string.Empty;
            name = string.Empty;
            description = string.Empty;
            color = string.Empty;
        }

        public Item(string brand, string name, string description, string color)
        {
            this.brand = brand;
            this.name = name;
            this.description = description;
            this.color = color;
        }

        public string getBrand() { return brand; }
        public string getName() { return name; }
        public string getDescription() { return description; }
        public string getColor() { return color; }

        public void setBrand(string brand) { this.brand = brand; }
        public void setName(string name) { this.name = name; }
        public void setDescription(string description)
        {
            this.description = description;
        }
        public void setColor(string color) { this.color = color; }



    }


    namespace LostAndFound.Models
    {
        public abstract class Item
        {
            public int ID { get; set; }
            public string Name { get; set; } = string.Empty;
            public string Category { get; set; } = string.Empty;
            public string Description { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public DateTime ReportedAt { get; set; } = DateTime.UtcNow;
            public string? ImagePath { get; set; }
            public ItemReportStatus Status { get; set; } = ItemReportStatus.Submitted;
        }

        public enum ItemReportStatus { Submitted, UnderReview, Matched, Claimed, Returned, Closed }
    }
}

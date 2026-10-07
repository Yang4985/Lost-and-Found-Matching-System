namespace LostAndFound.Models
{
    public class Item
    {
        private string category;
        private string brand;
        private string name;
        private string description;
        private string color;
        private string distinguishingFeatures;

        // Serializable properties retain all matching fields across app restarts.
        public string Category { get => category; set => category = value; }
        public string Brand { get => brand; set => brand = value; }
        public string Name { get => name; set => name = value; }
        public string Description { get => description; set => description = value; }
        public string Color { get => color; set => color = value; }
        public string DistinguishingFeatures { get => distinguishingFeatures; set => distinguishingFeatures = value; }

        // Default constructor
        public Item()
        {
            category = string.Empty;
            brand = string.Empty;
            name = string.Empty;
            description = string.Empty;
            color = string.Empty;
            distinguishingFeatures = string.Empty;
        }

        // Constructor with values
        public Item(
            string category,
            string brand,
            string name,
            string description,
            string color,
            string distinguishingFeatures)
        {
            this.category = category;
            this.brand = brand;
            this.name = name;
            this.description = description;
            this.color = color;
            this.distinguishingFeatures = distinguishingFeatures;
        }

        // Getters
        public string GetCategory()
        {
            return category;
        }

        public string GetBrand()
        {
            return brand;
        }

        public string GetName()
        {
            return name;
        }

        public string GetDescription()
        {
            return description;
        }

        public string GetColor()
        {
            return color;
        }

        public string GetDistinguishingFeatures()
        {
            return distinguishingFeatures;
        }

        // Setters
        public void SetCategory(string category)
        {
            this.category = category;
        }

        public void SetBrand(string brand)
        {
            this.brand = brand;
        }

        public void SetName(string name)
        {
            this.name = name;
        }

        public void SetDescription(string description)
        {
            this.description = description;
        }

        public void SetColor(string color)
        {
            this.color = color;
        }

        public void SetDistinguishingFeatures(string distinguishingFeatures)
        {
            this.distinguishingFeatures = distinguishingFeatures;
        }
    }
}

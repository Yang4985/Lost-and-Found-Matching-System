using NUnit.Framework;
using LostAndFound.Core.Models;
using LostAndFound.Core.Services;

namespace LostAndFound.Tests
{
    public class MatchingServiceTests
    {
        [Test]
        public void MatchingItems_ShouldReturnHighScore()
        {
            Item lostItem = new Item(
                "Phone",
                "Samsung",
                "Galaxy S24",
                "Black Samsung phone with cracked screen",
                "Black",
                "Crack on bottom right"
            );

            Item foundItem = new Item(
                "Phone",
                "Samsung",
                "Galaxy S24",
                "Black Samsung phone with cracked screen",
                "Black",
                "Crack near bottom right"
            );

            LostItemReport lostReport = new LostItemReport
            {
                Item = lostItem,
                DateLost = DateTime.Today,
                LocationLost = "Library"
            };

            FoundItemReport foundReport = new FoundItemReport
            {
                Item = foundItem,
                DateFound = DateTime.Today,
                LocationFound = "Library"
            };

            MatchingService service = new MatchingService();

            int score = service.CalculateMatchScore(
                lostReport,
                foundReport
            );

            Assert.That(score, Is.GreaterThanOrEqualTo(80));
        }
    }
}
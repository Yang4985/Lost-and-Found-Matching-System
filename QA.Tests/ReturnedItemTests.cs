using LostAndFound.Models;
using LostAndFound.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LostAndFound.QA;

[TestClass]
public class ReturnedItemTests
{
    [TestMethod]
    public void MarkFoundReportReturned_ShouldSetStatusToReturned()
    {
        var service = new PrototypeDataService();

        var report = new FoundItemReport
        {
            Name = "Samsung Phone",
            Category = "Electronics",
            Location = "WG403",
            Description = "Black Samsung phone",
            StorageLocation = "WZ Building",
            DateFound = DateTime.Today
        };

        service.AddFoundReport(report);

        service.MarkFoundReportReturned(report.ID);

        Assert.AreEqual(
            ItemReportStatus.Returned,
            report.Status
        );
    }


    [TestMethod]
    public void MarkFoundReportReturned_ShouldSetReturnedTimestamp()
    {
        var service = new PrototypeDataService();

        var report = new FoundItemReport
        {
            Name = "Wallet",
            Category = "Wallet",
            Location = "Library",
            Description = "Black leather wallet",
            StorageLocation = "Security Office",
            DateFound = DateTime.Today
        };

        service.AddFoundReport(report);

        service.MarkFoundReportReturned(report.ID);

        Assert.IsNotNull(report.ReturnedAt);
    }


    [TestMethod]
    public void ReturnedStatus_ShouldNotBeEligibleForMatching()
    {
        bool eligible =
            PrototypeDataService.IsEligible(
                ItemReportStatus.Returned
            );

        Assert.IsFalse(eligible);
    }


    [TestMethod]
    public void ReturnedStatus_ShouldPersistAfterReload()
    {
        string filePath = Path.Combine(
            Path.GetTempPath(),
            $"lostfound-test-{Guid.NewGuid()}.json"
        );

        try
        {
            var service =
                new PrototypeDataService(filePath);

            var report = new FoundItemReport
            {
                Name = "Backpack",
                Category = "Bag",
                Location = "Library",
                Description = "Black backpack",
                StorageLocation = "Security Office",
                DateFound = DateTime.Today
            };

            service.AddFoundReport(report);

            service.MarkFoundReportReturned(report.ID);

            // Simulates closing and reopening the app.
            var reloadedService =
                new PrototypeDataService(filePath);

            var reloadedReport =
                reloadedService.FoundReports
                    .First(r => r.ID == report.ID);

            Assert.AreEqual(
                ItemReportStatus.Returned,
                reloadedReport.Status
            );

            Assert.IsNotNull(
                reloadedReport.ReturnedAt
            );
        }
        finally
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
    }
}
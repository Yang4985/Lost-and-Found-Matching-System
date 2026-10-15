using LostAndFound.Models;
using LostAndFound.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.Json;

namespace LostAndFound.QA;

[TestClass]
public class FixRegressionTests
{
    [TestMethod]
    [DataRow(ItemReportStatus.Submitted, true)]
    [DataRow(ItemReportStatus.Available, true)]
    [DataRow(ItemReportStatus.UnderReview, true)]
    [DataRow(ItemReportStatus.Matched, false)]
    [DataRow(ItemReportStatus.Claimed, false)]
    [DataRow(ItemReportStatus.Returned, false)]
    [DataRow(ItemReportStatus.Closed, false)]
    public void TC19_StatusEligibility_AppliesToBothSides(ItemReportStatus status, bool eligible)
    {
        foreach (var changeLost in new[] { true, false })
        {
            var data = MatchingTests.Empty(); var lost = MatchingTests.Lost(); var found = MatchingTests.Found();
            data.AddLostReport(lost); data.AddFoundReport(found);
            if (changeLost) lost.Status = status; else found.Status = status;
            Assert.HasCount(eligible ? 1 : 0, data.GenerateMatches());
        }
    }

    [TestMethod]
    public void TC20_UI_ScoreEqualsProductionScorer()
    {
        var data = MatchingTests.Empty(); var lost = MatchingTests.Lost();
        data.AddLostReport(lost);
        foreach (var offset in new[] { -2, -1, 0, 1, 2 })
        {
            var found = MatchingTests.Found(); found.DateFound = found.DateFound.AddDays(offset);
            if (offset == 2) found.Item.SetBrand("");
            data.AddFoundReport(found);
        }
        var matches = data.GenerateMatches(); Assert.HasCount(5, matches);
        foreach (var match in matches)
            Assert.AreEqual(new MatchingService().CalculateMatchScore(lost, match.FoundReport), match.Score);
        Assert.AreEqual("Match score: 100/100", matches[0].ScoreText);
    }

    [TestMethod]
    public void TC21_Restart_PreservesFieldsStatusesAndNextIds()
    {
        WithStore(path =>
        {
            var data = new PrototypeDataService(path);
            var lost = MatchingTests.Lost(); lost.ReporterName = "QA"; lost.ContactEmail = "qa@example.test";
            lost.ApproximateTimeLost = new TimeSpan(12, 30, 0);
            var found = MatchingTests.Found(); found.StorageLocation = "Security";
            data.AddLostReport(lost); data.AddFoundReport(found);
            found.Status = ItemReportStatus.Returned; data.Save();
            var reopened = new PrototypeDataService(path);
            var actual = reopened.LostReports.Single();
            Assert.AreEqual(JsonSerializer.Serialize(lost), JsonSerializer.Serialize(actual));
            Assert.AreEqual(JsonSerializer.Serialize(found), JsonSerializer.Serialize(reopened.FoundReports.Single()));
            Assert.HasCount(0, reopened.GenerateMatches());
            var nextLost = MatchingTests.Lost(); var nextFound = MatchingTests.Found();
            reopened.AddLostReport(nextLost); reopened.AddFoundReport(nextFound);
            Assert.AreEqual(lost.ID + 1, nextLost.ID); Assert.AreEqual(found.ID + 1, nextFound.ID);
            var secondRestart = new PrototypeDataService(path);
            Assert.HasCount(2, secondRestart.LostReports); Assert.HasCount(2, secondRestart.FoundReports);
        });
    }

    [TestMethod]
    public void TC22_CorruptStore_IsNotSilentlyOverwritten()
    {
        WithStore(path =>
        {
            File.WriteAllText(path, "{broken");
            Assert.ThrowsExactly<JsonException>(() => new PrototypeDataService(path));
            Assert.AreEqual("{broken", File.ReadAllText(path));
        });
    }

    [TestMethod]
    public void TC23_SaveFailure_DoesNotAddUnsavedReports()
    {
        WithStore(path =>
        {
            File.WriteAllText(path, "blocking parent");
            var data = new PrototypeDataService(Path.Combine(path, "reports.json"));
            Assert.ThrowsExactly<IOException>(() => data.AddLostReport(MatchingTests.Lost()));
            Assert.ThrowsExactly<IOException>(() => data.AddFoundReport(MatchingTests.Found()));
            Assert.HasCount(0, data.LostReports); Assert.HasCount(0, data.FoundReports);
            Assert.AreEqual("blocking parent", File.ReadAllText(path));
        });
    }

    [TestMethod]
    public void TC24_NewStore_StartsWithoutFakeReports()
    {
        WithStore(path =>
        {
            var data = new PrototypeDataService(path);
            Assert.HasCount(0, data.LostReports); Assert.HasCount(0, data.FoundReports);
        });
    }

    private static void WithStore(Action<string> action)
    {
        var path = Path.Combine(Path.GetTempPath(), "lost-found-qa-" + Guid.NewGuid() + ".json");
        try { action(path); }
        finally { if (File.Exists(path)) File.Delete(path); }
    }
}

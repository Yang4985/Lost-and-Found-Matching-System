using LostAndFound.Models;
using LostAndFound.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel)]

namespace LostAndFound.QA;

[TestClass]
public class MatchingTests
{
    internal static LostItemReport Lost() => new()
    {
        Item = new Item("Electronics", "Samsung", "Galaxy", "black cracked screen", "Black", "scratch"),
        LocationLost = "Library", DateLost = new DateTime(2026, 10, 1)
    };
    internal static FoundItemReport Found() => new()
    {
        Item = new Item("Electronics", "Samsung", "Galaxy", "black cracked screen", "Black", "scratch"),
        LocationFound = "Library", DateFound = new DateTime(2026, 10, 1)
    };
    internal static PrototypeDataService Empty()
    {
        var data = new PrototypeDataService();
        data.LostReports.Clear(); data.FoundReports.Clear();
        return data;
    }

    [TestMethod]
    public void TC01_AllSignalsMatch_Score100()
    {
        var service = new MatchingService(); // Arrange
        var actual = service.CalculateMatchScore(Lost(), Found()); // Act
        Assert.AreEqual(100, actual); // Assert
    }

    [TestMethod]
    [DataRow("Wallet")]
    [DataRow("")]
    [DataRow(" ")]
    public void TC02_CategoryMismatchOrMissing_ScoreZero(string category)
    {
        var found = Found(); found.Category = category;
        var actual = new MatchingService().CalculateMatchScore(Lost(), found);
        Assert.AreEqual(0, actual);
    }

    [TestMethod]
    public void TC03_CaseAndWhitespace_DoNotChangeExactFields()
    {
        var found = Found(); found.Category = " electronics ";
        found.Item.SetBrand(" SAMSUNG "); found.Item.SetColor(" black "); found.Location = " LIBRARY ";
        var actual = new MatchingService().CalculateMatchScore(Lost(), found);
        Assert.AreEqual(100, actual);
    }

    [TestMethod]
    [DataRow(-2, 85)] [DataRow(-1, 100)] [DataRow(0, 100)]
    [DataRow(1, 100)] [DataRow(2, 85)]
    public void TC04_DateBoundary_UsesOneDayWindow(int offset, int expected)
    {
        var found = Found(); found.DateFound = new DateTime(2026, 10, 1).AddDays(offset);
        var actual = new MatchingService().CalculateMatchScore(Lost(), found);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [DataRow(0, "Not Recommended")] [DataRow(39, "Not Recommended")]
    [DataRow(40, "Low")] [DataRow(59, "Low")]
    [DataRow(60, "Medium")] [DataRow(79, "Medium")]
    [DataRow(80, "High")] [DataRow(100, "High")]
    public void TC05_RecommendationThresholds(int score, string expected)
    {
        var service = new MatchingService();
        var actual = service.GetMatchLevel(score);
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [DataRow(true)] [DataRow(false)]
    public void TC06_NullReport_ScoreZero(bool missingLost)
    {
        var service = new MatchingService();
        var actual = service.CalculateMatchScore(missingLost ? null! : Lost(), missingLost ? Found() : null!);
        Assert.AreEqual(0, actual);
    }

    [TestMethod]
    [DataRow(true)] [DataRow(false)]
    public void TC07_NullItem_ScoreZero(bool missingLost)
    {
        var lost = Lost(); var found = Found();
        if (missingLost) lost.Item = null!; else found.Item = null!;
        var actual = new MatchingService().CalculateMatchScore(lost, found);
        Assert.AreEqual(0, actual);
    }

    [TestMethod]
    public void TC08_EmptyDescriptions_NoKeywordBonus()
    {
        var lost = Lost(); var found = Found(); lost.Description = ""; found.Description = "";
        var actual = new MatchingService().CalculateMatchScore(lost, found);
        Assert.AreEqual(90, actual);
    }

    [TestMethod]
    public void TC09_RepeatedKeywords_DoNotExceed100()
    {
        var lost = Lost(); lost.Description = "black black black screen screen";
        var actual = new MatchingService().CalculateMatchScore(lost, Found());
        Assert.AreEqual(100, actual);
    }

    [TestMethod]
    public void TC10_EmptyCollections_NoMatches()
    {
        var data = Empty();
        var actual = data.GenerateMatches();
        Assert.HasCount(0, actual);
    }

    [TestMethod]
    [DataRow(true)] [DataRow(false)]
    public void TC11_ClosedReports_Excluded(bool closeLost)
    {
        var data = Empty(); var lost = Lost(); var found = Found();
        data.AddLostReport(lost); data.AddFoundReport(found);
        if (closeLost) lost.Status = ItemReportStatus.Closed; else found.Status = ItemReportStatus.Closed;
        var actual = data.GenerateMatches();
        Assert.HasCount(0, actual);
    }

    [TestMethod]
    public void TC12_AddLost_AssignsDistinctIdsAndSubmittedStatus()
    {
        var data = Empty(); var first = Lost(); var second = Lost();
        data.AddLostReport(first); data.AddLostReport(second);
        Assert.AreNotEqual(first.ID, second.ID);
        Assert.AreEqual(ItemReportStatus.Submitted, second.Status);
        Assert.HasCount(2, data.LostReports);
    }

    [TestMethod]
    public void TC13_AddFound_AssignsDistinctIdsAndSubmittedStatus()
    {
        var data = Empty(); var first = Found(); var second = Found();
        data.AddFoundReport(first); data.AddFoundReport(second);
        Assert.AreNotEqual(first.ID, second.ID);
        Assert.AreEqual(ItemReportStatus.Submitted, second.Status);
        Assert.HasCount(2, data.FoundReports);
    }

    [TestMethod]
    public void TC14_UIResults_SortedWithExplanations()
    {
        var data = Empty(); data.AddLostReport(Lost()); data.AddFoundReport(Found());
        var weak = Found(); weak.Location = "Gym"; weak.Description = "unrelated"; weak.Name = "Different";
        data.AddFoundReport(weak);
        var results = data.GenerateMatches();
        Assert.HasCount(2, results);
        Assert.IsGreaterThan(results[1].Score, results[0].Score);
        Assert.IsFalse(string.IsNullOrWhiteSpace(results[0].Explanation));
    }

    [TestMethod]
    public void TC15_RepeatedMatching_DoesNotMutateReports()
    {
        var data = Empty(); var lost = Lost(); var found = Found();
        data.AddLostReport(lost); data.AddFoundReport(found);
        var first = data.GenerateMatches(); var second = data.GenerateMatches();
        Assert.AreEqual(first[0].Score, second[0].Score);
        Assert.AreEqual(ItemReportStatus.Submitted, lost.Status);
        Assert.AreEqual(ItemReportStatus.Submitted, found.Status);
        Assert.HasCount(1, data.LostReports); Assert.HasCount(1, data.FoundReports);
    }

    // These are deliberate acceptance failures for OPEN defects, not skipped tests.
    [TestMethod, TestCategory("OpenDefect")]
    public void TC16_D001_UIAndCore_UseSameCategoryExclusion()
    {
        var data = Empty(); var lost = Lost(); var found = Found(); found.Category = "Wallet";
        data.AddLostReport(lost); data.AddFoundReport(found);
        Assert.AreEqual(0, new MatchingService().CalculateMatchScore(lost, found));
        Assert.HasCount(0, data.GenerateMatches(), "D001: UI still recommends a different-category item.");
    }

    [TestMethod, TestCategory("OpenDefect")]
    public void TC17_D002_ReturnedFoundItem_IsNotAvailableForMatching()
    {
        var data = Empty(); var found = Found();
        data.AddLostReport(Lost()); data.AddFoundReport(found); found.Status = ItemReportStatus.Returned;
        Assert.HasCount(0, data.GenerateMatches(), "D002: Returned item remains a candidate.");
    }

    [TestMethod, TestCategory("OpenDefect")]
    public void TC18_D003_RecreatedService_RetainsSavedReport()
    {
        var data = Empty(); var lost = Lost(); lost.Name = "QA-PERSISTENCE-UNIQUE-2026";
        data.AddLostReport(lost);
        var recreated = new PrototypeDataService();
        Assert.IsTrue(recreated.LostReports.Any(r => r.Name == lost.Name), "D003: Registered report is not persistent.");
    }
}


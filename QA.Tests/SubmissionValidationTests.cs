using LostAndFound.Models;
using LostAndFound.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LostAndFound.QA;

[TestClass]
public class SubmissionValidationTests
{
    private static LostItemReport Lost() => new()
    {
        Name = "Phone", Category = "Electronics", Location = "Library",
        Description = "Black phone", DateLost = DateTime.Today
    };
    private static FoundItemReport Found() => new()
    {
        Name = "Phone", Category = "Electronics", Location = "Library",
        Description = "Black phone", StorageLocation = "Security desk", DateFound = DateTime.Today
    };
    private static string TempPath() => Path.Combine(Path.GetTempPath(), $"submission-{Guid.NewGuid():N}.json");

    [TestMethod]
    public void Lost_MultipleMissingFields_ReportsEveryErrorTogether()
    {
        var report = new LostItemReport { DateLost = DateTime.Today.AddDays(1) };
        var errors = new ValidationService().GetLostReportErrors(report);
        CollectionAssert.AreEquivalent(new[] { "Item name is required.", "Category is required.",
            "Location is required.", "Description is required.", "Lost date cannot be in the future." }, errors.ToArray());
    }

    [TestMethod]
    public void Found_MultipleMissingFields_IncludesStorageAndDate()
    {
        var report = new FoundItemReport { DateFound = DateTime.Today.AddDays(1) };
        var errors = new ValidationService().GetFoundReportErrors(report);
        CollectionAssert.AreEquivalent(new[] { "Item name is required.", "Category is required.",
            "Location is required.", "Description is required.", "Storage location is required.",
            "Found date cannot be in the future." }, errors.ToArray());
    }

    [TestMethod]
    public void Lost_BooleanApi_IncludesAllErrorsInVisibleMessage()
    {
        var report = Lost(); report.Name = ""; report.Description = "";
        Assert.IsFalse(new ValidationService().ValidateLostReport(report, out var message));
        Assert.AreEqual("Item name is required." + Environment.NewLine + "Description is required.", message);
    }

    [TestMethod]
    public void Found_BooleanApi_IncludesEveryMissingField()
    {
        var report = Found(); report.Category = ""; report.StorageLocation = "";
        Assert.IsFalse(new ValidationService().ValidateFoundReport(report, out var message));
        Assert.AreEqual("Category is required." + Environment.NewLine + "Storage location is required.", message);
    }

    [TestMethod] [DataRow("")] [DataRow("   ")] [DataRow(null)]
    public void Lost_MissingDescription_IsRejected(string? description)
    {
        var report = Lost(); report.Description = description!;
        Assert.IsFalse(new ValidationService().ValidateLostReport(report, out var error));
        Assert.AreEqual("Description is required.", error);
    }

    [TestMethod] [DataRow("")] [DataRow("   ")] [DataRow(null)]
    public void Found_MissingStorageLocation_IsRejected(string? location)
    {
        var report = Found(); report.StorageLocation = location!;
        Assert.IsFalse(new ValidationService().ValidateFoundReport(report, out var error));
        Assert.AreEqual("Storage location is required.", error);
    }

    [TestMethod]
    public void Found_MissingDescription_IsRejected()
    {
        var report = Found(); report.Description = "";
        Assert.IsFalse(new ValidationService().ValidateFoundReport(report, out var error));
        Assert.AreEqual("Description is required.", error);
    }

    [TestMethod]
    public void Submit_NullReports_ReturnsFailureWithoutMutation()
    {
        var service = new PrototypeDataService();
        Assert.IsFalse(service.TrySubmitLostReport(null, out var lostError));
        Assert.IsFalse(service.TrySubmitFoundReport(null, out var foundError));
        Assert.AreEqual("Lost report is required.", lostError);
        Assert.AreEqual("Found report is required.", foundError);
        Assert.IsEmpty(service.LostReports); Assert.IsEmpty(service.FoundReports);
    }

    [TestMethod]
    public void Submit_InvalidLostReport_DoesNotWriteOrAssignIdentity()
    {
        string path = TempPath();
        try
        {
            var service = new PrototypeDataService(path); var report = Lost(); report.Name = "";
            Assert.IsFalse(service.TrySubmitLostReport(report, out _));
            Assert.IsEmpty(service.LostReports); Assert.AreEqual(0, report.ID);
            Assert.IsFalse(File.Exists(path));
            report.Name = "Corrected phone";
            Assert.IsTrue(service.TrySubmitLostReport(report, out var error)); Assert.AreEqual("", error);
            Assert.AreEqual(1, report.ID);
            var reloaded = new PrototypeDataService(path);
            Assert.HasCount(1, reloaded.LostReports);
            Assert.AreEqual("Corrected phone", reloaded.LostReports[0].Name);
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [TestMethod]
    public void Submit_InvalidFoundReport_PreservesExistingFileAndData()
    {
        string path = TempPath();
        try
        {
            var service = new PrototypeDataService(path);
            Assert.IsTrue(service.TrySubmitFoundReport(Found(), out _));
            byte[] original = File.ReadAllBytes(path);
            var invalid = Found(); invalid.StorageLocation = " "; invalid.Description = "";
            Assert.IsFalse(service.TrySubmitFoundReport(invalid, out var message));
            StringAssert.Contains(message, "Storage location"); StringAssert.Contains(message, "Description");
            CollectionAssert.AreEqual(original, File.ReadAllBytes(path));
            Assert.HasCount(1, service.FoundReports); Assert.AreEqual(0, invalid.ID);
            Assert.IsTrue(service.TrySubmitFoundReport(Found(), out _));
            Assert.AreEqual(2, service.FoundReports.Max(r => r.ID));
        }
        finally { if (File.Exists(path)) File.Delete(path); }
    }

    [TestMethod]
    public void Submit_PublicFoundReport_PendingIntakeRemainsValid()
    {
        var report = Found(); report.StorageLocation = "Pending staff intake"; report.RegisteredByStaffID = 0;
        var service = new PrototypeDataService();
        Assert.IsTrue(service.TrySubmitFoundReport(report, out var error)); Assert.AreEqual("", error);
        Assert.HasCount(1, service.FoundReports);
        Assert.AreEqual("Pending staff intake", service.FoundReports[0].StorageLocation);
    }

    [TestMethod]
    public void Submit_OptionalFieldsAndUnicodeNames_AreAccepted()
    {
        var report = Lost(); report.Name = "K膩kahu"; report.Item.Brand = ""; report.Item.Color = "";
        Assert.IsTrue(new PrototypeDataService().TrySubmitLostReport(report, out var error));
        Assert.AreEqual("", error);
    }

    [TestMethod]
    public void Validate_CorrectedReport_DoesNotRetainPreviousErrors()
    {
        var validator = new ValidationService(); var report = Lost(); report.Name = "";
        Assert.IsFalse(validator.ValidateLostReport(report, out _)); report.Name = "Phone";
        Assert.IsTrue(validator.ValidateLostReport(report, out var message)); Assert.AreEqual("", message);
    }

    [TestMethod]
    public void Submit_WriteFailure_RetainsInputAndRollsBackInsertion()
    {
        string blocker = TempPath();
        try
        {
            File.WriteAllText(blocker, "not a directory");
            var service = new PrototypeDataService(Path.Combine(blocker, "reports.json"));
            var report = Lost();
            Assert.ThrowsExactly<IOException>(() => service.TrySubmitLostReport(report, out _));
            Assert.IsEmpty(service.LostReports); Assert.AreEqual("Phone", report.Name);
            Assert.AreEqual("Black phone", report.Description);
            Assert.AreEqual("not a directory", File.ReadAllText(blocker));
        }
        finally { if (File.Exists(blocker)) File.Delete(blocker); }
    }
}

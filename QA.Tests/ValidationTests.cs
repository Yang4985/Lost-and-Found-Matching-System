using LostAndFound.Models;
using LostAndFound.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LostAndFound.QA;

[TestClass]
public class ValidationTests
{
    private static LostItemReport CreateValidLostReport()
    {
        return new LostItemReport
        {
            Name = "Samsung Phone",
            Category = "Electronics",
            Location = "Library",
            Description = "Black Samsung phone",
            DateLost = DateTime.Today
        };
    }


    private static FoundItemReport CreateValidFoundReport()
    {
        return new FoundItemReport
        {
            Name = "Samsung Phone",
            Category = "Electronics",
            Location = "Library",
            Description = "Black Samsung phone",
            StorageLocation = "Security Office",
            DateFound = DateTime.Today
        };
    }


    [TestMethod]
    public void ValidLostReport_ShouldPassValidation()
    {
        var service = new ValidationService();
        var report = CreateValidLostReport();

        bool result =
            service.ValidateLostReport(
                report,
                out string errorMessage);

        Assert.IsTrue(result);
        Assert.AreEqual(string.Empty, errorMessage);
    }


    [TestMethod]
    public void LostReport_WithMissingName_ShouldFailValidation()
    {
        var service = new ValidationService();
        var report = CreateValidLostReport();

        report.Name = string.Empty;

        bool result =
            service.ValidateLostReport(
                report,
                out string errorMessage);

        Assert.IsFalse(result);
        Assert.AreEqual(
            "Item name is required.",
            errorMessage);
    }


    [TestMethod]
    public void LostReport_WithWhitespaceLocation_ShouldFailValidation()
    {
        var service = new ValidationService();
        var report = CreateValidLostReport();

        report.Location = "   ";

        bool result =
            service.ValidateLostReport(
                report,
                out string errorMessage);

        Assert.IsFalse(result);
        Assert.AreEqual(
            "Location is required.",
            errorMessage);
    }


    [TestMethod]
    public void LostReport_WithFutureDate_ShouldFailValidation()
    {
        var service = new ValidationService();
        var report = CreateValidLostReport();

        report.DateLost =
            DateTime.Today.AddDays(1);

        bool result =
            service.ValidateLostReport(
                report,
                out string errorMessage);

        Assert.IsFalse(result);
        Assert.AreEqual(
            "Lost date cannot be in the future.",
            errorMessage);
    }


    [TestMethod]
    public void ValidFoundReport_ShouldPassValidation()
    {
        var service = new ValidationService();
        var report = CreateValidFoundReport();

        bool result =
            service.ValidateFoundReport(
                report,
                out string errorMessage);

        Assert.IsTrue(result);
        Assert.AreEqual(string.Empty, errorMessage);
    }


    [TestMethod]
    public void FoundReport_WithMissingCategory_ShouldFailValidation()
    {
        var service = new ValidationService();
        var report = CreateValidFoundReport();

        report.Category = string.Empty;

        bool result =
            service.ValidateFoundReport(
                report,
                out string errorMessage);

        Assert.IsFalse(result);
        Assert.AreEqual(
            "Category is required.",
            errorMessage);
    }


    [TestMethod]
    public void FoundReport_WithWhitespaceName_ShouldFailValidation()
    {
        var service = new ValidationService();
        var report = CreateValidFoundReport();

        report.Name = "   ";

        bool result =
            service.ValidateFoundReport(
                report,
                out string errorMessage);

        Assert.IsFalse(result);
        Assert.AreEqual(
            "Item name is required.",
            errorMessage);
    }


    [TestMethod]
    public void FoundReport_WithFutureDate_ShouldFailValidation()
    {
        var service = new ValidationService();
        var report = CreateValidFoundReport();

        report.DateFound =
            DateTime.Today.AddDays(1);

        bool result =
            service.ValidateFoundReport(
                report,
                out string errorMessage);

        Assert.IsFalse(result);
        Assert.AreEqual(
            "Found date cannot be in the future.",
            errorMessage);
    }
}
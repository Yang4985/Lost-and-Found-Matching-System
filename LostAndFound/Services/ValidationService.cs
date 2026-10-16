using LostAndFound.Models;

namespace LostAndFound.Services;

public class ValidationService
{
    // Collect errors together so users can correct every mandatory field in one submission.
    public IReadOnlyList<string> GetLostReportErrors(LostItemReport? report)
    {
        if (report is null) return new[] { "Lost report is required." };
        var errors = new List<string>();
        AddRequiredErrors(errors, report.Name, report.Category, report.Location, report.Description);
        if (report.DateLost.Date > DateTime.Today)
            errors.Add("Lost date cannot be in the future.");
        return errors.AsReadOnly();
    }

    public IReadOnlyList<string> GetFoundReportErrors(FoundItemReport? report)
    {
        if (report is null) return new[] { "Found report is required." };
        var errors = new List<string>();
        AddRequiredErrors(errors, report.Name, report.Category, report.Location, report.Description);
        if (string.IsNullOrWhiteSpace(report.StorageLocation))
            errors.Add("Storage location is required.");
        if (report.DateFound.Date > DateTime.Today)
            errors.Add("Found date cannot be in the future.");
        return errors.AsReadOnly();
    }

    // Preserve the existing boolean/message API, including exact single-error messages.
    public bool ValidateLostReport(LostItemReport? report, out string errorMessage)
    {
        var errors = GetLostReportErrors(report);
        errorMessage = string.Join(Environment.NewLine, errors);
        return errors.Count == 0;
    }

    public bool ValidateFoundReport(FoundItemReport? report, out string errorMessage)
    {
        var errors = GetFoundReportErrors(report);
        errorMessage = string.Join(Environment.NewLine, errors);
        return errors.Count == 0;
    }

    private static void AddRequiredErrors(List<string> errors, string? name,
        string? category, string? location, string? description)
    {
        if (string.IsNullOrWhiteSpace(name)) errors.Add("Item name is required.");
        if (string.IsNullOrWhiteSpace(category)) errors.Add("Category is required.");
        if (string.IsNullOrWhiteSpace(location)) errors.Add("Location is required.");
        if (string.IsNullOrWhiteSpace(description)) errors.Add("Description is required.");
    }
}

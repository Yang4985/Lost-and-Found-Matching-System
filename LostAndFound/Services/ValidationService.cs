using LostAndFound.Models;

namespace LostAndFound.Services
{
    public class ValidationService
    {
        public bool ValidateLostReport(
            LostItemReport report,
            out string errorMessage)
        {
            if (report == null)
            {
                errorMessage = "Lost report is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(report.Name))
            {
                errorMessage = "Item name is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(report.Category))
            {
                errorMessage = "Category is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(report.Location))
            {
                errorMessage = "Location is required.";
                return false;
            }

            if (report.DateLost.Date > DateTime.Today)
            {
                errorMessage = "Lost date cannot be in the future.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        public bool ValidateFoundReport(
            FoundItemReport report,
            out string errorMessage)
        {
            if (report == null)
            {
                errorMessage = "Found report is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(report.Name))
            {
                errorMessage = "Item name is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(report.Category))
            {
                errorMessage = "Category is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(report.Location))
            {
                errorMessage = "Location is required.";
                return false;
            }

            if (report.DateFound.Date > DateTime.Today)
            {
                errorMessage = "Found date cannot be in the future.";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }
    }
}
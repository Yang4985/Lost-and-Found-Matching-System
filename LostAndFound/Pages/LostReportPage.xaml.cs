    using LostAndFound.Models;
    using LostAndFound.Services;

    namespace LostAndFound.Pages
    {
        public partial class LostReportPage : ContentPage
        {
            private readonly PrototypeDataService _data;
            public LostReportPage(PrototypeDataService data)
            {
                InitializeComponent();
                _data = data;
                CategoryPicker.ItemsSource = new[] { "Wallet", "Electronics", "Keys", "Clothing", "Bag", "Other" };
                ReportsView.ItemsSource = _data.LostReports;
            }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            // The shared submission service validates all mandatory fields.
            var report = new LostItemReport
            {
                Item = new Item
                {
                    Brand = BrandEntry.Text?.Trim() ?? string.Empty,
                    Color = ColourEntry.Text?.Trim() ?? string.Empty
                },

                Name = ItemNameEntry.Text?.Trim() ?? string.Empty,
                Category = CategoryPicker.SelectedItem?.ToString() ?? string.Empty,
                Location = LocationEntry.Text?.Trim() ?? string.Empty,
                DateLost = LostDatePicker.Date ?? DateTime.Today,
                Description = DescriptionEditor.Text?.Trim() ?? string.Empty,
                DistinguishingFeatures =
                    FeaturesEntry.Text?.Trim() ?? string.Empty,
                ReportedByStudentID = 1
            };


            try
            {
                if (!_data.TrySubmitLostReport(report, out string errorMessage))
                {
                    ValidationLabel.Text = errorMessage;
                    ValidationLabel.IsVisible = true;
                    return;
                }
                ValidationLabel.IsVisible = false;
            }
            catch (Exception error)
                when (error is IOException or UnauthorizedAccessException)
            {
                await DisplayAlertAsync(
                    "Unable to save",
                    "Your report could not be saved. Please retry. Your form has been kept.",
                    "OK");

                return;
            }

            ClearForm();

            await DisplayAlertAsync(
                "Report saved",
                "The lost item report was added with Submitted status.",
                "OK");
        }

        private void ClearForm()
            {
                ItemNameEntry.Text = string.Empty;
                BrandEntry.Text = string.Empty;
                ColourEntry.Text = string.Empty;
                CategoryPicker.SelectedIndex = -1;
                LocationEntry.Text = string.Empty;
                DescriptionEditor.Text = string.Empty;
                FeaturesEntry.Text = string.Empty;
                LostDatePicker.Date = DateTime.Today;
                ValidationLabel.IsVisible = false;
            }
        }
    }

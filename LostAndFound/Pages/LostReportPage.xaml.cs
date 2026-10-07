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
            if (string.IsNullOrWhiteSpace(ItemNameEntry.Text) ||
                CategoryPicker.SelectedItem is null ||
                string.IsNullOrWhiteSpace(LocationEntry.Text) ||
                string.IsNullOrWhiteSpace(DescriptionEditor.Text))
            {
                ValidationLabel.Text = "Please complete the item name, category, location, and description.";
                ValidationLabel.IsVisible = true;
                return;
            }

            try
            {
            _data.AddLostReport(new LostItemReport
            {
                Item = new Item { Brand = BrandEntry.Text?.Trim() ?? string.Empty, Color = ColourEntry.Text?.Trim() ?? string.Empty },
                Name = ItemNameEntry.Text.Trim(),
                Category = CategoryPicker.SelectedItem.ToString()!,
                Location = LocationEntry.Text.Trim(),
                DateLost = LostDatePicker.Date ?? DateTime.Today,
                Description = DescriptionEditor.Text.Trim(),
                DistinguishingFeatures = FeaturesEntry.Text?.Trim() ?? string.Empty,
                ReportedByStudentID = 1
            });
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                await DisplayAlertAsync("Unable to save", "Your report could not be saved. Please retry. Your form has been kept.", "OK");
                return;
            }

            ClearForm();
            await DisplayAlertAsync("Report saved", "The lost item report was added with Submitted status.", "OK");
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

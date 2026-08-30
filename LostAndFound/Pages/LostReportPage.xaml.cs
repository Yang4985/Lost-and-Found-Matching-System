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

            _data.AddLostReport(new LostItemReport
            {
                Name = ItemNameEntry.Text.Trim(),
                Category = CategoryPicker.SelectedItem.ToString()!,
                Location = LocationEntry.Text.Trim(),
                DateLost = LostDatePicker.Date,
                Description = DescriptionEditor.Text.Trim(),
                DistinguishingFeatures = FeaturesEntry.Text?.Trim() ?? string.Empty,
                ReportedByStudentID = 1
            });

            ClearForm();
            await DisplayAlertAsync("Report saved", "The lost item report was added with Submitted status.", "OK");
        }

        private void ClearForm()
        {
            ItemNameEntry.Text = string.Empty;
            CategoryPicker.SelectedIndex = -1;
            LocationEntry.Text = string.Empty;
            DescriptionEditor.Text = string.Empty;
            FeaturesEntry.Text = string.Empty;
            LostDatePicker.Date = DateTime.Today;
            ValidationLabel.IsVisible = false;
        }
    }
}

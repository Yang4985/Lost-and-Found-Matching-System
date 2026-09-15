using LostAndFound.Models;
using LostAndFound.Services;

namespace LostAndFound.Pages
{
    public partial class FoundReportPage : ContentPage
    {
        private readonly PrototypeDataService _data;

        public FoundReportPage(PrototypeDataService data)
        {
            InitializeComponent();
            _data = data;
            CategoryPicker.ItemsSource = new[] { "Wallet", "Electronics", "Keys", "Clothing", "Bag", "Other" };
            ReportsView.ItemsSource = _data.FoundReports;
        }

        private async void OnSaveClicked(object? sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ItemNameEntry.Text) ||
                CategoryPicker.SelectedItem is null ||
                string.IsNullOrWhiteSpace(LocationEntry.Text) ||
                string.IsNullOrWhiteSpace(DescriptionEditor.Text) ||
                string.IsNullOrWhiteSpace(StorageEntry.Text))
            {
                ValidationLabel.Text = "Please complete all required fields before saving.";
                ValidationLabel.IsVisible = true;
                return;
            }

            _data.AddFoundReport(new FoundItemReport
            {
                Name = ItemNameEntry.Text.Trim(),
                Category = CategoryPicker.SelectedItem.ToString()!,
                Location = LocationEntry.Text.Trim(),
                DateFound = FoundDatePicker.Date,
                Description = DescriptionEditor.Text.Trim(),
                StorageLocation = StorageEntry.Text.Trim(),
                RegisteredByStaffID = 1
            });

            ClearForm();
            await DisplayAlertAsync("Item registered", "The found item was added and is ready for matching.", "OK");
        }

        private void ClearForm()
        {
            ItemNameEntry.Text = string.Empty;
            CategoryPicker.SelectedIndex = -1;
            LocationEntry.Text = string.Empty;
            DescriptionEditor.Text = string.Empty;
            StorageEntry.Text = string.Empty;
            FoundDatePicker.Date = DateTime.Today;
            ValidationLabel.IsVisible = false;
        }
    }
}

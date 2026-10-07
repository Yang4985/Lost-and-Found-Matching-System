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

            try
            {
            _data.AddFoundReport(new FoundItemReport
            {
                Item = new Item { Brand = BrandEntry.Text?.Trim() ?? string.Empty, Color = ColourEntry.Text?.Trim() ?? string.Empty },
                Name = ItemNameEntry.Text.Trim(),
                Category = CategoryPicker.SelectedItem.ToString()!,
                Location = LocationEntry.Text.Trim(),
                DateFound = FoundDatePicker.Date ?? DateTime.Today,
                Description = DescriptionEditor.Text.Trim(),
                StorageLocation = StorageEntry.Text.Trim(),
                RegisteredByStaffID = 1
            });
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                await DisplayAlertAsync("Unable to save", "Your report could not be saved. Please retry. Your form has been kept.", "OK");
                return;
            }

            ClearForm();
            await DisplayAlertAsync("Item registered", "The found item was added and is ready for matching.", "OK");
        }

        private void ClearForm()
        {
            ItemNameEntry.Text = string.Empty;
            BrandEntry.Text = string.Empty;
            ColourEntry.Text = string.Empty;
            CategoryPicker.SelectedIndex = -1;
            LocationEntry.Text = string.Empty;
            DescriptionEditor.Text = string.Empty;
            StorageEntry.Text = string.Empty;
            FoundDatePicker.Date = DateTime.Today;
            ValidationLabel.IsVisible = false;
        }
    }
}

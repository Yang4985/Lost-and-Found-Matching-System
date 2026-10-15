using LostAndFound.Models;
using LostAndFound.Services;

namespace LostAndFound.Pages
{
    public partial class PublicFoundReportPage : ContentPage
    {
        private readonly PrototypeDataService _data;
        private readonly ValidationService _validationService =
            new ValidationService();

        public PublicFoundReportPage(PrototypeDataService data)
        {
            InitializeComponent();

            _data = data;

            CategoryPicker.ItemsSource = new[]
            {
                "Wallet",
                "Electronics",
                "Keys",
                "Clothing",
                "Bag",
                "Other"
            };
        }

        private async void OnSubmitClicked(
            object? sender,
            EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ItemNameEntry.Text) ||
                CategoryPicker.SelectedItem is null ||
                string.IsNullOrWhiteSpace(LocationEntry.Text) ||
                string.IsNullOrWhiteSpace(DescriptionEditor.Text))
            {
                ValidationLabel.Text =
                    "Please complete all required fields.";

                ValidationLabel.IsVisible = true;
                return;
            }

            var report = new FoundItemReport
            {
                Item = new Item
                {
                    Brand = BrandEntry.Text?.Trim() ?? string.Empty,
                    Color = ColourEntry.Text?.Trim() ?? string.Empty
                },

                Name = ItemNameEntry.Text.Trim(),

                Category =
                    CategoryPicker.SelectedItem.ToString()!,

                Location =
                    LocationEntry.Text.Trim(),

                DateFound =
                    FoundDatePicker.Date ?? DateTime.Today,

                Description =
                    DescriptionEditor.Text.Trim(),

                StorageLocation =
                    "Pending staff intake",

                RegisteredByStaffID = 0
            };

            if (!_validationService.ValidateFoundReport(
                    report,
                    out string errorMessage))
            {
                ValidationLabel.Text = errorMessage;
                ValidationLabel.IsVisible = true;
                return;
            }

            ValidationLabel.IsVisible = false;

            try
            {
                _data.AddFoundReport(report);
            }
            catch (Exception error)
                when (error is IOException
                    or UnauthorizedAccessException)
            {
                await DisplayAlertAsync(
                    "Unable to submit",
                    "The report could not be saved. Please try again.",
                    "OK");

                return;
            }

            ClearForm();

            await DisplayAlertAsync(
                "Report Submitted",
                "Thank you. Campus staff can now review your found-item report.",
                "OK");

            await Shell.Current.GoToAsync("//home");
        }

        private async void OnBackClicked(
            object? sender,
            EventArgs e)
        {
            await Shell.Current.GoToAsync("//home");
        }

        private void ClearForm()
        {
            ItemNameEntry.Text = string.Empty;
            BrandEntry.Text = string.Empty;
            ColourEntry.Text = string.Empty;
            CategoryPicker.SelectedIndex = -1;
            LocationEntry.Text = string.Empty;
            DescriptionEditor.Text = string.Empty;
            FoundDatePicker.Date = DateTime.Today;
            ValidationLabel.IsVisible = false;
        }
    }
}
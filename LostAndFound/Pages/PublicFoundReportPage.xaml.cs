using LostAndFound.Models;
using LostAndFound.Services;

namespace LostAndFound.Pages
{
    public partial class PublicFoundReportPage : ContentPage
    {
        private readonly PrototypeDataService _data;
        private string? _photoPath;
        private async void OnAttachPhotoClicked(object? sender, EventArgs e)
        {
            try
            {
                var photo = await FilePicker.Default.PickAsync(new PickOptions { PickerTitle = "Choose an optional item image", FileTypes = FilePickerFileType.Images });
                if (photo is null) return;
                await using var stream = await photo.OpenReadAsync();
                _photoPath = await ItemPhotoService.SaveAsync(stream, Path.Combine(FileSystem.AppDataDirectory, "item-photos"));
                PhotoLabel.Text = "Image attached";
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException or InvalidDataException)
            { PhotoLabel.Text = error.Message; }
        }


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
            var report = new FoundItemReport
            {
                Item = new Item
                {
                    ImagePath = _photoPath,
                    Brand = BrandEntry.Text?.Trim() ?? string.Empty,
                    Color = ColourEntry.Text?.Trim() ?? string.Empty
                },

                Name = ItemNameEntry.Text?.Trim() ?? string.Empty,

                Category =
                    CategoryPicker.SelectedItem?.ToString() ?? string.Empty,

                Location =
                    LocationEntry.Text?.Trim() ?? string.Empty,

                DateFound =
                    FoundDatePicker.Date ?? DateTime.Today,

                Description =
                    DescriptionEditor.Text?.Trim() ?? string.Empty,

                StorageLocation =
                    "Pending staff intake",

                RegisteredByStaffID = 0
            };

            try
            {
                if (!_data.TrySubmitPublicFoundReport(report, out string errorMessage))
                {
                    ValidationLabel.Text = errorMessage;
                    ReportFormFeedback.Show(this, errorMessage);
                    ValidationLabel.IsVisible = true;
                    return;
                }
                ValidationLabel.IsVisible = false;
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
                ReportFormFeedback.Show(this, string.Empty);
                _photoPath = null; PhotoLabel.Text = "No image selected";
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

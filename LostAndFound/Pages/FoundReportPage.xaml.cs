using System.Collections.ObjectModel;
using LostAndFound.Models;
using LostAndFound.Services;

namespace LostAndFound.Pages
{
    public partial class FoundReportPage : ContentPage
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
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            { PhotoLabel.Text = error.Message; }
        }


        private readonly ObservableCollection<FoundItemReport>
            _availableReports = new();

        private readonly ObservableCollection<FoundItemReport>
            _returnedReports = new();

        private readonly HashSet<int> _selectedForReturn = new();

        private readonly ObservableCollection<FoundItemReport>
            _publicSubmissions = new();


        public FoundReportPage(PrototypeDataService data)
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

            PublicSubmissionsView.ItemsSource = _publicSubmissions;
            AvailableReportsView.ItemsSource = _availableReports;
            ReturnedReportsView.ItemsSource = _returnedReports;

            RefreshReportLists();
        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!StaffSessionService.IsAuthenticated)
            {
                await DisplayAlertAsync(
                    "Staff Access Required",
                    "Please sign in with an authorised staff account.",
                    "OK");

                await Shell.Current.GoToAsync(
                    "//staff-login");

                return;
            }

            RefreshReportLists();
        }


        private async void OnSaveClicked(
            object? sender,
            EventArgs e)
        {
            if (!StaffSessionService.IsAuthenticated) { await Shell.Current.GoToAsync("//staff-login"); return; }
            // All validation is performed at the submission boundary.
            var report = new FoundItemReport
            {
                Item = new Item
                {
                    ImagePath = _photoPath,
                    Brand =
                        BrandEntry.Text?.Trim()
                        ?? string.Empty,

                    Color =
                        ColourEntry.Text?.Trim()
                        ?? string.Empty
                },

                Name =
                    ItemNameEntry.Text?.Trim() ?? string.Empty,

                Category =
                    CategoryPicker.SelectedItem?.ToString() ?? string.Empty,

                Location =
                    LocationEntry.Text?.Trim() ?? string.Empty,

                DateFound =
                    FoundDatePicker.Date
                    ?? DateTime.Today,

                Description =
                    DescriptionEditor.Text?.Trim() ?? string.Empty,

                StorageLocation =
                    StorageEntry.Text?.Trim() ?? string.Empty,

                RegisteredByStaffID = 1
            };



            try
            {
                if (!_data.TryRegisterFoundReport(report, out string errorMessage))
                {
                    ValidationLabel.Text = errorMessage;
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
                    "Unable to save",
                    "Your report could not be saved. " +
                    "Please retry. Your form has been kept.",
                    "OK");

                return;
            }


            ClearForm();

            RefreshReportLists();


            await DisplayAlertAsync(
                "Item registered",
                "The found item was added and is ready for matching.",
                "OK");
        }


        private void OnReturnSelectionChanged(
            object? sender,
            CheckedChangedEventArgs e)
        {
            if (sender is not CheckBox checkBox)
                return;

            if (checkBox.BindingContext
                is not FoundItemReport report)
            {
                return;
            }


            if (e.Value)
            {
                _selectedForReturn.Add(report.ID);
            }
            else
            {
                _selectedForReturn.Remove(report.ID);
            }
        }


        private async void OnMarkReturnedClicked(
            object? sender,
            EventArgs e)
        {
            if (_selectedForReturn.Count == 0)
            {
                await DisplayAlertAsync(
                    "No items selected",
                    "Select at least one found item first.",
                    "OK");

                return;
            }


            bool confirmed =
                await DisplayAlertAsync(
                    "Confirm Return",
                    $"Mark {_selectedForReturn.Count} selected item(s) as returned?",
                    "Mark Returned",
                    "Cancel");


            if (!confirmed)
                return;


            try
            {
                foreach (int reportId in _selectedForReturn)
                {
                    _data.ReturnReviewedItem(reportId, "Staff confirmed item handover");
                }
            }
            catch (Exception error)
                when (error is IOException
                    or UnauthorizedAccessException
                    or InvalidOperationException)
            {
                await DisplayAlertAsync(
                    "Unable to update",
                    "One or more reports could not be marked as returned.",
                    "OK");

                return;
            }


            int returnedCount =
                _selectedForReturn.Count;


            _selectedForReturn.Clear();

            RefreshReportLists();


            await DisplayAlertAsync(
                "Items Returned",
                $"{returnedCount} item(s) were moved to the Returned Items list.",
                "OK");
        }

        private async void OnBackToDashboardClicked(
    object? sender,
    EventArgs e)
        {
            await Shell.Current.GoToAsync("//staff-dashboard");
        }
        private void RefreshReportLists()
        {
            _publicSubmissions.Clear();
            _availableReports.Clear();
            _returnedReports.Clear();


            foreach (FoundItemReport report
                     in _data.FoundReports)
            {
                if (report.Status
                    == ItemReportStatus.Returned)
                {
                    _returnedReports.Add(report);

                    continue;
                }


                if (report.RegisteredByStaffID == 0)
                {
                    _publicSubmissions.Add(report);
                }
                else
                {
                    _availableReports.Add(report);
                }
            }
        }


        private void ClearForm()
        {
                _photoPath = null; PhotoLabel.Text = "No image selected";
            ItemNameEntry.Text =
                string.Empty;

            BrandEntry.Text =
                string.Empty;

            ColourEntry.Text =
                string.Empty;

            CategoryPicker.SelectedIndex =
                -1;

            LocationEntry.Text =
                string.Empty;

            DescriptionEditor.Text =
                string.Empty;

            StorageEntry.Text =
                string.Empty;

            FoundDatePicker.Date =
                DateTime.Today;

            ValidationLabel.IsVisible =
                false;
        }
    }
}

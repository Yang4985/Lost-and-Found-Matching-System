using LostAndFound.Services;

namespace LostAndFound.Pages
{
    public partial class WelcomePage : ContentPage
    {
        private readonly PrototypeDataService _data;

        public WelcomePage(PrototypeDataService data)
        {
            InitializeComponent();

            _data = data;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            RefreshFoundItemSummary();
        }

        private void RefreshFoundItemSummary()
        {
            WalletCountLabel.Text =
                CountActiveCategory("Wallet").ToString();

            ElectronicsCountLabel.Text =
                CountActiveCategory("Electronics").ToString();

            KeysCountLabel.Text =
                CountActiveCategory("Keys").ToString();

            ClothingCountLabel.Text =
                CountActiveCategory("Clothing").ToString();

            BagCountLabel.Text =
                CountActiveCategory("Bag").ToString();

            OtherCountLabel.Text =
                CountActiveCategory("Other").ToString();
        }

        private int CountActiveCategory(string category)
        {
            return _data.FoundReports.Count(
                report =>
                    PrototypeDataService.IsEligible(report.Status)
                    &&
                    string.Equals(
                        report.Category,
                        category,
                        StringComparison.OrdinalIgnoreCase));
        }

        private async void OnReportLostClicked(
            object? sender,
            EventArgs e)
        {
            await Shell.Current.GoToAsync("//lost-reports");
        }

        private async void OnReportFoundClicked(
            object? sender,
            EventArgs e)
        {
            await Shell.Current.GoToAsync("//public-found-report");
        }

        private async void OnStaffSignInClicked(
            object? sender,
            EventArgs e)
        {
            await Shell.Current.GoToAsync("//staff-login");
        }
    }
}
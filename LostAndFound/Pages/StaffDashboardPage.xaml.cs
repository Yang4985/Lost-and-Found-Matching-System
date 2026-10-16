using LostAndFound.Models;
using LostAndFound.Services;

namespace LostAndFound.Pages
{
    public partial class StaffDashboardPage : ContentPage
    {
        private readonly PrototypeDataService _data;

        public StaffDashboardPage(
            PrototypeDataService data)
        {
            InitializeComponent();

            _data = data;
        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!StaffSessionService.IsAuthenticated)
            {
                await Shell.Current.GoToAsync(
                    "//staff-login");

                return;
            }

            RefreshDashboard();
        }


        private async void OnWorkflowClicked(object? sender, EventArgs e) => await Shell.Current.GoToAsync("//review-workflow");

        private void RefreshDashboard()
        {
            SignedInLabel.Text =
                $"Signed in as {StaffSessionService.CurrentStaffId}";

            int activeFound =
                _data.FoundReports.Count(
                    r => PrototypeDataService.IsEligible(r.Status));

            int returned =
                _data.FoundReports.Count(
                    r => r.Status == ItemReportStatus.Returned);

            int matches =
                _data.GenerateMatches().Count;

            ActiveFoundCountLabel.Text =
                activeFound.ToString();

            ReturnedCountLabel.Text =
                returned.ToString();

            MatchCountLabel.Text =
                matches.ToString();
        }


        private async void OnFoundReportsClicked(
            object? sender,
            EventArgs e)
        {
            await Shell.Current.GoToAsync(
                "//found-reports");
        }


        private async void OnMatchesClicked(
            object? sender,
            EventArgs e)
        {
            await Shell.Current.GoToAsync(
                "//matches");
        }


        private async void OnSignOutClicked(
            object? sender,
            EventArgs e)
        {
            StaffSessionService.SignOut();

            await Shell.Current.GoToAsync(
                "//home");
        }
    }
}
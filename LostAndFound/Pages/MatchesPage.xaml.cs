using LostAndFound.Services;

namespace LostAndFound.Pages
{
    public partial class MatchesPage : ContentPage
    {
        private readonly PrototypeDataService _data;

        public MatchesPage(PrototypeDataService data)
        {
            InitializeComponent();

            _data = data;

            RefreshMatches();
        }


        protected override async void OnAppearing()
        {
            base.OnAppearing();

            if (!StaffSessionService.IsAuthenticated)
            {
                await DisplayAlertAsync(
                    "Staff Access Required",
                    "Only authorised staff can review possible matches.",
                    "OK");

                await Shell.Current.GoToAsync(
                    "//staff-login");

                return;
            }

            RefreshMatches();
        }


        private void OnRefreshClicked(
            object? sender,
            EventArgs e)
        {
            RefreshMatches();
        }

        private async void OnBackToDashboardClicked(
             object? sender,
             EventArgs e)
             {
            await Shell.Current.GoToAsync("//staff-dashboard");
             
        }
        private void RefreshMatches()
        {
            var matches = _data.GenerateMatches();

            MatchesView.ItemsSource = matches;

            MatchCountLabel.Text =
                matches.Count == 1
                    ? "1 possible match"
                    : $"{matches.Count} possible matches";
        }
    }
}
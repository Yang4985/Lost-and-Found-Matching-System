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

        protected override void OnAppearing()
        {
            base.OnAppearing();
            RefreshMatches();
        }

        private void OnRefreshClicked(object? sender, EventArgs e) => RefreshMatches();

        private void RefreshMatches() => MatchesView.ItemsSource = _data.GenerateMatches();
    }
}

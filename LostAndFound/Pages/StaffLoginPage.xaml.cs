using LostAndFound.Services;

namespace LostAndFound.Pages
{
    public partial class StaffLoginPage : ContentPage
    {
        public StaffLoginPage()
        {
            InitializeComponent();
        }

        private async void OnSignInClicked(
            object? sender,
            EventArgs e)
        {
            string staffId =
                StaffIdEntry.Text?.Trim()
                ?? string.Empty;

            string password =
                PasswordEntry.Text
                ?? string.Empty;


            if (string.IsNullOrWhiteSpace(staffId) ||
                string.IsNullOrWhiteSpace(password))
            {
                ErrorLabel.Text =
                    "Enter your staff ID and password.";

                ErrorLabel.IsVisible = true;
                return;
            }


            bool authenticated =
                StaffSessionService.SignIn(
                    staffId,
                    password);


            if (!authenticated)
            {
                ErrorLabel.Text =
                    "Invalid staff ID or password.";

                ErrorLabel.IsVisible = true;

                PasswordEntry.Text = string.Empty;

                return;
            }


            ErrorLabel.IsVisible = false;


            await Shell.Current.GoToAsync(
                 "//staff-dashboard");
        }


        private async void OnBackClicked(
            object? sender,
            EventArgs e)
        {
            await Shell.Current.GoToAsync(
                "//home");
        }
    }
}
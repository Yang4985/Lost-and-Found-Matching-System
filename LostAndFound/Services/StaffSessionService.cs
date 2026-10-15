namespace LostAndFound.Services
{
    public static class StaffSessionService
    {
        public static bool IsAuthenticated { get; private set; }

        public static string? CurrentStaffId { get; private set; }

        public static bool SignIn(string staffId, string password)
        {
            bool valid =
                staffId.Equals(
                    "STAFF001",
                    StringComparison.OrdinalIgnoreCase)
                &&
                password == "Campus123!";

            if (!valid)
                return false;

            IsAuthenticated = true;
            CurrentStaffId = staffId;

            return true;
        }

        public static void SignOut()
        {
            IsAuthenticated = false;
            CurrentStaffId = null;
        }
    }
}
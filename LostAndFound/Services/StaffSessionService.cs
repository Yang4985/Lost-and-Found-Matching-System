namespace LostAndFound.Services
{
    public static class StaffSessionService
    {
        public static bool IsAuthenticated { get; private set; }

        public static string? CurrentStaffId { get; private set; }

        public static bool SignIn(string staffId, string password)
        {
            bool valid =
                string.Equals(staffId,
                    "STAFF001",
                    StringComparison.OrdinalIgnoreCase)
                &&
                password == "Campus123!";

            if (!valid)
            {
                SignOut();
                return false;
            }

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
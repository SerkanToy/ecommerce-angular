namespace ecommerce.utility
{
    public static class SD
    {
        private static readonly Random _random = new Random();
        // Cookie
        public static readonly string IdentityAppCookie = "identityappcookie";

        public const string NameRegex = "^[a-zA-ZçÇğĞıİöÖşŞüÜ]*$";
        public const string EmailRegex = "^.+@[^\\.].*\\.[a-z]{2,}$";
        public const string UserNameRegex = "^[a-zA-Z][a-zA-Z0-9]*$";

        // Application rules
        public const int RequiredPasswordLength = 6;
        public const int MaxFailedAccessAttempts = 3;
        public const int DefaultLockoutTimeSpanInDays = 1;
        public const int DefaultDaysToLock = 5;


        // Application roles
        public const string AdminRole = "admin";
        public const string UserRole = "normaluser";
        public const string TechnicalUserRole = "technicaluser";
        public const string ManagerUserRole = "manageruser";
        public const string PresidentUserRole = "presidentuser";
        public static readonly List<string> Roles = new List<string> { AdminRole, UserRole, TechnicalUserRole, ManagerUserRole, PresidentUserRole };

        // User actions
        public const string Active = "active";
        public const string Inactive = "inactive";
        public const string Lock = "lock";
        public const string Unlock = "unlock";
        public const string Delete = "delete";
        public const string Block = "block";
        public const string Unblock = "unblock";

        public const string IdentityAppTokenProvider = "web";
        public const string IdentityAppTokenName = "access_token";

        // Naming
        public const string EC = "ec";
        public const string FUP = "fup";



        public static string AccountLockedMessage(DateTime endDate)
        {
            DateTime startDate = DateTime.UtcNow;
            TimeSpan difference = endDate - startDate;

            int days = difference.Days;
            int hours = difference.Hours;
            int minutes = difference.Minutes;
            return string.Format("Your account is temporary locked.<br>You should wait {0} day(s), {1} hour(s) and {2} minute(s)",
                days, hours, minutes);
        }

        public static string GenerateRandomPassword(int length = 6)
        {
            //int[] dizi = [ 6, 7, 8, 9, 10, 11, 12 ];
            //length = _random.Next(6, 12); // Random.Shared.Next(6,12);
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            return new string(Enumerable.Repeat(validChars, length)
                            .Select(x => x[_random.Next(x.Length)]).ToArray());
        }

    }
}

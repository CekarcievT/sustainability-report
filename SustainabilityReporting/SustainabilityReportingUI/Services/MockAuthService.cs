using System.Security.Claims;

namespace SustainabilityReportingUI.Services
{
    public class MockAuthService
    {
        public string? UserName { get; private set; }
        public string? Role { get; private set; }
        public string? VenueId { get; private set; }

        public bool IsAuthenticated => !string.IsNullOrEmpty(UserName);

        public event Action? AuthStateChanged;

        public void Login(string user)
        {
            UserName = user;
            switch (user)
            {
                case "group_A":
                    UserName = "UserA";
                    Role = "UserA";
                    break;
                case "group_B":
                    UserName = "UserB";
                    Role = "UserB";
                    break;
                case "Manager":
                    UserName = "Manager";
                    Role = "Manager";
                    break;
                default:
                    Role = null;
                    break;
            }
            AuthStateChanged?.Invoke();
        }

        public void Logout()
        {
            UserName = null;
            Role = null;
            AuthStateChanged?.Invoke();
        }
    }
}
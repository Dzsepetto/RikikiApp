using RikikiApp.Models;

namespace RikikiApp.Services
{
    public class UserSessionService
    {
        public User? CurrentUser { get; private set; }

        public bool HasUser => CurrentUser != null;

        public bool IsLoggedIn =>
            CurrentUser != null &&
            !CurrentUser.IsGuest;

        public void SetCurrentUser(User user)
        {
            CurrentUser = user;
        }

        public void Clear()
        {
            CurrentUser = null;
        }
    }
}
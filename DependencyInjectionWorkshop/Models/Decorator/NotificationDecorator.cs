using DependencyInjectionWorkshop.Models.Notify;

namespace DependencyInjectionWorkshop.Models.Decorator
{
    public class NotificationDecorator : IAuthentication
    {
        private readonly IAuthentication _authenticationService;
        private readonly INotification _notify;

        public NotificationDecorator(IAuthentication authenticationService, INotification notify)
        {
            _authenticationService = authenticationService;
            _notify = notify;
        }

        private void Send(string userAccount)
        {
            _notify.Send(userAccount);
        }

        public bool Verify(string userAccount, string password, string otp)
        {
            var validResult = _authenticationService.Verify(userAccount, password, otp);
            if (!validResult)
            {
                Send(userAccount);
            }

            return validResult;
        }
    }
}
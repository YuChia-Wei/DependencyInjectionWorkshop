using DependencyInjectionWorkshop.Models.Notify;

namespace DependencyInjectionWorkshop.Models.Decorator
{
    public class NotificationDecorator : AuthenticationBaseDecorator
    {
        private readonly INotification _notify;

        public NotificationDecorator(IAuthentication authentication, INotification notify) : base(authentication)
        {
            _notify = notify;
        }

        private void Send(string userAccount)
        {
            _notify.Send(userAccount);
        }

        public override bool Verify(string userAccount, string password, string otp)
        {
            var validResult = base.Verify(userAccount, password, otp);
            if (!validResult)
            {
                Send(userAccount);
            }

            return validResult;
        }
    }
}
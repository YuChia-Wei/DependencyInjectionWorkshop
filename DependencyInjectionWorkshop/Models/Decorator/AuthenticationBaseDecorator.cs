namespace DependencyInjectionWorkshop.Models.Decorator
{
    public class AuthenticationBaseDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;

        public AuthenticationBaseDecorator(IAuthentication authentication)
        {
            _authentication = authentication;
        }

        public virtual bool Verify(string userAccount, string password, string otp)
        {
            var validResult = _authentication.Verify(userAccount, password, otp);
            return validResult;
        }
    }
}
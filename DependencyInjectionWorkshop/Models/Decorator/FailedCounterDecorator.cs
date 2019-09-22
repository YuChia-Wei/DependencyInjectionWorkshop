using DependencyInjectionWorkshop.Models.FailedCounter;

namespace DependencyInjectionWorkshop.Models.Decorator
{
    public class FailedCounterDecorator:AuthenticationBaseDecorator
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authentication, IFailedCounter failedCounter) : base(authentication)
        {
            _failedCounter = failedCounter;
        }

        public override bool Verify(string userAccount, string password, string otp)
        {
            var validResult = base.Verify(userAccount, password, otp);
            if (validResult)
            {
                _failedCounter.ResetFailedCount(userAccount);
            }
            else
            {
                _failedCounter.AddFailedCount(userAccount);
            }
            return validResult;
        }
    }
}
using DependencyInjectionWorkshop.Models.FailedCounter;
using DependencyInjectionWorkshop.Models.LogService;

namespace DependencyInjectionWorkshop.Models.Decorator
{
    public class LogFailedCountDecorator:AuthenticationBaseDecorator
    {
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;

        public LogFailedCountDecorator(IAuthentication authentication, IFailedCounter failedCounter, ILogger logger) : base(authentication)
        {
            _failedCounter = failedCounter;
            _logger = logger;
        }

        public override bool Verify(string userAccount, string password, string otp)
        {
            bool isvalid = base.Verify(userAccount, password, otp);
            if (!isvalid)
            {
                LogFailedCount(userAccount);
            }

            return isvalid;
        }

        private void LogFailedCount(string userAccount)
        {
            var failedCount = _failedCounter.GetFailedCount(userAccount);
            _logger.Info($"accountId:{userAccount} failed times:{failedCount}");
        }
    }
}
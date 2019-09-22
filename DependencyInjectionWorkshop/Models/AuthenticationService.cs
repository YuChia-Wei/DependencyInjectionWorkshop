using System;
using DependencyInjectionWorkshop.Models.Decorator;
using DependencyInjectionWorkshop.Models.FailedCounter;
using DependencyInjectionWorkshop.Models.Hash;
using DependencyInjectionWorkshop.Models.LogService;
using DependencyInjectionWorkshop.Models.Otp;
using DependencyInjectionWorkshop.Models.Profile;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService : IAuthentication
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly ILogger _logger;
        private readonly IFailedCounter _failedCounter;
        private readonly FailedCounterDecorator _failedCounterDecorator;

        public AuthenticationService()
        {
            //_failedCounterDecorator = new FailedCounterDecorator(this);
            _profile = new ProfileDbo();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            _logger = new NLogAdapter();
            _failedCounter = new FailedCounter.FailedCounter();
        }

        public AuthenticationService(IFailedCounter failedCounter, ILogger logger, IOtpService otpService,
            IProfile profile, IHash hash)
        {
            //_failedCounterDecorator = new FailedCounterDecorator(this);
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
            _logger = logger;
            _failedCounter = failedCounter;
        }

        public bool Verify(string userAccount, string password, string otp)
        {
            var UserIsLocked = _failedCounter.GetAccountIsLocked(userAccount);
            if (UserIsLocked)
            {
                throw new FailedTooManyTimesException();
            }

            var passwordfordb = _profile.GetPassword(userAccount);

            string hashedPsw = _hash.Compute(password);

            var currentOtp = _otpService.GetCurrentOtp(userAccount);

            if (passwordfordb == hashedPsw && otp == currentOtp)
            {
                return true;
            }
            else
            {
                //_failedCounterDecorator.AddFailedCount(userAccount);

                var failedCount = _failedCounter.GetFailedCount(userAccount);
                _logger.Info($"accountId:{userAccount} failed times:{failedCount}");
            }

            return false;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}
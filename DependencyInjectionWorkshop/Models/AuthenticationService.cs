using System;
using DependencyInjectionWorkshop.Models.FailedCounter;
using DependencyInjectionWorkshop.Models.Hash;
using DependencyInjectionWorkshop.Models.LogService;
using DependencyInjectionWorkshop.Models.Otp;
using DependencyInjectionWorkshop.Models.Profile;

namespace DependencyInjectionWorkshop.Models
{
    public class LogFailedCountDecorator
    {
        private AuthenticationService _authenticationService;

        public LogFailedCountDecorator(AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        private void LogFailedCount(string userAccount)
        {
            var failedCount = _authenticationService._failedCounter.GetFailedCount(userAccount);
            _authenticationService._logger.Info($"accountId:{userAccount} failed times:{failedCount}");
        }
    }

    public class AuthenticationService : IAuthentication
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly ILogger _logger;
        private readonly IFailedCounter _failedCounter;
        private readonly LogFailedCountDecorator _logFailedCountDecorator;

        public AuthenticationService()
        {
            //_failedCounterDecorator = new FailedCounterDecorator(this);
            _logFailedCountDecorator = new LogFailedCountDecorator(this);
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
            _logFailedCountDecorator = new LogFailedCountDecorator(this);
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
            _logger = logger;
            _failedCounter = failedCounter;
        }

        public bool Verify(string userAccount, string password, string otp)
        {
            var passwordfordb = _profile.GetPassword(userAccount);

            string hashedPsw = _hash.Compute(password);

            var currentOtp = _otpService.GetCurrentOtp(userAccount);

            if (passwordfordb == hashedPsw && otp == currentOtp)
            {
                return true;
            }
            else
            {
                _logFailedCountDecorator.LogFailedCount(userAccount);

                return false;
            }

        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}
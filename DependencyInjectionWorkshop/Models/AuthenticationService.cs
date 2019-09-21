using System;
using DependencyInjectionWorkshop.Models.Hash;
using DependencyInjectionWorkshop.Models.LogService;
using DependencyInjectionWorkshop.Models.Notify;
using DependencyInjectionWorkshop.Models.Otp;
using DependencyInjectionWorkshop.Models.Profile;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly IProfile _profile;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpService _otpService;
        private readonly SlackAdapter _slackAdapter;
        private readonly NLogAdapter _nLogAdapter;
        private readonly FailedCounter.FailedCounter _failedCounter;

        public AuthenticationService()
        {
            _profile = new ProfileDbo();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _slackAdapter = new SlackAdapter();
            _nLogAdapter = new NLogAdapter();
            _failedCounter = new FailedCounter.FailedCounter();
        }

        public AuthenticationService(IProfile profileDbo, Sha256Adapter sha256Adapter, OtpService otpService,
            SlackAdapter slackAdapter, NLogAdapter nLogAdapter, FailedCounter.FailedCounter failedCounter)
        {
            _profile = profileDbo;
            _sha256Adapter = sha256Adapter;
            _otpService = otpService;
            _slackAdapter = slackAdapter;
            _nLogAdapter = nLogAdapter;
            _failedCounter = failedCounter;
        }

        public bool Verify(string userAccount, string password, string otp)
        {
            var UserIsLocked = _failedCounter.GetUserLockedStatus(userAccount);
            if (UserIsLocked)
            {
                throw new FailedTooManyTimesException();
            }

            var passwordfordb = _profile.GetPassword(userAccount);

            string hashedPsw = _sha256Adapter.Hash(password);

            var currentOtp = _otpService.GetCurrentOtp(userAccount);

            if (passwordfordb == hashedPsw && otp == currentOtp)
            {
                _failedCounter.ResetFailedCount(userAccount);
                return true;
            }
            else
            {
                _failedCounter.AddFailedCount(userAccount);

                var failedCount = _failedCounter.GetFailedCount(userAccount);
                _nLogAdapter.LogMessage($"accountId:{userAccount} failed times:{failedCount}");

                _slackAdapter.Post("Er");
            }

            return false;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}
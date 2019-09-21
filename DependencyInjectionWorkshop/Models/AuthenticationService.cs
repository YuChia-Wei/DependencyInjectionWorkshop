﻿using System;
using DependencyInjectionWorkshop.Models.FailedCounter;
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
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly INotification _notify;
        private readonly ILogger _logger;
        private readonly IFailedCounter _failedCounter;

        public AuthenticationService()
        {
            _profile = new ProfileDbo();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            _notify = new SlackAdapter();
            _logger = new NLogAdapter();
            _failedCounter = new FailedCounter.FailedCounter();
        }

        public AuthenticationService(IFailedCounter failedCounter, ILogger logger, IOtpService otpService,
            IProfile profile, IHash hash, INotification notification)
        {
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
            _notify = notification;
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
                _failedCounter.ResetFailedCount(userAccount);
                return true;
            }
            else
            {
                _failedCounter.AddFailedCount(userAccount);

                var failedCount = _failedCounter.GetFailedCount(userAccount);
                _logger.Info($"accountId:{userAccount} failed times:{failedCount}");

                _notify.Send("Er");
            }

            return false;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}
using System;
using DependencyInjectionWorkshop.Models.Hash;
using DependencyInjectionWorkshop.Models.Otp;
using DependencyInjectionWorkshop.Models.Profile;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService : IAuthentication
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;

        public AuthenticationService()
        {
            _profile = new ProfileDbo();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
        }

        public AuthenticationService(IOtpService otpService,
            IProfile profile, IHash hash)
        {
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
        }

        public bool Verify(string userAccount, string password, string otp)
        {
            var passwordfordb = _profile.GetPassword(userAccount);

            string hashedPsw = _hash.Compute(password);

            var currentOtp = _otpService.GetCurrentOtp(userAccount);

            return passwordfordb == hashedPsw && otp == currentOtp;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}
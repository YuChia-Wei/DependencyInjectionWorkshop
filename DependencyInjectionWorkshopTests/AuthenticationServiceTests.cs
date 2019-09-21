using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Models.FailedCounter;
using DependencyInjectionWorkshop.Models.Hash;
using DependencyInjectionWorkshop.Models.LogService;
using DependencyInjectionWorkshop.Models.Notify;
using DependencyInjectionWorkshop.Models.Otp;
using DependencyInjectionWorkshop.Models.Profile;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [Test]
        public void is_valid()
        {
            var profile = Substitute.For<IProfile>();
            var hash = Substitute.For<IHash>();
            var otpService = Substitute.For<IOtpService>();
            var failedCouter = Substitute.For<IFailedCounter>();
            var logger = Substitute.For<ILogger>();
            var notification = Substitute.For<INotify>();

            var authenticationService =
                new AuthenticationService(profile, hash, otpService, notification, logger, failedCouter);

            profile.GetPassword("joey").Returns("my hashed password");
            hash.Hash("abc").Returns("my hashed password");
            otpService.GetCurrentOtp("joey").Returns("123456");

            var isValid = authenticationService.Verify("joey", "abc", "123456");

            Assert.IsTrue(isValid);
        }
    }
}
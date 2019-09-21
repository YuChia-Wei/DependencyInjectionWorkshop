using System;
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
        private const string DefaultAccountId = "joey";
        private const string DefaultHashedPassword = "my hashed password";
        private const string DefaultInputPassword = "abc";
        private const string DefaultOtp = "123456";
        private const int DefaultFailedCount = 88;
        private AuthenticationService _authenticationService;
        private IFailedCounter _failedCounter;
        private IHash _hash;
        private ILogger _logger;
        private INotification _notification;
        private IOtpService _otpService;
        private IProfile _profile;

        [SetUp]
        public void SetUp()
        {
            _notification = Substitute.For<INotification>();
            _logger = Substitute.For<ILogger>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _otpService = Substitute.For<IOtpService>();
            _hash = Substitute.For<IHash>();
            _profile = Substitute.For<IProfile>();
            _authenticationService =
                new AuthenticationService(_failedCounter, _logger, _otpService, _profile, _hash, _notification);
        }

        [Test]
        public void is_valid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultInputPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultInputPassword, DefaultOtp);

            ShouldBeValid(isValid);
        }

        [Test]
        public void is_invalid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultInputPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultInputPassword, "wrong otp");

            ShouldBeInvalid(isValid);
        }

        [Test]
        public void reset_failed_count_when_valid()
        {
            WhenValid();
            ShouldResetFailedCount(DefaultAccountId);
        }

        [Test]
        public void add_failed_count_when_invalid()
        {
            WhenInvalid();
            ShouldAddFailedCount(DefaultAccountId);
        }

        [Test]
        public void log_failed_count_when_invalid()
        {
            GivenFailedCount(DefaultAccountId, DefaultFailedCount);
            WhenInvalid();
            LogShouldContains(DefaultAccountId, DefaultFailedCount.ToString());
        }

        [Test]
        public void notify_user_when_invalid()
        {
            WhenInvalid();
            ShouldNotify(DefaultAccountId);
        }

        [Test]
        public void account_is_locked()
        {
            GivenAccountIsLocked(DefaultAccountId, true);
            ShouldThrow<FailedTooManyTimesException>();
        }

        private static void ShouldBeInvalid(bool isValid)
        {
            Assert.IsFalse(isValid);
        }

        private static void ShouldBeValid(bool isValid)
        {
            Assert.IsTrue(isValid);
        }

        private void ShouldThrow<TException>() where TException : Exception
        {
            TestDelegate action = () => WhenVerify(DefaultAccountId, DefaultInputPassword, DefaultOtp);
            Assert.Throws<TException>(action);
        }

        private void GivenAccountIsLocked(string accountId, bool isLocked)
        {
            _failedCounter.GetAccountIsLocked(accountId).Returns(isLocked);
        }

        private void ShouldNotify(string accountId)
        {
            _notification.Received(1).Send(accountId);
        }

        private void LogShouldContains(string accountId, string failedCount)
        {
            _logger.Received(1).Info(
                Arg.Is<string>(m => m.Contains(accountId) && m.Contains(failedCount)));
        }

        private void GivenFailedCount(string accountId, int failedCount)
        {
            _failedCounter.GetFailedCount(accountId).Returns(failedCount);
        }

        private void ShouldAddFailedCount(string accountId)
        {
            _failedCounter.Received(1).AddFailedCount(accountId);
        }

        private bool WhenInvalid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultInputPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultInputPassword, "wrong otp");
            return isValid;
        }

        private void ShouldResetFailedCount(string accountId)
        {
            _failedCounter.Received(1).ResetFailedCount(accountId);
        }

        private bool WhenValid()
        {
            GivenPassword(DefaultAccountId, DefaultHashedPassword);
            GivenHash(DefaultInputPassword, DefaultHashedPassword);
            GivenOtp(DefaultAccountId, DefaultOtp);

            var isValid = WhenVerify(DefaultAccountId, DefaultInputPassword, DefaultOtp);
            return isValid;
        }

        private bool WhenVerify(string accountId, string password, string otp)
        {
            return _authenticationService.Verify(accountId, password, otp);
        }

        private void GivenOtp(string accountId, string otp)
        {
            _otpService.GetCurrentOtp(accountId).Returns(otp);
        }

        private void GivenHash(string inputPassword, string hashedPassword)
        {
            _hash.Compute(inputPassword).Returns(hashedPassword);
        }

        private void GivenPassword(string accountId, string password)
        {
            _profile.GetPassword(accountId).Returns(password);
        }
    }
}
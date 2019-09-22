using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionWorkshop.Models;
using DependencyInjectionWorkshop.Models.Decorator;
using DependencyInjectionWorkshop.Models.FailedCounter;
using DependencyInjectionWorkshop.Models.Hash;
using DependencyInjectionWorkshop.Models.LogService;
using DependencyInjectionWorkshop.Models.Notify;
using DependencyInjectionWorkshop.Models.Otp;
using DependencyInjectionWorkshop.Models.Profile;

namespace MyConsole
{

    class Program
    {
        private static INotification _notification;
        private static ILogger _logger;
        private static IOtpService _otpService;
        private static IFailedCounter _failedCounter;
        private static IHash _hash;
        private static IProfile _profile;
        private static IAuthentication _authentication;

        static void Main(string[] args)
        {
            _notification = new FakeSlack();
            _logger = new FakeLogger();
            _failedCounter = new FakeFailedCounter();
            _otpService = new FakeOtp();
            _hash = new FakeHash();
            _profile = new FakeProfile();
            _authentication =
                new AuthenticationService(_otpService, _profile, _hash);

            _authentication = new NotificationDecorator(_authentication, _notification);
            _authentication = new FailedCounterDecorator(_authentication, _failedCounter);
            _authentication = new LogFailedCountDecorator(_authentication, _failedCounter, _logger);

            var isValid = _authentication.Verify("joey", "abc", "wrong otp");
            Console.WriteLine($"result:{isValid}");
        }
    }

    internal class FakeLogger : ILogger
    {
        public void Info(string message)
        {
            Console.WriteLine(message);
        }
    }

    internal class FakeSlack : INotification
    {
        public void PushMessage(string message)
        {
            Console.WriteLine(message);
        }

        public void Send(string accountId)
        {
            PushMessage($"{nameof(Send)}, accountId:{accountId}");
        }
    }

    internal class FakeFailedCounter : IFailedCounter
    {
        public void ResetFailedCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(ResetFailedCount)}({accountId})");
        }

        public void AddFailedCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(AddFailedCount)}({accountId})");
        }

        public bool GetAccountIsLocked(string accountId)
        {
            return IsAccountLocked(accountId);
        }

        public int GetFailedCount(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(GetFailedCount)}({accountId})");
            return 91;
        }

        public bool IsAccountLocked(string accountId)
        {
            Console.WriteLine($"{nameof(FakeFailedCounter)}.{nameof(IsAccountLocked)}({accountId})");
            return false;
        }
    }

    internal class FakeOtp : IOtpService
    {
        public string GetCurrentOtp(string accountId)
        {
            Console.WriteLine($"{nameof(FakeOtp)}.{nameof(GetCurrentOtp)}({accountId})");
            return "123456";
        }
    }

    internal class FakeHash : IHash
    {
        public string Compute(string plainText)
        {
            Console.WriteLine($"{nameof(FakeHash)}.{nameof(Compute)}({plainText})");
            return "my hashed password";
        }
    }

    internal class FakeProfile : IProfile
    {
        public string GetPassword(string accountId)
        {
            Console.WriteLine($"{nameof(FakeProfile)}.{nameof(GetPassword)}({accountId})");
            return "my hashed password";
        }
    }
}

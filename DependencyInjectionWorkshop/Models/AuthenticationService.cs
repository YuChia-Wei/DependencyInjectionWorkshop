using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class ProfileDbo
    {
        public string GetPasswordFormDB(string userAccount)
        {
            string passwordfordb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordfordb = connection.Query<string>("spGetUserPassword", new { Id = userAccount },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return passwordfordb;
        }
    }

    public class Sha256Adapter
    {
        public Sha256Adapter()
        {
        }

        public string GetHashedPassword(string psw)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(psw));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            return hash.ToString();
        }
    }

    public class OtpService
    {
        public OtpService()
        {
        }

        public string GetCurrentOtp(string userAccount)
        {
            string currentOtp;
            var response = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/otps", userAccount).Result;
            if (response.IsSuccessStatusCode)
            {
                currentOtp = response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api error, accountId:{userAccount}");
            }

            return currentOtp;
        }
    }

    public class SlackAdapter
    {
        public SlackAdapter()
        {
        }

        public void NotifyToSlack(string messageText)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", messageText, "my bot name");
        }
    }

    public class NLogAdapter
    {
        public NLogAdapter()
        {
        }

        public void LogMessage(string s)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(s);
        }
    }

    public class FailedCounter
    {
        public FailedCounter()
        {
        }

        public int GetFailedCount(string userAccount)
        {
            var failedCountResponse =
                new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/GetFailedCount", userAccount).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        public void AddFailedCount(string userAccount)
        {
            var addFailedCountResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Add", userAccount).Result;

            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public void ResetFailedCount(string userAccount)
        {
            var resetResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/Reset", userAccount).Result;

            resetResponse.EnsureSuccessStatusCode();
        }

        public bool GetUserLockedStatus(string userAccount)
        {
            var isLockedResponse = new HttpClient() { BaseAddress = new Uri("http://joey.com/") }.PostAsJsonAsync("api/failedCounter/IsLocked", userAccount).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            var UserIsLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return UserIsLocked;
        }
    }

    public class AuthenticationService
    {
        private readonly ProfileDbo _profileDbo;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpService _otpService;
        private readonly SlackAdapter _slackAdapter;
        private readonly NLogAdapter _nLogAdapter;
        private readonly FailedCounter _failedCounter;

        public AuthenticationService()
        {
            _profileDbo = new ProfileDbo();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _slackAdapter = new SlackAdapter();
            _nLogAdapter = new NLogAdapter();
            _failedCounter = new FailedCounter();
        }

        public bool Verify(string userAccount, string password, string otp)
        {
            var UserIsLocked = _failedCounter.GetUserLockedStatus(userAccount);
            if (UserIsLocked)
            {
                throw new FailedTooManyTimesException();
            }

            var passwordfordb = _profileDbo.GetPasswordFormDB(userAccount);

            string hashedPsw = _sha256Adapter.GetHashedPassword(password);

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
                _nLogAdapter.LogMessage( $"accountId:{userAccount} failed times:{failedCount}");

                _slackAdapter.NotifyToSlack("Er");
            }

            return false;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}
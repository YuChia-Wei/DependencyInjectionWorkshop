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
    public class AuthenticationService
    {
        public bool Verify(string userAccount, string psw, string otp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };

            #region 驗證是否被鎖

            var UserIsLocked = GetUserLockedStatus(userAccount, httpClient);
            if (UserIsLocked)
            {
                throw new FailedTooManyTimesException();
            }

            #endregion 驗證是否被鎖

            #region Get Psw

            var passwordfordb = GetPasswordFormDB(userAccount);

            #endregion Get Psw

            #region Get hash

            var hash = GetHashedPassword(psw);
            string hashedPsw = hash.ToString();

            #endregion Get hash

            #region Get Otp

            string currentOtp;
            var response = httpClient.PostAsJsonAsync("api/otps", userAccount).Result;
            if (response.IsSuccessStatusCode)
            {
                currentOtp = response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api error, accountId:{userAccount}");
            }

            #endregion Get Otp

            if (passwordfordb == hashedPsw && otp == currentOtp)
            {
                ResetFailedCount(userAccount, httpClient);
                return true;
            }
            else
            {
                #region 驗證失敗要紀錄失敗次數

                AddFailedCount(userAccount, httpClient);

                #endregion 驗證失敗要紀錄失敗次數

                #region 用 NLog 紀錄失敗訊息

                var failedCount = GetFailedCount(userAccount, httpClient);
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Info($"accountId:{userAccount} failed times:{failedCount}");

                #endregion 用 NLog 紀錄失敗訊息

                var slackClient = new SlackClient("my api token");
                slackClient.PostMessage(response1 => { }, "my channel", "ERR", "my bot name");
            }

            return false;
        }

        private static int GetFailedCount(string userAccount, HttpClient httpClient)
        {
            var failedCountResponse =
                httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", userAccount).Result;

            failedCountResponse.EnsureSuccessStatusCode();

            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        private static void AddFailedCount(string userAccount, HttpClient httpClient)
        {
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", userAccount).Result;

            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        private static void ResetFailedCount(string userAccount, HttpClient httpClient)
        {
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", userAccount).Result;

            resetResponse.EnsureSuccessStatusCode();
        }

        private static StringBuilder GetHashedPassword(string psw)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(psw));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            return hash;
        }

        private static bool GetUserLockedStatus(string userAccount, HttpClient httpClient)
        {
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", userAccount).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            var UserIsLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return UserIsLocked;
        }

        private static string GetPasswordFormDB(string userAccount)
        {
            string passwordfordb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordfordb = connection.Query<string>("spGetUserPassword", new {Id = userAccount},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return passwordfordb;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}
﻿using System;
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

            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", userAccount).Result;

            isLockedResponse.EnsureSuccessStatusCode();
            if (isLockedResponse.Content.ReadAsAsync<bool>().Result)
            {
                throw new FailedTooManyTimesException();
            }

            #endregion 驗證是否被鎖

            #region Get Psw

            string passwordfordb;
            using (var connection = new SqlConnection("my connection string"))
            {
                passwordfordb = connection.Query<string>("spGetUserPassword", new { Id = userAccount },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            #endregion Get Psw

            #region Get hash

            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(psw));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
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
                #region 驗證成功要重設失敗次數

                var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", userAccount).Result;

                resetResponse.EnsureSuccessStatusCode();

                #endregion 驗證成功要重設失敗次數

                return true;
            }
            else
            {
                #region 驗證失敗要紀錄失敗次數

                var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", userAccount).Result;

                addFailedCountResponse.EnsureSuccessStatusCode();

                #endregion 驗證失敗要紀錄失敗次數

                var slackClient = new SlackClient("my api token");
                slackClient.PostMessage(response1 => { }, "my channel", "ERR", "my bot name");
            }

            return false;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}
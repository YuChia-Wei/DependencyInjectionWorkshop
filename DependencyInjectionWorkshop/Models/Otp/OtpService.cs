using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models.Otp
{
    public class OtpService : IOtpService
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
}
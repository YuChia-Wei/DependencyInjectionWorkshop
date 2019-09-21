using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models.FailedCounter
{
    public class FailedCounter : IFailedCounter
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
}
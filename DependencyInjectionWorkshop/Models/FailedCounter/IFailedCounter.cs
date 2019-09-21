namespace DependencyInjectionWorkshop.Models.FailedCounter
{
    public interface IFailedCounter
    {
        int GetFailedCount(string userAccount);
        void AddFailedCount(string userAccount);
        void ResetFailedCount(string userAccount);
        bool GetUserLockedStatus(string userAccount);
    }
}
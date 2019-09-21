namespace DependencyInjectionWorkshop.Models.Notify
{
    public interface INotification
    {
        void Post(string messageText);
    }
}
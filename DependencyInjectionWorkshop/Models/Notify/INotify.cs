namespace DependencyInjectionWorkshop.Models.Notify
{
    public interface INotification
    {
        void Send(string messageText);
    }
}
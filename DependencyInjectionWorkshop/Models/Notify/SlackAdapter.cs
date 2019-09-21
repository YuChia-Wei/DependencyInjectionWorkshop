using SlackAPI;

namespace DependencyInjectionWorkshop.Models.Notify
{
    public class SlackAdapter : INotification
    {
        public SlackAdapter()
        {
        }

        public void Send(string messageText)
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(response1 => { }, "my channel", messageText, "my bot name");
        }
    }
}
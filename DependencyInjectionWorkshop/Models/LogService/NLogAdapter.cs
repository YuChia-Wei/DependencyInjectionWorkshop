namespace DependencyInjectionWorkshop.Models.LogService
{
    public class NLogAdapter : ILogger
    {
        public NLogAdapter()
        {
        }

        public void Info(string s)
        {
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(s);
        }
    }
}
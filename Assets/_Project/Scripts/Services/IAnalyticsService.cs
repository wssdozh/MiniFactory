using System.Collections.Generic;

namespace MiniFactory.Services
{
    public interface IAnalyticsService
    {
        void AddProvider(IAnalyticsProvider provider);
        void LogEvent(string eventName, Dictionary<string, object> parameters = null);
    }
}

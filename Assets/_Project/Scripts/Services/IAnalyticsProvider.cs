using System.Collections.Generic;

namespace MiniFactory.Services
{
    public interface IAnalyticsProvider
    {
        void LogEvent(string eventName, Dictionary<string, object> parameters = null);
    }
}

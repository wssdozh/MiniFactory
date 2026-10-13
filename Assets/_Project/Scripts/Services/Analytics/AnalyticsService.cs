using System.Collections.Generic;

namespace MiniFactory.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly List<IAnalyticsProvider> _providers = new();

        public AnalyticsService(params IAnalyticsProvider[] providers)
        {
            if (providers != null)
            {
                foreach (IAnalyticsProvider provider in providers)
                {
                    if (provider != null)
                        _providers.Add(provider);
                }
            }
        }

        public void AddProvider(IAnalyticsProvider provider)
        {
            if (provider != null && _providers.Contains(provider) == false)
                _providers.Add(provider);
        }

        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (string.IsNullOrEmpty(eventName))
                return;

            for (int i = 0; i < _providers.Count; i++)
            {
                _providers[i]?.LogEvent(eventName, parameters);
            }
        }
    }
}

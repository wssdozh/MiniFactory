using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MiniFactory.Services
{
    public class DebugAnalyticsProvider : IAnalyticsProvider
    {
        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (parameters != null && parameters.Count > 0)
            {
                string paramString = string.Join(", ", parameters.Select(kv => $"{kv.Key}={kv.Value}"));
                Debug.Log($"[Analytics] {eventName}: {paramString}");
            }
            else
            {
                Debug.Log($"[Analytics] {eventName}");
            }
        }
    }
}

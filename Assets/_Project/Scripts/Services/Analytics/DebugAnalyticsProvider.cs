using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace MiniFactory.Services
{
    public class DebugAnalyticsProvider : IAnalyticsProvider
    {
        private readonly StringBuilder _sb = new();

        public void LogEvent(string eventName, Dictionary<string, object> parameters = null)
        {
            if (parameters != null && parameters.Count > 0)
            {
                _sb.Clear();
                _sb.Append("[Analytics] ").Append(eventName).Append(": ");
                bool first = true;
                foreach (KeyValuePair<string, object> kv in parameters)
                {
                    if (first == false)
                        _sb.Append(", ");

                    _sb.Append(kv.Key).Append('=').Append(kv.Value);
                    first = false;
                }
                Debug.Log(_sb.ToString());
            }
            else
            {
                Debug.Log($"[Analytics] {eventName}");
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace MiniFactory.Feedback
{
    [RequireComponent(typeof(AudioSource))]
    public class FeedbackRoot : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private List<FeedbackEntry> entries = new List<FeedbackEntry>();

        private Dictionary<FeedbackType, FeedbackEntry> _entryLookup;

        private void Awake()
        {
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();

            _entryLookup = new Dictionary<FeedbackType, FeedbackEntry>();
            if (entries != null)
            {
                for (int i = 0; i < entries.Count; i++)
                {
                    FeedbackEntry entry = entries[i];
                    if (entry != null && _entryLookup.ContainsKey(entry.Type) == false)
                        _entryLookup.Add(entry.Type, entry);
                }
            }
        }

        public void Play(FeedbackType type)
        {
            if (audioSource == null || _entryLookup == null)
                return;

            if (_entryLookup.TryGetValue(type, out FeedbackEntry entry) && entry.Clip != null)
                audioSource.PlayOneShot(entry.Clip, entry.Volume);
        }
    }
}

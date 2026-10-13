using System;
using UnityEngine;

namespace MiniFactory.Feedback
{
    [Serializable]
    public class FeedbackEntry
    {
        [SerializeField] private FeedbackType type;
        [SerializeField] private AudioClip clip;
        [SerializeField] [Range(0f, 1f)] private float volume = 1f;

        public FeedbackType Type => type;
        public AudioClip Clip => clip;
        public float Volume => volume;

        public FeedbackEntry(FeedbackType type, AudioClip clip, float volume = 1f)
        {
            this.type = type;
            this.clip = clip;
            this.volume = volume;
        }
    }
}

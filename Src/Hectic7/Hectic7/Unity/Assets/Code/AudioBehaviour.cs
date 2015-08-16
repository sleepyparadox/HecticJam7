using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Hectic7
{
    public enum AudioTrack
    {
        None,
        Menu,
        Combat,
    }

    public class AudioBehaviour : MonoBehaviour
    {
        public AudioTrack CurrentTrack { get; private set; }
        public float DefaultVolume = 1f;
        public AudioSource MenuMusic;
        public AudioSource CombatMusic;

        public void SetTrack(AudioTrack track)
        {
            if (track == CurrentTrack)
                return;

            MenuMusic.volume = track == AudioTrack.Menu ? DefaultVolume : 0f;
            CombatMusic.volume = track == AudioTrack.Combat ? DefaultVolume : 0f;

            CurrentTrack = track;
        }
    }
}

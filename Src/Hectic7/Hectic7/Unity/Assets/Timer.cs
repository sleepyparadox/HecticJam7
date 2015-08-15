using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Hectic7
{
    public class Timer : UnityObject
    {
        public float Time
        {
            get { return _time; }
            set
            {
                _time = value;
                _timerText.text = Mathf.CeilToInt(_time).ToString();
            }
        }

        float _time;
        private TextMesh _timerText;

        public Timer()
            : base(Assets.TimerPrefab)
        {
            _timerText = FindChildComponent<TextMesh>("Text");
        }

    }
}

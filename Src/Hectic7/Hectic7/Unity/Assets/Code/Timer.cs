using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Hectic7
{
    public class Timer : UnityObject
    {

        public bool InfiniteTime
        {
            set
            {
                _infTime = value;
                if(_infTime)
                {
                    _timerText.text = "inf";
                }
                else
                {
                    Time = Time;
                }
            }
        }

        public float Time
        {
            get { return _time; }
            set
            {
                _infTime = false;
                _time = value;
                _timerText.text = Mathf.CeilToInt(_time).ToString();
            }
        }

        bool _infTime;
        float _time;
        private TextMesh _timerText;

        public Timer()
            : base(Assets.TimerPrefab)
        {
            _timerText = new UnityObject(FindChild("Line")).FindChildComponent<TextMesh>("Text");
        }

    }
}

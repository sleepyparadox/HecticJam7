using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Hectic7
{
    public class MenuItem : UnityObject
    {
        public TextMesh TextMesh;
        public bool Empty { get { return string.IsNullOrEmpty(TextMesh.text); } }
        public Action OnClick;
        public GetTipFunc GenerateToolTip;

        public MenuItem(GameObject gameObject)
            : base(gameObject)
        {
            TextMesh = FindChildComponent<TextMesh>("Text");
            TextMesh.text = string.Empty;
        }

        public void Set(string text, Action onClick, GetTipFunc generateToolTip = null)
        {
            TextMesh.text = text;
            OnClick = onClick;
            GenerateToolTip = generateToolTip;
        }
    }
}

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

        public MenuItem(GameObject gameObject)
            : base(gameObject)
        {
            Debug.Log(gameObject.name + " looking for child Text");
            TextMesh = FindChildComponent<TextMesh>("Text");
            TextMesh.text = string.Empty;
        }

        public void Set(string text, Action onClick)
        {
            TextMesh.text = text;
            OnClick = onClick;
        }
    }
}

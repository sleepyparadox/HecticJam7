using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Hectic7
{
    public delegate string[] GetTipFunc();
    public class ToolTip : UnityObject
    {
        private List<TextMesh> _items;
        public ToolTip()
            : base(Assets.Dialogs.TipPopupPrefab)
        {
            _items = new List<TextMesh>();
            for (var i = 0; i < 100; ++i)
            {
                var child = FindChild("Line" + i);
                if (child == null)
                    break;
                _items.Add(child.GetComponentsInChildren<TextMesh>().First());
            }
        }

        public void SetText(string[] text)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (i < text.Length)
                    _items[i].text = text[i];
                else
                    _items[i].text = string.Empty;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityTools_4_6;

namespace Hectic7
{
    public class DialogPopup : UnityObject
    {
        public static List<KeyCode> _enterKeys = new List<KeyCode>() { KeyCode.Z, KeyCode.Space, KeyCode.Return };
        public static List<KeyCode> _backKeys = new List<KeyCode>() { KeyCode.X, KeyCode.Backspace, KeyCode.Escape };

        public static List<DialogPopup> Stack = new List<DialogPopup>();
        public List<MenuItem> _items;
        private bool _canBack;
        public int _index;
        private GameObject _cursor;
        float _lastVisibleAt;
        bool _wasVisible = false;
        int _oldIndex = -1;

        public event Action OnFixedClick;
        public bool FixedInputDisabled = false;
        private ToolTip _toolTip;

        public MenuItem this[int i] { get { return _items[i]; } }

        public string Title
        {
            set
            {
                var title = FindChild("Title");
                if(title == null)
                {
                    Debug.LogWarning("Title not found");
                    return;
                }
                var text = title.GetComponentsInChildren<TextMesh>().First();
                text.text = value;
            }
        }

        public DialogPopup(PrefabAsset popup, bool canBack = true, bool fixedCursor = false, string name = null)
            : base(popup)
        {
            if (!string.IsNullOrEmpty(name))
                GameObject.name = name;
            _canBack = canBack;
            Stack.Add(this);
            OnDispose += (me) => Debug.Log("Disposing " + GameObject.name);
            OnDispose += (me) => Stack.Remove(this);

            var depth = Stack.Any() ? Stack.Min(d => d.WorldPosition.z) : 0;
            depth -= 25;
            WorldPosition = new Vector3(WorldPosition.x, WorldPosition.y, depth);

            _toolTip = new ToolTip();
            _toolTip.WorldPosition = new Vector3(_toolTip.WorldPosition.x, _toolTip.WorldPosition.y, depth);
            _toolTip.SetActive(false);
            _toolTip.Parent = this;

            _items = new List<MenuItem>();
            for(var i = 0; i < 100; ++i)
            {
                var child = FindChild("Line" + i);
                if (child == null)
                    break;
                _items.Add(new MenuItem(child));
            }
            _cursor = FindChild("Cursor");

            if (fixedCursor)
            {
                UnityUpdate += HandleFixedCursorInput;
            }
            else
            {
                UnityUpdate += HandleChoiceInput;
            }
        }
        void HandleFixedCursorInput(UnityObject me)
        {
            _toolTip.SetActive(false);
            if (FixedInputDisabled
                || HiddenOrStunned())
            {
                _cursor.SetActive(false);
                return;
            }
            
            _cursor.SetActive(Time.time % 1f > 0.5f);

            if(_enterKeys.Any(key => Input.GetKeyUp(key))
                || _backKeys.Any(key => Input.GetKeyUp(key)))
            {
                if (OnFixedClick != null)
                {
                    OnFixedClick();
                }
            }
        }

        bool HiddenOrStunned()
        {
            var visible = Stack.Last() == this;
            var justBecameVisible = visible && !_wasVisible;
            _wasVisible = visible;

            if (justBecameVisible)
                _lastVisibleAt = Time.time;

            return !visible || justBecameVisible;
        }

        void HandleChoiceInput(UnityObject me)
        {
            if(HiddenOrStunned())
            {
                _toolTip.SetActive(false);
                _cursor.SetActive(false);
                return;
            }

            //Exit
            if (_canBack
                && _backKeys.Any(key => Input.GetKeyUp(key)))
            {
                Dispose();
                return;
            }
           
            if (Stack.Last() != this)
                _lastVisibleAt = Time.time;
                        
            var optionCount = _items.Count(item => !item.Empty);
            if (optionCount == 0)
            {
                _index = 0;
                _cursor.SetActive(false);
            }
            else if (optionCount == 1)
            {
                _cursor.SetActive(true);
                _index = _items.FirstIndex(item => !item.Empty);
            }
            else
            {
                _cursor.SetActive(true);

                if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W))
                {
                    _index = (_index - 1).WrapToEnd(_items.Count);

                    while (_items[_index].Empty)
                    {
                        _index = (_index - 1).WrapToEnd(_items.Count);
                    }
                }

                //Nav up
                if (Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S))
                {
                    _index = (_index + 1).WrapToEnd(_items.Count);

                    while (_items[_index].Empty)
                    {
                        _index = (_index + 1).WrapToEnd(_items.Count);
                    }
                }
            }

            _cursor.transform.position = _items[_index].WorldPosition + new Vector3(-8, /*All lines are bad hack*/-8, 0);

            if (_items[_index].GenerateToolTip != null) //TODO HACK
               // && (_index != _oldIndex || !_toolTip.GameObject.activeInHierarchy))
            {
                var tipText = _items[_index].GenerateToolTip();
                _toolTip.SetText(tipText);
                _toolTip.SetActive(true);
            }
            else// if (_items[_index].GenerateToolTip == null)
            {
                _toolTip.SetActive(false);
            }

            //Enter
            if (optionCount > 0
                && _enterKeys.Any(key => Input.GetKeyUp(key)))
            {
                if (!_items[_index].Empty
                    && _items[_index].OnClick != null)
                {
                    Debug.Log("On click " + _items[_index].TextMesh.text + " in " + GameObject.name + ", " + this);
                    _items[_index].OnClick();
                }
            }

            _oldIndex = _index;
        }
    }
}

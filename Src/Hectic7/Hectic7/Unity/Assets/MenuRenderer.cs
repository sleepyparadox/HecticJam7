using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Hectic7
{
    public class MenuRenderer : UnityObject
    {
        private List<KeyCode> _enterKeys = new List<KeyCode>() { KeyCode.Z, KeyCode.Space, KeyCode.Return };
        private GameObject _cursor;
        private List<TextMesh> _options;
        private Menu _targetMenu;
        private TextMesh _title;
        private int _index;

        public MenuRenderer()
            : base(Assets.MenuPrefab)
        {
            _options = new List<UnityEngine.TextMesh>();
            for (var i = 0; i < 4; ++i)
            {
                var option = FindChildComponent<TextMesh>("Option" + i);
                _options.Add(option);
            }
            _title = FindChildComponent<TextMesh>("Title");
            _cursor = FindChild("Cursor");

            UnityUpdate += HandleInput;
            UnityUpdate += UpdateDisplay;

            UpdateDisplay(this);
        }

        public void Render(Menu menu)
        {
            Debug.Log("Clear");
            _targetMenu = menu;
        }

        public void Clear()
        {
            Debug.Log("Clear");
            _targetMenu = null;
        }

        void HandleInput(UnityObject me)
        {
            if(_targetMenu == null)
            {
                return;
            }
            var optionCount = _targetMenu.Items.Count(item => !item.Empty);
            if (optionCount == 0)
            {
                _index = 0;
                _cursor.SetActive(false);
            }
            else if (optionCount == 1)
            {
                _index = _targetMenu.Items.FirstIndex(item => !item.Empty);
                _cursor.SetActive(false);
            }
            else
            {
                if (Input.GetKeyUp(KeyCode.UpArrow))
                {
                    _index = (_index - 1).WrapToEnd(Menu.ItemCount);
                    
                    while(_targetMenu[_index].Empty)
                    {
                        _index = (_index - 1).WrapToEnd(Menu.ItemCount);
                    }
                }

                //Nav up
                if (Input.GetKeyUp(KeyCode.DownArrow))
                {
                    Debug.Log("DownArrow");
                    _index = (_index + 1).WrapToEnd(Menu.ItemCount);

                    while (_targetMenu[_index].Empty)
                    {
                        _index = (_index + 1).WrapToEnd(Menu.ItemCount);
                    }
                }
            }

            //Enter
            if (optionCount > 0
                && _enterKeys.Any(key => Input.GetKeyUp(key)))
            {
                if (!_targetMenu[_index].Empty
                    && _targetMenu[_index].OnClick != null)
                {
                    _targetMenu[_index].OnClick();
                }
            }
        }

        void UpdateDisplay(UnityObject me)
        {
            if (_targetMenu == null)
            {
                _title.gameObject.SetActive(false);
                for (int i = 0; i < _options.Count; i++)
                {
                    _options[i].gameObject.SetActive(false);
                }
                _cursor.SetActive(false);
            }
            else
            {
                _title.gameObject.SetActive(true);
                for (int i = 0; i < _options.Count; i++)
                {
                    _options[i].gameObject.SetActive(true);
                }

                _title.text = _targetMenu.Title;
                for (int i = 0; i < _options.Count; i++)
                {
                    _options[i].text = _targetMenu[i].Text;
                }
                _cursor.transform.position = _options[_index].transform.position + new Vector3(-4.5f, 0, 0);

                _cursor.SetActive(true);

                if (_targetMenu[_index].Empty)
                {
                    _cursor.SetActive(false);
                }
                else
                {
                    _cursor.SetActive(true);
                    if (_targetMenu[_index].OnClick != null)
                    {
                        _cursor.GetComponent<Renderer>().material.color = Color.white;
                    }
                    else
                    {
                        _cursor.GetComponent<Renderer>().material.color = Color.grey;
                    }
                }
            }
        }
    }

    public static class IEnumerableHelper
    {
        public static int FirstIndex<T>(this IEnumerable<T> col, Func<T, bool> match = null)
        {
            var index = 0;
            foreach(var item in col)
            {
                if (match(item))
                    return index;
                index++;
            }

            return -1;
        }
        
        public static int WrapToEnd(this int val, int max)
        {
            if (max > 0)
            {
                if (val < 0)
                    return max - 1;

                if (val >= max)
                    return 0;
            }

            return val;
        }
    }
   
}

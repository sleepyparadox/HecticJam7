using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hectic7
{
    public class Menu
    {
        public string Title { get; set; }
        public const int ItemCount = 4;
        public IEnumerable<MenuItem> Items { get { return _items; } }
        public MenuItem this[int i] { get { return _items[i]; } }

        MenuItem[] _items;
        public Menu(string title = null)
        {
            Title = title;
            _items = new MenuItem[ItemCount];
            for (int i = 0; i < _items.Length; i++)
            {
                _items[i] = new MenuItem();
            }
        }
    }
    public class MenuItem
    {
        public bool Empty { get { return string.IsNullOrEmpty(Text); } }
        public string Text;
        public Action OnClick;
        public void Set(string text, Action onClick)
        {
            Text = text;
            OnClick = onClick;
        }
    }
}

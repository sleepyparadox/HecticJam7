using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityTools_4_6;

namespace Hectic7
{
    public class DialogPicker<T> : DialogPopup
    {
        public event Action<T> OnItemPicked;
        public DialogPicker(T[] pickData, string customFieldName = null)
            : base(Assets.Dialogs.BigDialogPrefab)
        {
            for (int i = 0; i < pickData.Length; i++)
            {
                var temp = i;
                this[temp].Set(pickData[temp].ToString(), () =>
                {
                    OnItemPicked(pickData[temp]);
                });
            }

            if (string.IsNullOrEmpty(customFieldName))
            {
                customFieldName = typeof(T).Name;
            }

            var pickCoro = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(new[] { "Pick a " + typeof(T).Name }));
            OnDispose += (u) => pickCoro.Kill();
        }
    }

    public class FieldEntry
    {
        public string CustomName;
        public Type EnumType;
        public int? Choice;
        public int DefaultIndex = 0;
        public bool ShowIntro = true;
        public FieldEntry(Type enumType, string customName = null)
        {
            EnumType = enumType;
            CustomName = customName;
        }
    }

    public class DialogEnumPicker : DialogPopup
    {
        public Action<int> OnEnumValPicked;
        public DialogEnumPicker(Type enumType, bool showIntro, string enumName = null, int defaultIndex = 0)
            : base(Assets.Dialogs.BigDialogPrefab)
        {
            var pickData = Enum.GetValues(enumType);
            for (int i = 0; i < pickData.Length; i++)
            {
                var temp = i;
                var choiceName = Enum.GetName(enumType, i);
                this[temp].Set(choiceName.ToString(), () =>
                {
                    OnEnumValPicked(temp);
                    Dispose();
                });
            }

            _index = defaultIndex;

            if (string.IsNullOrEmpty(enumName))
            {
                enumName = enumType.Name;
            }

            enumName = enumName.Replace("Bullet", "");

            Title = enumName;

            if (showIntro)
            {
                var pickCoro = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(new[] { "Pick a " + enumName }));
                OnDispose += (u) => pickCoro.Kill();
            }
        }
    }
}
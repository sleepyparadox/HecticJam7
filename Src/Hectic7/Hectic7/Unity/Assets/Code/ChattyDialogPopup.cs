using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityTools_4_6;

namespace Hectic7
{
    public class ChattyDialog
    {
        public static IEnumerator DoChattyDialog(string[] text)
        {
            var consumableText = text.ToList();

            while (consumableText.Any())
            {
                var dialog = new DialogPopup(Assets.Dialogs.MainDialogPrefab, canBack: false, fixedCursor: true);
                dialog.FixedInputDisabled = true;
                dialog.OnFixedClick += dialog.Dispose;

                //Consiome up to 4 lines
                var lines = consumableText.Take(4).ToList();
                consumableText.RemoveRange(0, lines.Count);
                var i = 0;
                foreach (var line in lines)
                {
                    dialog[i].Set(line.Replace("#", ""), null);
                    i++;

                    yield return null;
                }

                if (lines.Any(l => l.Contains("#")))
                    dialog.WorldPosition += new UnityEngine.Vector3(0, 64, 0);

                //Wait for next to be pressed
                dialog.FixedInputDisabled = false;
                yield return TinyCoro.WaitUntil(() => !dialog.NotDisposed);
            }
        }
    }
}

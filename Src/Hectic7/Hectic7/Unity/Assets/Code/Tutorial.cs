using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityTools_4_6;

namespace Hectic7
{
    public class Tutorial
    {
        public static IEnumerator DoTutorial()
        {
            var reachedEnd = false;
            var pressZDialog = new DialogPopup(Assets.Dialogs.TutorialDialogPrefab, false);
            pressZDialog[0].Set("press z", () =>
            {
                var scrollDownDialog = new DialogPopup(Assets.Dialogs.BigDialogPrefab, reachedEnd);
                for (int i = 0; i < 13; i++)
                {
                    scrollDownDialog[i].Set(".", null);
                }
                scrollDownDialog[13].Set("down here", () =>
                {
                    var pressZ2Dialog = new DialogPopup(Assets.Dialogs.TutorialDialogPrefab, reachedEnd);
                    pressZ2Dialog.WorldPosition += new Vector3(8, -8);
                    pressZ2Dialog[0].Set("last one", () =>
                    {
                        var pressXDialog = new DialogPopup(Assets.Dialogs.TutorialDialogPrefab, true);
                        pressXDialog.WorldPosition += new Vector3(16, -16);
                        GetTipFunc toolTip = null;
                        if (reachedEnd)
                            toolTip = () => new[] { "close all", "dialogs", "", "" };
                        pressXDialog[0].Set("press x", null, toolTip);
                        pressZDialog._canBack = true;
                        pressZ2Dialog._canBack = true;
                        scrollDownDialog._canBack = true;

                        pressXDialog.OnDispose += (u) =>
                        {
                            pressZ2Dialog[0].TextMesh.text = "press x";
                        };
                    });
                    pressZ2Dialog.OnDispose += (u) =>
                    {
                        scrollDownDialog[13].TextMesh.text = "press x";
                    };
                });
                scrollDownDialog.OnDispose += (u) =>
                {
                    pressZDialog[0].TextMesh.text = "press x";
                };
            });

            while (pressZDialog.NotDisposed)
                yield return null;

            var chatty = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(new[] { "tutorial complete" }));
            yield return TinyCoro.Join(chatty);
        }
    }
}

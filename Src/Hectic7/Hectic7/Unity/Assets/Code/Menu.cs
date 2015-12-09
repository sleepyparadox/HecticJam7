using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityTools_4_6;

namespace Hectic7
{
    public static class Menu
    {
        public static void ShowMenu(Marionette mario, Action<BetterPattern> onPatternChoice, /*TODO: Support skip*/ Action onSkip, bool spellsOnly = false, Action onReady = null)
        {
            var mainDialog = new DialogPopup(Assets.Dialogs.MenuDialogPrefab, onPatternChoice == null, name: "main");

            if (mario.WorldPosition.x + (mario.Size.x / 2f) < Main.Left + (Main.MapSize.x / 2f))
            {
                //Sprite on left, show dialog on right
                mainDialog.WorldPosition = mario.WorldPosition + new Vector3(+16, -24, mainDialog.WorldPosition.z);
            }
            else
            {
                //Sprite on right, show dialog on left
                mainDialog.WorldPosition = mario.WorldPosition + new Vector3(-56, -24, mainDialog.WorldPosition.z);
            }


            if (mainDialog.WorldPosition.y < Main.Bottom)
            {
                mainDialog.WorldPosition += new Vector3(0, 24, 0);
            }
            if (mainDialog.WorldPosition.y + 24 > Main.Top)
            {
                mainDialog.WorldPosition += new Vector3(0, Main.Top - 24, 0);
            }

            mainDialog[0].Set("Spells" + (onPatternChoice == null ? "(i)": ""), () =>
            {
                var slotTexts = mario.Patterns.Select(p => p.Name).ToList();
                var toolTips = mario.Patterns.Select<BetterPattern, GetTipFunc>(p => () => p.GetToolTip()).ToList();

                ShowSpellSelector(slotTexts, toolTips, (slot) =>
                {
                    //If spell selectable, else just browsing
                    if (onPatternChoice != null)
                    {
                        mainDialog.Dispose();
                        var choice = mario.Patterns[slot];
                        if(choice.Phases.All(p => p.IsValid()))
                        {
                            onPatternChoice(choice);
                        }
                        else
                        {
                            ShowMenu(mario, onPatternChoice, onSkip);
                            TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(new[] { "Invalid", "pattern" }));
                        }
                    }
                }, disposeOnSelect: true);
            });

            if (Marionette.EditUnlocked)
            {
                mainDialog[1].Set("Edit", () =>
                {
                    var slotTexts = mario.Patterns.Select(p => p.Name).ToList();
                    var toolTips = mario.Patterns.Select<BetterPattern, GetTipFunc>(p => () => p.GetToolTip()).ToList();
                    if (slotTexts.Count < 14)
                    {
                        slotTexts.Add("New");
                        toolTips.Add(() => new[] { "Create a", "new", "pattern" });
                    }
                    ShowSpellSelector(slotTexts, toolTips, (slot) =>
                    {
                        ShowPhaseSelector(mario, slot, (u) => ShowMenu(mario, onPatternChoice, onSkip));
                    }, disposeOnSelect: false);
                });
            }

            mainDialog[2].Set("Config", ShowConfig);

            if(onSkip != null)
            {
                mainDialog[3].Set("Skip", () => 
                {
                    onSkip();
                    mainDialog.Dispose();
                });
            }
            if (onReady != null)
            {
                mainDialog[3].Set("Ready", () =>
                {
                    onReady();
                    mainDialog.Dispose();
                });
            }
        }
        static void ShowConfig()
        {
            var configDialog = new DialogPopup(Assets.Dialogs.MenuDialogPrefab, name: "config");
            configDialog[0].Set("Help", () =>
            {
                var msgText = new[]
                {
                    "Your collider", "is the 4 pixels", "on your back", "",
                    "You cannot", "be hurt by", "your own", "patterns",
                };
                var msg = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(msgText));
            });
            configDialog[1].Set("About", () =>
            {
                var msgText = new[]
                {
                    "Created by Don Logan", "", "", "",
                    "Music by Jared Hahn", "", "", "",
                    "Made for", "GameBoy Jam 2015" , "Rules: Only 4 colors", "Rules: 160px x 144px",
                    "Made for", "Hectic Jam 7", "Aug 15 to 17", "Theme: Puppet Master", "",
                };
                var msg = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(msgText));
            });
        }

        public static void ShowPhaseSelector(Marionette mario, int patternIndex, Action<UnityObject> onClosed)
        {
            var pattern = patternIndex < mario.Patterns.Count ? mario.Patterns[patternIndex] : new BetterPattern();

            var phaseDialog = new DialogPopup(Assets.Dialogs.BigDialogPrefab, true, name: "phase picker");
            phaseDialog.OnDispose += (u) =>
            {
                if(patternIndex < mario.Patterns.Count)
                    mario.Patterns.RemoveAt(patternIndex);
                mario.Patterns.Insert(patternIndex, pattern);
            };

            var i = 0;
            phaseDialog[i].Set(pattern.Name, () =>
            {
                var temp = i;
                ShowNameDialog(pattern, () =>
                {
                    phaseDialog.Dispose();
                    ShowPhaseSelector(mario, patternIndex, onClosed);
                });
            });
            i++;
            for (var iPhase = 0; iPhase < 3; iPhase++, i++ )
            {
                string label;
                string[] tipText;
                GetPhaseLabelAndTip(pattern.Phases, iPhase, out label, out tipText);

                var iTemp = i;
                var iPhaseTemp = i;
                phaseDialog[i].Set(label, () =>
                {
                    phaseDialog.Dispose();
                    ShowPropKeySelector(mario, pattern, iTemp, (u) => ShowPhaseSelector(mario, patternIndex, onClosed));
                }, () => tipText);

                if (iPhase >= pattern.Phases.Count)
                    break;
            }
        }

        static void GetPhaseLabelAndTip(List<Dictionary<string, int>> phases, int index, out string label, out string[] tip)
        {
            if (index < phases.Count)
            {
                label = "Phase " + (index + 1);
                tip = GetTopFromPhase(phases[index]);
            }
            else
            {
                label = "???";
                tip = new[] { "Create a", "phase" };
            }
        }

        public static void ShowNameDialog(BetterPattern pattern, Action onSelection)
        {
            var nameDialog = new DialogPopup(Assets.Dialogs.BigDialogPrefab, name: "name picker");
            var names = Names.All.GetRandomVals(13).ToList();
            names.Insert(0, pattern.Name);

            for (int i = 0; i < names.Count; i++)
            {
                var temp = i;
                nameDialog[temp].Set(names[i], () =>
                {
                    pattern.Name = names[temp];
                    nameDialog.Dispose();
                    onSelection();
                });
            }
        }

        static string[] GetTopFromPhase(Dictionary<string, int> phase)
        {
            return new string[]
            {
                phase.EGetTip<BulletCount>() + " " + phase.EGetTip<BulletType>(),
                phase.EGetTip<BulletShape>(),
                phase.EGetTip<BulletOrigin>(),
                phase.EGetTip<BulletShape>(),
                phase.EGetTip<BulletSpeed>(),
                phase.EGetTip<BulletFill>(),
            };
        }

        public static void ShowPropKeySelector(Marionette mario, BetterPattern pattern, int index, Action<UnityObject> onClosed)
        {
            var phase = index < pattern.Phases.Count ? pattern.Phases[index] : new Dictionary<string, int>();
            pattern.Phases.Add(phase);
            var propertyDialog = new DialogPopup(Assets.Dialogs.BigDialogPrefab, name: "prop key picker");

            var i = 0;


            foreach (var p in BetterPattern.PatternProperties)
            {
                var temp = p;
                var key = temp.FullName;

                var label = phase.ContainsKey(key) ? Enum.GetName(temp, phase[key]) : "???";

                propertyDialog[i].Set(label, () =>
                {
                    ShowPropValSelector(temp, (intVal) =>
                    {
                        if (phase.ContainsKey(key))
                        {
                            phase[key] = intVal;
                        }
                        else
                        {
                            phase.Add(key, intVal);
                        }
                        propertyDialog.Dispose();
                        ShowPropKeySelector(mario, pattern, index, onClosed);
                    }, phase.ContainsKey(key) ? phase[key] : 0);
                });
                i++;
            }
        }

        public static void ShowPropValSelector(Type enumType, Action<int> onValChosen, int defaultIndex)
        {
            var cells = Enum.GetNames(enumType);
            var propValDialog = new DialogPopup(Assets.Dialogs.BigDialogPrefab, false, name: "prop val picker");

            for (int i = 0; i < cells.Length; i++)
            {
                var propVal = cells[i];
                propValDialog[i].Set(propVal, () =>
                {
                    var intVal = (int)(ValueType)Enum.Parse(enumType, propVal);
                    onValChosen(intVal);
                    propValDialog.Dispose();
                });
            }
            propValDialog._index = defaultIndex;
        }

        public static void ShowSpellSelector(List<string> cells, List<GetTipFunc> toolTips, Action<int> onSelection, bool disposeOnSelect)
        {
            var spellDialog = new DialogPopup(Assets.Dialogs.BigDialogPrefab, true, name: "spell picker");

            for (int i = 0; i < cells.Count; i++)
            {
                var temp = i;
                spellDialog[temp].Set(cells[temp], () =>
                {
                    onSelection(temp);
                    if(disposeOnSelect)
                        spellDialog.Dispose();
                }, toolTips[temp]);
            }
        }
    }
}

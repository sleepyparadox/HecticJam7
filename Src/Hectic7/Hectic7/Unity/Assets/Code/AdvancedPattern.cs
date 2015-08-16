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

            if(string.IsNullOrEmpty(customFieldName))
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

            if(showIntro)
            {
                var pickCoro = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(new[] { "Pick a " + enumName }));
                OnDispose += (u) => pickCoro.Kill();
            }
        }
    }

    public class AdvanceSet : List<AdvancedPattern>
    {
        public string Name;
    }

    public class AdvancedPattern
    {
        public const float PhaseDuration = 5f;

        static List<BulletOrigin> Origins = Enum.GetValues(typeof(BulletOrigin)).Cast<BulletOrigin>().ToList();
        static List<BulletDirection> Directions = Enum.GetValues(typeof(BulletDirection)).Cast<BulletDirection>().ToList();
        static List<BulletShape> Shapes = Enum.GetValues(typeof(BulletShape)).Cast<BulletShape>().ToList();
        static List<BulletRotation> Rotations = Enum.GetValues(typeof(BulletRotation)).Cast<BulletRotation>().ToList();
        static List<BulletFill> Fills = Enum.GetValues(typeof(BulletFill)).Cast<BulletFill>().ToList();
        static List<BulletCount> BulletCounts = Enum.GetValues(typeof(BulletCount)).Cast<BulletCount>().ToList();
        static List<BulletType> BulletTypes = Enum.GetValues(typeof(BulletType)).Cast<BulletType>().ToList();
        static List<BulletSpeed> BulletSpeeds = Enum.GetValues(typeof(BulletSpeed)).Cast<BulletSpeed>().ToList();

        public int Slot;
        public BulletOrigin Origin { get; set; }
        public BulletDirection Direction { get; set; }
        public BulletShape Shape { get; set; }
        public BulletRotation Rotation { get; set; }
        public BulletFill Fill { get; set; }
        public BulletCount BulletCount { get; set; }
        public BulletType BulletType { get; set; }
        public BulletSpeed BulletSpeed { get; set; }

        public IEnumerator DoPreform(Marionette attacker, Marionette defender, DirVertical direction)
        {
            yield break;
        }

        public static List<AdvanceSet> GeneratePatternSets(int count)
        {
            var sets = new List<AdvanceSet>();
            for (int iSet = 0; iSet < count; iSet++)
            {
                var set = sets.AddNew(new AdvanceSet());
                for (int iPhase = 0; iPhase < 3; iPhase++)
                {
                    set.Add(new AdvancedPattern()
                    {
                        Origin = Origins.GetRandomVal(),
                        Direction = Directions.GetRandomVal(),
                        Shape = Shapes.GetRandomVal(),
                        Rotation = Rotations.GetRandomVal(),
                        Fill = Fills.GetRandomVal(),
                        BulletCount = BulletCounts.GetRandomVal(),
                        BulletType = BulletTypes.GetRandomVal(),
                        BulletSpeed = BulletSpeeds.GetRandomVal(),
                    });
                }
                set.Name = Names.Adjectives.GetRandomVal() + " " + Names.Nouns.GetRandomVal();
            }
            return sets;
        }
    }
    public static class AdvancedPatternEditor
    {
        public static IEnumerator DoBuildAndShowEditDialog(Marionette mario, int defaultSlotIndex = 0)
        {
            var phases = new List<AdvancedPattern>();

            var slotDialog = new DialogPopup(Assets.Dialogs.BigDialogPrefab, true);
            slotDialog._index = defaultSlotIndex;

            var slots = mario.PatternSets.Select(p => p.Name).ToList();
            if (slots.Count < slotDialog._items.Count)
                slots.Add("New");

            int? selectedSlot = null;
            for (int i = 0; i < slots.Count; i++)
            {
                var temp = i;
                slotDialog[i].Set(slots[i], () =>
                {
                    selectedSlot = temp;
                });
            }

            while (slotDialog.NotDisposed && !selectedSlot.HasValue)
                yield return null;

            slotDialog.Dispose();

            //User quit
            if (!selectedSlot.HasValue)
            {
                //Abandon edit
                yield break;
            }

            while (phases.Count < 3)
            {
                var fields = new List<FieldEntry>()
                {
                    new FieldEntry(typeof(BulletOrigin)),
                    new FieldEntry(typeof(BulletDirection)),
                    new FieldEntry(typeof(BulletShape)),
                    new FieldEntry(typeof(BulletRotation)),
                    new FieldEntry(typeof(BulletFill)),
                    new FieldEntry(typeof(BulletCount)),
                    new FieldEntry(typeof(BulletType)),
                    new FieldEntry(typeof(BulletSpeed)),
                };

                while (fields.Any(field => !field.Choice.HasValue))
                {
                    var field = fields.First(p => !p.Choice.HasValue);

                    var fieldDialog = new DialogEnumPicker(field.EnumType, field.ShowIntro, field.CustomName);
                    fieldDialog.OnEnumValPicked += (i) => field.Choice = i;

                    field.ShowIntro = false;

                    //Wait for choice
                    while (fieldDialog.NotDisposed && !field.Choice.HasValue)
                        yield return null;

                    fieldDialog.Dispose();

                    if (!field.Choice.HasValue)
                    {
                        var lastField = fields.LastOrDefault(f => f.Choice.HasValue);

                        if (lastField == null)
                        {
                            var lastPhase = phases.LastOrDefault();

                            Debug.Log("Return to slot select");
                            
                            var cancelMsg = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(new[] { "Canceled edit" }));
                            //cancelMsg.OnFinished += (c, e) =>
                            //{
                            //    //Pick slot
                            //    TinyCoro.SpawnNext(() => DoBuildAndShowEditDialog(mario, selectedSlot.Value));
                            //};

                            //Abandon thread!
                            yield break;
                        }

                        Debug.Log("Return to " + lastField.EnumType.Name + "  select");

                        //Retry choice
                        lastField.DefaultIndex = lastField.Choice.Value;
                        lastField.Choice = null;
                    }

                    //yield return null;
                }

                if (fields.All(f => f.Choice.HasValue))
                {
                    var phase = new AdvancedPattern()
                    {
                        Origin = fields.EnumFromFields<BulletOrigin>(),
                        Direction = fields.EnumFromFields<BulletDirection>(),
                        Shape = fields.EnumFromFields<BulletShape>(),
                        Rotation = fields.EnumFromFields<BulletRotation>(),
                        Fill = fields.EnumFromFields<BulletFill>(),
                        BulletCount = fields.EnumFromFields<BulletCount>(),
                        BulletType = fields.EnumFromFields<BulletType>(),
                        BulletSpeed = fields.EnumFromFields<BulletSpeed>()
                    };
                    phases.Add(phase);

                    var phasesChoices = Enum.GetValues(typeof(PhaseCount)).Cast<PhaseCount>().Take(3 - phases.Count).ToArray();
                    var phaseDialog = new DialogPicker<PhaseCount>(phasesChoices, "Phases used");

                    PhaseCount? phasesUsed = null;

                    while (phaseDialog.NotDisposed && !selectedSlot.HasValue)
                        yield return null;

                    phaseDialog.Dispose();

                    if (!phasesUsed.HasValue)
                    {
                        var lastField = fields.Last(f => f.Choice.HasValue);

                        Debug.Log("Return to " + lastField.EnumType.Name + "  select");

                        //Retry choice
                        lastField.DefaultIndex = lastField.Choice.Value;
                        lastField.Choice = null;
                        continue;
                    }
                    else
                    {
                        for (var i = 1; i < (int)phasesUsed.Value; ++i)
                        {
                            var clone = new AdvancedPattern()
                            {
                                Origin = phase.Origin,
                                Direction = phase.Direction,
                                Shape = phase.Shape,
                                Rotation = phase.Rotation,
                                Fill = phase.Fill,
                                BulletCount = phase.BulletCount,
                                BulletType = phase.BulletType,
                                BulletSpeed = phase.BulletSpeed,
                            };
                            phases.Add(clone);
                        }
                    }
                }
            }

            var set = new AdvanceSet();
            set.AddRange(phases);
            set.Name = Names.Adjectives.GetRandomVal() + " " + Names.Nouns.GetRandomVal();

            if (selectedSlot.Value > mario.PatternSets.Count)
            {
                mario.PatternSets.RemoveAt(selectedSlot.Value);
            }
            mario.PatternSets.Insert(selectedSlot.Value, set);
        }
    }

    public enum PhaseCount
    {
        One,
        Two,
        Three,
    }

    public enum BulletOrigin
    {
        Bottom,
        BottomLeft,
        BottomRight,
        BottomBoth,
        Left,
        Right,
    }

    public enum BulletDirection
    {
        Up,
        Left,
        Right,
        UpLeft,
        UpRight
    }

    public enum BulletShape
    {
        Line,

        [PatternAttribute(6)]
        Circle,
        [PatternAttribute(6)]
        Box,
        
    }

    public enum BulletRotation
    {
        None,

        [PatternAttribute(6)]
        Clock,
        [PatternAttribute(6)]
        Counter,
    }

    public enum BulletCount
    {
        [PatternAttribute(0)]
        One,
        [PatternAttribute(2)]
        Few,
        [PatternAttribute(4)]
        Many,
        [PatternAttribute(8)]
        Lots,
    }

    public enum BulletFill
    {
        Instant,
        Slow,
        SlowAlt,
    }

    public enum BulletType
    {
        [PatternAttribute(0)]
        Bullet8,
        [PatternAttribute(4)]
        Bullet16,
    }

   

    public enum BulletSpeed
    {
        [PatternAttribute(0)]
        Slow,
        [PatternAttribute(4)]
        Medium,
        [PatternAttribute(8)]
        Fast,
    }

    public enum Phases
    {
        One,
        Two,
        Three,
    }

    public static class Names
    {
        public static List<string> Adjectives = new List<string>()
        {
            "Parallel",
            "Ossified",
            "Awesome",
            "Regular",
            "Medical",
            "Greasy",
            "Full",
            "Present",
            "Habitual",
            "Nostalgic",
            "Difficult",
            "Rigid",
            "Puzzled",
            "Chemical",
            "Labored",
            "Fantastic",
            "Flawless",
            "Double",
            "Spaghetti",
            "Night",
        };

        public static List<string> Nouns = new List<string>()
        {
            "Unlimited",
            "Night",
            "Trouble",
            "Doubt",
            "Opinion",
            "Hour",
            "Destruction",
            "Existence",
            "Discussion",
            "Guide",
            "Limit",
            "Trick",
            "Belief",
            "Sky",
            "Manager",
            "Space",
            "Reason",
            "View",
            "Stranger",
            "Trouble",
            "Doubt",
            "Hour",
            "Stretch",
        };
    }

    [AttributeUsageAttribute(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class PatternAttribute : System.Attribute
    {
        public readonly int Cost;
        public readonly string CustomLabel; 

        public PatternAttribute(int cost, string label = null, string tooltip = null)
        {
            Cost = cost;
            CustomLabel = label;
        }
    }
}

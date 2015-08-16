using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

           TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(new[] { "Pick a " + typeof(T).Name }));
        }

    }

    public class AdvancedPattern
    {
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
        public string Name { get; set; }

        public static List<AdvancedPattern> GeneratePatterns(int count)
        {
            var patterns = new List<AdvancedPattern>();
            for (int i = 0; i < count; i++)
            {
                var pattern = new AdvancedPattern()
                {
                    Origin = Origins.GetRandomVal(),
                    Direction = Directions.GetRandomVal(),
                    Shape = Shapes.GetRandomVal(),
                    Rotation = Rotations.GetRandomVal(),
                    Fill = Fills.GetRandomVal(),
                    BulletCount = BulletCounts.GetRandomVal(),
                    BulletType = BulletTypes.GetRandomVal(),
                    BulletSpeed = BulletSpeeds.GetRandomVal(),
                    Name = Names.Adjectives.GetRandomVal() + " " + Names.Nouns.GetRandomVal(),
                };
            }
            return patterns;
        }
    }
    public static class AdvancedPatternEditor
    {
        public static void BuildAndShowEditDialog()
        {
            var dialogs = new List<UnityObject>();
            var editDialog = new DialogPopup(Assets.Dialogs.BigDialogPrefab, true);
            dialogs.Add(editDialog);
            for (int i = 0; i < 10; i++)
            {
                editDialog[i].Set("Slot " + i, () =>
                {
                    var spawnDialog = new DialogPicker<BulletOrigin>(Enum.GetValues(typeof(BulletOrigin)).Cast<BulletOrigin>().ToArray());
                    dialogs.Add(spawnDialog);
                    spawnDialog.OnItemPicked += (spawnType) =>
                    {
                        var typeDialog = new DialogPicker<BulletType>(Enum.GetValues(typeof(BulletType)).Cast<BulletType>().ToArray());
                        dialogs.Add(typeDialog);
                        typeDialog.OnItemPicked += (bulletType) =>
                        {
                            var countDialog = new DialogPicker<BulletCount>(Enum.GetValues(typeof(BulletCount)).Cast<BulletCount>().ToArray());
                            dialogs.Add(countDialog);
                            countDialog.OnItemPicked += (bulletCount) =>
                            {
                                var speedDialog = new DialogPicker<BulletSpeed>(Enum.GetValues(typeof(BulletSpeed)).Cast<BulletSpeed>().ToArray());
                                dialogs.Add(speedDialog);
                                speedDialog.OnItemPicked += (bulletSpeed) =>
                                {
                                    var name0Dialog = new DialogPicker<string>(Names.Adjectives.GetRandomVals(14), "Name");
                                    dialogs.Add(name0Dialog);
                                    name0Dialog.OnItemPicked += (name0) =>
                                    {
                                        var name1Dialog = new DialogPicker<string>(Names.Nouns.GetRandomVals(14), "Name");
                                        dialogs.Add(name1Dialog);
                                        name1Dialog.OnItemPicked += (name1) =>
                                        {
                                            var msgText = new[] { "Created " + name0 + " " + name1 };
                                            var msg = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(msgText));
                                            msg.OnFinished += (c, r) =>
                                            {
                                                foreach (var d in dialogs)
                                                    d.Dispose();
                                            };
                                        };
                                    };


                                };
                            };
                        };

                    };
                });
            }
        }
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
            "Fluffy",
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
        };

        public static List<string> Nouns = new List<string>()
        {
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

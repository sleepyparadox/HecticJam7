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
        public AdvanceSet()
        {

        }
        public AdvanceSet(string name, params AdvancedPattern[] phases)
        {
            Name = name;
            AddRange(phases);
        }
        public AdvanceSet(params AdvancedPattern[] phases)
        {
            AddRange(phases);
        }
    }

    public class AdvancedPattern
    {
        public const float PhaseDuration = 3f;

        static List<BulletOrigin> Origins = Enum.GetValues(typeof(BulletOrigin)).Cast<BulletOrigin>().ToList();
        static List<BulletMovement> Directions = Enum.GetValues(typeof(BulletMovement)).Cast<BulletMovement>().ToList();
        static List<BulletShape> Shapes = Enum.GetValues(typeof(BulletShape)).Cast<BulletShape>().ToList();
        static List<BulletRotation> Rotations = Enum.GetValues(typeof(BulletRotation)).Cast<BulletRotation>().ToList();
        static List<BulletFill> Fills = Enum.GetValues(typeof(BulletFill)).Cast<BulletFill>().ToList();
        static List<BulletCount> BulletCounts = Enum.GetValues(typeof(BulletCount)).Cast<BulletCount>().ToList();
        static List<BulletType> BulletTypes = Enum.GetValues(typeof(BulletType)).Cast<BulletType>().ToList();
        static List<BulletSpeed> BulletSpeeds = Enum.GetValues(typeof(BulletSpeed)).Cast<BulletSpeed>().ToList();

        public int Slot;
        public BulletOrigin Origin { get; set; }
        public BulletMovement Direction { get; set; }
        public BulletShape Shape { get; set; }
        public BulletRotation Rotation { get; set; }
        public BulletFill Fill { get; set; }
        public BulletCount BulletCount { get; set; }
        public BulletType BulletType { get; set; }
        public BulletSpeed BulletSpeed { get; set; }

        public AdvancedPattern()
        {

        }

        public AdvancedPattern(AdvancedPattern other)
        {
            Origin = other.Origin;
            Direction = other.Direction;
            Shape = other.Shape;
            Rotation = other.Rotation;
            Fill = other.Fill;
            BulletCount = other.BulletCount;
            BulletType = other.BulletType;
            BulletSpeed = other.BulletSpeed;
        }

        public AdvancedPattern Clone(Action<AdvancedPattern> postEdit = null)
        {
            var p = new AdvancedPattern(this);
            if (postEdit != null)
                postEdit(p);
            return p;
        }

        public IEnumerator DoPreform(Marionette attacker, Marionette defender, DirVertical direction)
        {
            var start = GetStartPos(direction);
            var count = GetBulletCount(BulletCount);
            //var shootDir = GetDirection(Direction);
            var speed = GetSpeed(BulletSpeed);
            var fillTime = GetFillTime(Fill);

            var bullets = new List<Bullet>();

            var randTurn = speed * UnityEngine.Random.Range(0f, 0.5f) * (UnityEngine.Random.Range(0, 100) >= 50 ? -1 : 1);

            for (var i = 0; i < count; ++i)
            {
                float n;

                if(count == 1)
                {
                    n = Shape == BulletShape.LineFloor || Shape == BulletShape.LineSide ? 0.5f : 0f;
                }
                else
                {
                    n = (float)i / (count - 1);
                }

                if (Fill == BulletFill.SlowAlt)
                    n = 1f - n;

                var bullet = bullets.AddNew(new Bullet(attacker, BulletType));

                if (Shape == BulletShape.LineFloor)
                {
                    bullet.Center = new Vector3(n * Main.MapSize.x, start.y);
                    bullet.Velocity = Vector3.up * speed * (direction == DirVertical.Down ? -1f : 1f);
                }
                else if (Shape == BulletShape.LineSide)
                {
                    bullet.Center = new Vector3(start.x, n * Main.MapSize.y);
                    if(Origin == BulletOrigin.Left)
                        bullet.Velocity = Vector3.right * speed;
                    else if (Origin == BulletOrigin.Right)
                        bullet.Velocity = Vector3.left * speed;
                    else
                        bullet.Velocity = Vector3.up * speed * (direction == DirVertical.Down ? -1f : 1f);
                }
                else if (Shape == BulletShape.Circle)
                {
                    var angle = n * Mathf.PI * 2f;
                    bullet.Center = start + (new Vector3(Mathf.Sin(angle), Mathf.Cos(angle)) * 1);
                    bullet.Velocity = (bullet.Center - start).normalized * speed;
                }
                else
                {
                    bullet.Center = start;
                    bullet.Velocity = Vector3.up * speed * (direction == DirVertical.Down ? -1f : 1f);
                }


                if(Mathf.Abs(bullet.Velocity.x) > Mathf.Abs(bullet.Velocity.y))
                {
                    bullet.Velocity.y += randTurn;
                }
                else
                {
                    var steerAmount = 0f;
                    if (n > 0.5f)
                        steerAmount = 1f;
                    else if (n < 0.5f)
                        steerAmount = -1f;

                    bullet.Velocity.x += randTurn * steerAmount;
                }

                if (fillTime > 0f && count > 1)
                {
                    var delayPerBullet = fillTime / count;
                    yield return TinyCoro.Wait(delayPerBullet);
                }
            }

            while (bullets.Any(b => b.NotDisposed))
                yield return null;
        }

        private float GetFillTime(BulletFill fill)
        {
            switch (fill)
            {
                case BulletFill.Instant:
                    return 0f;
                case BulletFill.Slow:
                case BulletFill.SlowAlt:
                    return AdvancedPattern.PhaseDuration;
            }
            throw new NotImplementedException();
        }

        float GetSpeed(BulletSpeed speed)
        {
            switch (speed)
            {
                case BulletSpeed.Slow:
                    return 10f;
                case BulletSpeed.Medium:
                    return 20f;
                case BulletSpeed.Fast:
                    return 30f;
                case BulletSpeed.CrazyFast:
                    return 40f;
            }
            throw new NotImplementedException();
        }

        //Vector3 GetDirection(BulletMovement bulletDir, DirVertical vertDir)
        //{
        //    var dir = Vector3.zero;
        //    switch (bulletDir)
        //    {
        //        case BulletMovement.Straight:
        //            dir = Vector3.up;
        //            break;
        //    }

        //    if (vertDir == DirVertical.Down)
        //        dir.y *= -1f;
        //    return dir;
        //}

        int GetBulletCount(BulletCount count)
        {
            switch (count)
            {
                case BulletCount.One:
                    return 1;
                case BulletCount.Few:
                    return 3;
                case BulletCount.Many:
                    return 5;
                case BulletCount.Lots:
                    return 10;
                case BulletCount.Hell:
                    return 15;
            }
            throw new NotImplementedException();
        }

        Vector3 GetStartPos(DirVertical direction)
        {
            switch(Origin)
            {
                case BulletOrigin.Bottom:
                    return new Vector3(Main.MapSize.x / 2f, direction == DirVertical.Down ? Main.Top : Main.Bottom);
                case BulletOrigin.Middle:
                    return Main.MapSize / 2f;
                case BulletOrigin.Left:
                    return new Vector3(0f, Main.MapSize.y / 2f);
                case BulletOrigin.Right:
                    return new Vector3(Main.MapSize.x, Main.MapSize.y / 2f);
            }
            throw new NotImplementedException();
        }

        public static List<AdvanceSet> GeneratePatternSets()
        {
            var cUpSmall = new AdvancedPattern()
            {
                Origin = BulletOrigin.Bottom,
                Direction = BulletMovement.Straight,
                Shape = BulletShape.LineFloor,
                Fill = BulletFill.Instant,
                BulletCount =  BulletCount.Many,
                BulletType =  BulletType.BulletSmall,
                BulletSpeed = BulletSpeed.Fast,
            };

            var cUpBig = new AdvancedPattern()
            {
                Origin = BulletOrigin.Bottom,
                Direction = BulletMovement.Straight,
                Shape = BulletShape.LineFloor,
                Fill = BulletFill.Instant,
                BulletCount = BulletCount.Lots,
                BulletType = BulletType.BulletLarge,
                BulletSpeed = BulletSpeed.Slow,
            };

            var fillRight = new AdvancedPattern()
            {
                Origin = BulletOrigin.Right,
                Direction = BulletMovement.Straight,
                Shape = BulletShape.LineSide,
                Fill = BulletFill.SlowAlt,
                BulletCount = BulletCount.Many,
                BulletType = BulletType.BulletSmall,
                BulletSpeed = BulletSpeed.Medium,
            };

            var circleBig = new AdvancedPattern()
            {
                Origin = BulletOrigin.Middle,
                Direction = BulletMovement.Straight,
                Shape = BulletShape.Circle,
                Fill = BulletFill.SlowAlt,
                BulletCount = BulletCount.Few,
                BulletType = BulletType.BulletLarge,
                BulletSpeed = BulletSpeed.Slow,
            };

            var sets = new List<AdvanceSet>()
            {
                new AdvanceSet
                (
                    "Upbeat",
                    cUpBig.Clone(),
                    cUpSmall.Clone(),
                    cUpSmall.Clone()
                ),
                new AdvanceSet
                (
                    "Clockwork",
                    fillRight.Clone(),
                    fillRight.Clone((p) => { p.Origin = BulletOrigin.Bottom; p.Shape = BulletShape.LineFloor; }),
                    fillRight.Clone((p) => { p.Origin = BulletOrigin.Left; p.Fill = BulletFill.Slow; })
                ),
                new AdvanceSet
                (
                    "WindMill",
                    circleBig.Clone(),
                    circleBig.Clone(),
                    fillRight.Clone((p) => {  p.Origin = BulletOrigin.Bottom; p.Fill = BulletFill.Slow; p.BulletSpeed = BulletSpeed.Fast; })
                ),
                new AdvanceSet
                (
                    "Cross Counter",
                    fillRight.Clone((p) => { p.Origin = BulletOrigin.Right; p.Fill = BulletFill.Slow; p.BulletType = BulletType.BulletLarge; }),
                    fillRight.Clone((p) => { p.Origin = BulletOrigin.Left; p.Fill = BulletFill.SlowAlt; }),
                    fillRight.Clone((p) => { p.Origin = BulletOrigin.Left; p.Fill = BulletFill.SlowAlt; })
                ),
            };
            
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
                    new FieldEntry(typeof(BulletMovement)),
                    new FieldEntry(typeof(BulletShape)),
                    //new FieldEntry(typeof(BulletRotation)),
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
                        Direction = fields.EnumFromFields<BulletMovement>(),
                        Shape = fields.EnumFromFields<BulletShape>(),
                        //Rotation = fields.EnumFromFields<BulletRotation>(),
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
        //BottomLeft,
        //BottomRight,
        //BottomBoth,
        Left,
        Right,
        Middle,
    }

    public enum BulletMovement
    {
        Straight
    }

    public enum BulletShape
    {
        [PatternAttribute(0)]
        LineFloor,
        [PatternAttribute(2)]
        LineSide,

        [PatternAttribute(6)]
        Circle,

        [PatternAttribute(6)]
        Box,
        
    }

    public enum BulletRotation
    {
        None,

        //[PatternAttribute(6)]
        //Clock,
        //[PatternAttribute(6)]
        //Counter,
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
        [PatternAttribute(12)]
        Hell,
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
        BulletSmall,
        [PatternAttribute(4)]
        BulletLarge,
    }


    public enum BulletSpeed
    {
        [PatternAttribute(0)]
        Slow,
        [PatternAttribute(4)]
        Medium,
        [PatternAttribute(8)]
        Fast,
        [PatternAttribute(12)]
        CrazyFast,
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

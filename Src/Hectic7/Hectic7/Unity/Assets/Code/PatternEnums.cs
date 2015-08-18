using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hectic7
{
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
        Floor,
        [PatternAttribute(2)]
        Side,

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
}

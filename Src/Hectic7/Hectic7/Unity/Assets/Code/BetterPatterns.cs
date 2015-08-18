using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityTools_4_6;

namespace Hectic7
{
    public class BetterPatterns
    {
        public static List<BetterPattern> GeneratePatternSets()
        {
            var cUpSmall = new Dictionary<string, int>();
            cUpSmall.ESet(BulletOrigin.Bottom);
            cUpSmall.ESet(BulletShape.Floor);
            cUpSmall.ESet(BulletFill.Instant);
            cUpSmall.ESet(BulletCount.Many);
            cUpSmall.ESet(BulletType.BulletSmall);
            cUpSmall.ESet(BulletSpeed.Fast);
            cUpSmall.ESet(BulletMovement.Straight);

            var cUpBig = new Dictionary<string, int>();
            cUpBig.ESet(BulletOrigin.Bottom);
            cUpBig.ESet(BulletShape.Floor);
            cUpBig.ESet(BulletFill.Instant);
            cUpBig.ESet(BulletCount.Few);
            cUpBig.ESet(BulletType.BulletLarge);
            cUpBig.ESet(BulletSpeed.Slow);
            cUpBig.ESet(BulletMovement.Straight);

            var fillRight = new Dictionary<string, int>();
            fillRight.ESet(BulletOrigin.Right);
            fillRight.ESet(BulletShape.Side);
            fillRight.ESet(BulletFill.SlowAlt);
            fillRight.ESet(BulletCount.Many);
            fillRight.ESet(BulletType.BulletSmall);
            fillRight.ESet(BulletSpeed.Medium);
            fillRight.ESet(BulletMovement.Straight);

            var circleBig = new Dictionary<string, int>();
            circleBig.ESet(BulletOrigin.Middle);
            circleBig.ESet(BulletShape.Circle);
            circleBig.ESet(BulletFill.SlowAlt);
            circleBig.ESet(BulletCount.Many);
            circleBig.ESet(BulletType.BulletLarge);
            circleBig.ESet(BulletSpeed.Slow);
            circleBig.ESet(BulletMovement.Straight);

            var sets = new List<BetterPattern>()
            {
                new BetterPattern
                (
                    "Upbeat",
                    cUpBig.Clone(),
                    cUpSmall.Clone(),
                    cUpSmall.Clone()
                ),
                new BetterPattern
                (
                    "Clockwork",
                    fillRight.Clone(),
                    fillRight.Clone((p) => { p.ESet(BulletOrigin.Bottom); p.ESet(BulletShape.Floor); }),
                    fillRight.Clone((p) => { p.ESet(BulletOrigin.Left); p.ESet(BulletFill.Slow); })
                ),
                new BetterPattern
                (
                    "WindMill",
                    circleBig.Clone(),
                    circleBig.Clone(),
                    fillRight.Clone((p) => {  p.ESet(BulletOrigin.Bottom); p.ESet(BulletFill.Slow); p.ESet(BulletSpeed.Fast); })
                ),
                new BetterPattern
                (
                    "Cross Counter",
                    fillRight.Clone((p) => { p.ESet(BulletOrigin.Right); p.ESet(BulletFill.Slow); p.ESet(BulletType.BulletLarge); }),
                    fillRight.Clone((p) => { p.ESet(BulletOrigin.Left); p.ESet(BulletFill.SlowAlt); }),
                    fillRight.Clone((p) => { p.ESet(BulletOrigin.Left); p.ESet(BulletFill.SlowAlt); })
                ),
            };

            return sets;
        }
    }
}

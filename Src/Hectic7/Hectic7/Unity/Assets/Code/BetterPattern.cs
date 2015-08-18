using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityTools_4_6;

namespace Hectic7
{
    public class BetterPattern
    {
        public const float PhaseDuration = 3f;
        public static List<Type> PatternProperties = new List<Type>()
        {
            typeof(BulletShape), //tip
            typeof(BulletOrigin), //
            typeof(BulletMovement),
            //typeof(BulletRotation),
            typeof(BulletFill),
            typeof(BulletCount),
            typeof(BulletType),
            typeof(BulletSpeed),
        };

        public List<Dictionary<string, int>> Phases { get; set; }
        public string Name { get; set; }

        public string[] GetToolTip()
        {
            var tip = new string[6];
            for(var i = 0; i < 3; ++i)
            {
                if (i >= Phases.Count || !Phases[i].IsValid())
                {
                    tip[(i * 2)] = "???";
                    tip[(i * 2) + 1] = "???";
                }
                else
                {
                    tip[(i * 2)] = Phases[i].EGetTip<BulletCount>() + " " + Phases[i].EGetTip<BulletType>();
                    tip[(i * 2) + 1] = Phases[i].EGetTip<BulletShape>();
                }
            }
            return tip;
        }

        public BetterPattern()
        {
            Phases = new List<Dictionary<string, int>>();
            Name = Names.All.GetRandomVal();
        }

        public BetterPattern(string name, params Dictionary<string, int>[] phases)
        {
            Phases = phases.ToList();
            Name = name;
        }

        public IEnumerator DoPreform(Marionette attacker, Marionette defender, DirVertical attackDirection)
        {
            var phaseCoros = new List<TinyCoro>();
            var bullets = new List<Bullet>();

            var current = TinyCoro.Current;
            current.OnFinished += (c, r) =>
            {
                foreach (var coro in phaseCoros)
                    coro.Kill();
            };

            var i = 0;
            foreach (var phase in Phases)
            {
                Debug.LogWarning("Shart phase " + i);
                i++;
                var phaseCoro = TinyCoro.SpawnNext(() => DoPhase(attacker, defender, attackDirection, bullets, phase));
                yield return TinyCoro.Wait(PhaseDuration);
            }

            //Wait for turn to end
            //TODO: Wait for bullets and phases to finish, 
            //so phase ends when all bullets are gone
            while (phaseCoros.Any(p => p.Alive) || bullets.Any(b => b.NotDisposed))
            {
                yield return null;
            }
        }

        static IEnumerator DoPhase(Marionette attacker, Marionette defender, DirVertical attackDirection, List<Bullet> bullets, Dictionary<string, int> phase)
        {
            var shape = phase.EGet<BulletShape>();
            var origin = phase.EGet<BulletOrigin>();
            var start = GetStartPos(origin, attackDirection);
            var count = GetBulletCount(phase.EGet<BulletCount>());
            var speed = GetSpeed(phase.EGet<BulletSpeed>());
            var fill = phase.EGet<BulletFill>();
            var fillTime = GetFillTime(fill);

            //Todo use direction
            var randTurn = speed * UnityEngine.Random.Range(0f, 0.5f) * (UnityEngine.Random.Range(0, 100) >= 50 ? -1 : 1);

            for (var i = 0; i < count; ++i)
            {
                float n;

                if (count == 1)
                {
                    n = shape == BulletShape.Floor || shape == BulletShape.Side ? 0.5f : 0f;
                }
                else
                {
                    n = (float)i / (count - 1);
                }

                if (fill == BulletFill.SlowAlt)
                    n = 1f - n;

                var bullet = bullets.AddNew(new Bullet(attacker, phase.EGet<BulletType>()));

                if (shape == BulletShape.Floor)
                {
                    bullet.Center = new Vector3(n * Main.MapSize.x, start.y);
                    bullet.Velocity = Vector3.up * speed * (attackDirection == DirVertical.Down ? -1f : 1f);
                }
                else if (shape == BulletShape.Side)
                {
                    bullet.Center = new Vector3(start.x, n * Main.MapSize.y);
                    if (origin == BulletOrigin.Left)
                        bullet.Velocity = Vector3.right * speed;
                    else if (origin == BulletOrigin.Right)
                        bullet.Velocity = Vector3.left * speed;
                    else
                        bullet.Velocity = Vector3.up * speed * (attackDirection == DirVertical.Down ? -1f : 1f);
                }
                else if (shape == BulletShape.Circle)
                {
                    var angle = n * Mathf.PI * 2f;
                    bullet.Center = start + (new Vector3(Mathf.Sin(angle), Mathf.Cos(angle)) * 1);
                    bullet.Velocity = (bullet.Center - start).normalized * speed;
                }
                else
                {
                    bullet.Center = start;
                    bullet.Velocity = Vector3.up * speed * (attackDirection == DirVertical.Down ? -1f : 1f);
                }


                if (Mathf.Abs(bullet.Velocity.x) > Mathf.Abs(bullet.Velocity.y))
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
        }
        static private float GetFillTime(BulletFill fill)
        {
            switch (fill)
            {
                case BulletFill.Instant:
                    return 0f;
                case BulletFill.Slow:
                case BulletFill.SlowAlt:
                    return PhaseDuration;
            }
            throw new NotImplementedException();
        }

        static float GetSpeed(BulletSpeed speed)
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

        static int GetBulletCount(BulletCount count)
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

        static Vector3 GetStartPos(BulletOrigin origin, DirVertical attackDirection)
        {
            switch (origin)
            {
                case BulletOrigin.Bottom:
                    return new Vector3(Main.MapSize.x / 2f, attackDirection == DirVertical.Down ? Main.Top : Main.Bottom);
                case BulletOrigin.Middle:
                    return Main.MapSize / 2f;
                case BulletOrigin.Left:
                    return new Vector3(0f, Main.MapSize.y / 2f);
                case BulletOrigin.Right:
                    return new Vector3(Main.MapSize.x, Main.MapSize.y / 2f);
            }
            throw new NotImplementedException();
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityTools_4_6;

namespace Hectic7
{
    public enum PhaseDelay
    {
        Input,
        Auto,
    }

    public class BulletPattern
    {
        public static IEnumerator TripleSpray(Marionette parent, Marionette target, Direction dir)
        {
            var phaseDuration = 3f;
            var bulletSpeed = 15f;
            var phaseTotal = 3;

            var phases = new List<TinyCoro>();

            for (int phaseIndex = 0; phaseIndex < phaseTotal; phaseIndex++)
            {
                yield return TinyCoro.Wait(phaseDuration / 2f);
                Debug.Log(string.Format("{0}, shot", parent.Control));

                var spray = TinyCoro.SpawnNext(() => DoSpray(parent, target, bulletSpeed));
                TinyCoro.Current.OnFinished += (c, r) => spray.Kill();
                phases.Add(spray);

                yield return TinyCoro.Wait(phaseDuration / 2f);
            }

            yield return TinyCoro.WaitUntil(() => phases.All(p => !p.Alive));
            yield return TinyCoro.WaitUntil(() => !Main.S.ActiveBullets.Any());
        }
        static IEnumerator DoSpray(Marionette parent, Marionette target, float bulletSpeed)
        {
            for (var angleOffset = 0f; angleOffset < Mathf.PI * 0.35f; angleOffset += Mathf.PI * 0.05f)
            {
                //var angleLook = Mathf.Atan2(parent.WorldPosition.y - target.WorldPosition.y, parent.WorldPosition.x - target.WorldPosition.x);
                var angleLook = -1f * Mathf.Atan2(parent.WorldPosition.x - target.WorldPosition.x, target.WorldPosition.y - parent.WorldPosition.y);
                for (var side = -1f; side <= 1; side += 2)
                {
                    var angle = angleLook + (angleOffset);

                    var bullet = new Bullet(parent, 5f);
                    bullet.WorldPosition = parent.WorldPosition;
                    bullet.Velocity = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0f) * bulletSpeed;
                }

                yield return TinyCoro.Wait(0.01f);
            }
        }
    }
}

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

    public class BulletPatterns
    {
        public const float Duration = 5f;

        public static IBulletPattern LToR { get { return new LToR(); } }
    }

    public class LToR : IBulletPattern
    {
        public IEnumerator DoPreform(Marionette attacker, Marionette defender, Direction direction)
        {
            new Bullet(attacker).WorldPosition = attacker.WorldPosition;

            var elapsed = 0f;
            while (elapsed < 100f)
            {
                yield return null;
                elapsed += Time.deltaTime;
            }

        }
    }

    public interface IBulletPattern
    {
        IEnumerator DoPreform(Marionette attacker, Marionette defender, Direction direction);
    }
    //public static IEnumerator TripleSpray(Marionette attacker, Marionette defender, Direction dir)
    //{
    //    var phaseDuration = 3f;
    //    var bulletSpeed = 15f;
    //    var phaseTotal = 3;

    //    var phases = new List<TinyCoro>();

    //    for (int phaseIndex = 0; phaseIndex < phaseTotal; phaseIndex++)
    //    {
    //        yield return TinyCoro.Wait(phaseDuration / 2f);
    //        var spray = TinyCoro.SpawnNext(() => DoSpray(attacker, defender, bulletSpeed), "phase " + phaseIndex);
    //        TinyCoro.Current.OnFinished += (c, r) =>
    //        {
    //            Debug.Log("Bullet pattern died");
    //            spray.Kill();
    //        };
    //        phases.Add(spray);

    //        yield return TinyCoro.Wait(phaseDuration / 2f);
    //    }

    //    yield return TinyCoro.WaitUntil(() => phases.All(p => !p.Alive));
    //    yield return TinyCoro.WaitUntil(() => !Main.S.ActiveBullets.Any());
    //}
    //static IEnumerator DoSpray(Marionette attacker, Marionette defender, float bulletSpeed)
    //{
    //    for (var angleOffset = 0f; angleOffset < Mathf.PI * 0.35f; angleOffset += Mathf.PI * 0.05f)
    //    {
    //        //var angleLook = Mathf.Atan2(parent.WorldPosition.y - target.WorldPosition.y, parent.WorldPosition.x - target.WorldPosition.x);
    //        var angleLook = -1f * Mathf.Atan2(attacker.WorldPosition.x - defender.WorldPosition.x, defender.WorldPosition.y - attacker.WorldPosition.y);
    //        for (var side = -1f; side <= 1; side += 2)
    //        {
    //            var angle = angleLook + (angleOffset * side);

    //            var bullet = new Bullet(attacker, 5f);
    //            bullet.WorldPosition = attacker.WorldPosition;
    //            bullet.Velocity = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0f) * bulletSpeed;
    //        }

    //        yield return TinyCoro.Wait(0.01f);
    //    }
    //}
}

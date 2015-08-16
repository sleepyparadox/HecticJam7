using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityTools_4_6;

namespace Hectic7
{
    //public enum PhaseDelay
    //{
    //    Input,
    //    Auto,
    //}

    //public class HorizontalSwipe : IBulletPattern
    //{
    //    private DirHorizontal _dir;
    //    public HorizontalSwipe(DirHorizontal dir)
    //    {
    //        _dir = dir;
    //    }
    //    public IEnumerator DoPreform(Marionette attacker, Marionette defender, DirVertical direction)
    //    {
    //        var numberOfBullets = 12;
    //        var timePerBullet = BulletPatterns.Duration / numberOfBullets;

    //        for (int i = 0; i < numberOfBullets; i++)
    //        {
    //            var bullet = new Bullet(attacker, BulletType.Bullet8);

    //            var x = (float)i / (numberOfBullets - 1) * Main.MapSize.x;
    //            if (_dir == DirHorizontal.Left)
    //                x = Main.MapSize.x - x;
    //            bullet.Center = new Vector3(x, direction == DirVertical.Down ? Main.Top : Main.Bottom);

    //            bullet.Velocity = (direction == DirVertical.Down ? Vector3.down : Vector3.up) * 20f;   

    //            yield return TinyCoro.Wait(timePerBullet);
    //        }

    //        while (Main.S.ActiveBullets.Any())
    //            yield return null;
    //    }
    //}

    //public class SpinCenter : IBulletPattern
    //{
    //    public IEnumerator DoPreform(Marionette attacker, Marionette defender, DirVertical direction)
    //    {
    //        var numberOfBullets = 20;
    //        var timePerBullet = BulletPatterns.Duration / numberOfBullets;

    //        for (int i = 0; i < numberOfBullets; i++)
    //        {
    //            var bullet = new Bullet(attacker, BulletType.Bullet8);

    //            var angle = Mathf.PI * 2f * i / (numberOfBullets - 1);

    //            bullet.Center = Main.MapSize * 0.5f;;

    //            bullet.Velocity = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0) * 25f;

    //            yield return TinyCoro.Wait(timePerBullet);
    //        }

    //        while (Main.S.ActiveBullets.Any())
    //            yield return null;
    //    }
    //}

    //public class BigAimedShot : IBulletPattern
    //{
    //    private DirHorizontal _corner;
    //    public BigAimedShot(DirHorizontal corner)
    //    {
    //        _corner = corner;
    //    }
    //    public IEnumerator DoPreform(Marionette attacker, Marionette defender, DirVertical direction)
    //    {
    //        Debug.Log("AimedShot  DoPreform");
    //        var distFromCorner = 4f;
    //        var origin = new Vector3(_corner == DirHorizontal.Left ? distFromCorner : Main.MapSize.x - distFromCorner - 1, direction == DirVertical.Up ? distFromCorner : Main.MapSize.y - distFromCorner - 1, 0);

    //        var numberOfBullets = 5;
    //        var timePerBullet = BulletPatterns.Duration / numberOfBullets;

    //        for (int i = 0; i < numberOfBullets; i++)
    //        {
    //            var bullet = new Bullet(attacker, BulletType.Bullet16);
    //            bullet.Center = origin;

    //            var dif = (defender.WorldPosition - origin).Flatten();
    //            bullet.Velocity = dif.normalized * 25;
    //            Debug.Log("Difference " + dif + " vel " + bullet.Velocity);

    //            yield return TinyCoro.Wait(timePerBullet);
    //        }

    //        while (Main.S.ActiveBullets.Any())
    //            yield return null;
    //    }
    //}


    //public interface IBulletPattern
    //{
    //    IEnumerator DoPreform(Marionette attacker, Marionette defender, DirVertical direction);
    //}
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

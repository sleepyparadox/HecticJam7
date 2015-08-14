using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityTools_4_6;

namespace Hectic7
{
    public enum BulletDirection
    {
        Down = -1,
        Up = 1,
    }
    public class BulletPattern
    {
        public static IEnumerator TripleSpray(UnityObject parent, BulletDirection dir)
        {
            var bulletSpeed = 15f;

            for (int i = 0; i < 3; i++)
            {
                for (var angle = 0f; angle < Mathf.PI * 0.35f; angle += Mathf.PI * 0.05f)
                {
                    for (var side = -1f; side <= 1; side += 2)
                    {
                        var bullet = new Bullet(parent, 5f);
                        bullet.WorldPosition = parent.WorldPosition;
                        bullet.Velocity = new Vector3(Mathf.Sin(angle * side), Mathf.Cos(angle * side) * (int)dir, 0f) * bulletSpeed;
                    }

                    yield return TinyCoro.Wait(0.01f);
                }

                yield return TinyCoro.Wait(1f);
            }

            yield return TinyCoro.WaitUntil(() => !Main.S.ActiveBullets.Any());
        }
    }
}

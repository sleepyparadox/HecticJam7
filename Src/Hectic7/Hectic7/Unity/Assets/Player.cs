using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityTools_4_6;

namespace Hectic7
{
    public class Player : UnityObject
    {
        public bool Dead = false;
        public Player()
            : base(GameObject.Find("Player"))
        {
        }

        
        public IEnumerator DoTurn(Foe foe)
        {
            //TODO choose pattern

            var pattern = TinyCoro.SpawnNext(() => BulletPattern.TripleSpray(this, BulletDirection.Up));
            var defendMove = TinyCoro.SpawnNext(() => foe.DoMove(Role.Defending));
            var attackMove = TinyCoro.SpawnNext(() => this.DoMove(Role.Attacking));

            yield return TinyCoro.Join(pattern);
            defendMove.Kill();
            attackMove.Kill();
        }

        public IEnumerator DoMove(Role role)
        {
            while (true)
            {
                var speed = role == Role.Attacking ? 10f : 50f;

                var input = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f);

                var speedMod = Input.GetKey(KeyCode.LeftShift) ? 0.5f : 1f;
                WorldPosition += input * speed * speedMod * Time.deltaTime;
                WorldPosition = Main.ClampToMap(WorldPosition);

                //if (role == Role.Defending)
                //{
                //    for (int i = 0; i < Main.S.ActiveBullets.Count; i++)
                //    {
                //        var bullet = Main.S.ActiveBullets[i];
                //        var coolideAt = (bullet.Transform.localScale.y + Transform.localScale.y) / 2f;

                //        if ((WorldPosition - bullet.WorldPosition).sqrMagnitude < coolideAt * coolideAt)
                //        {
                //            Dead = true;

                //            //Stop all movement
                //            for (int j = 0; j < Main.S.ActiveBullets.Count; j++)
                //            {
                //                Main.S.ActiveBullets[j].UnityFixedUpdate = null;
                //            }
                //            yield return TinyCoro.Wait(5f);
                //            for (int j = 0; j < Main.S.ActiveBullets.Count; j++)
                //            {
                //                Main.S.ActiveBullets[j].Dispose();
                //            }
                //            yield break;
                //        }
                //    }
                //}

                yield return null;
            }
        }
    }
}

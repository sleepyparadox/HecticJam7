using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityTools_4_6;

namespace Hectic7
{
    public class Foe : UnityObject
    {
        public bool Dead = false;
        float SteerAngle = Mathf.PI;

        public Foe()
            : base(GameObject.Find("Foe"))
        {
        }

        public IEnumerator DoTurn(Player player)
        {
            var pattern = TinyCoro.SpawnNext(() => BulletPattern.TripleSpray(this, BulletDirection.Down));
            var defendMove = TinyCoro.SpawnNext(() => player.DoMove(Role.Defending));
            var attackMove = TinyCoro.SpawnNext(() => this.DoMove(Role.Attacking));

            yield return TinyCoro.Join(pattern);
            defendMove.Kill();
            attackMove.Kill();
        }

        public IEnumerator DoMove(Role role)
        {
            while (true)
            {
                var speed = role == Role.Attacking ? 1f : 10f;

                SteerAngle += UnityEngine.Random.Range(-10f, 10f) * Time.deltaTime;
                var input = new Vector3(Mathf.Sin(SteerAngle), Mathf.Cos(SteerAngle), 0f);

                WorldPosition += input * speed * Time.deltaTime;
                WorldPosition = Main.ClampToMap(WorldPosition);

                yield return null;
            }
        }
    }
}

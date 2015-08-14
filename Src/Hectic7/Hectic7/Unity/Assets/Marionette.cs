using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityTools_4_6;
using UnityEngine;
using System.Collections;

namespace Hectic7
{
    public enum ControlScheme
    {
        Ai,
        Player,
    }
    public class Marionette : UnityObject
    {
        public Direction Section { get; private set; }
        public bool Alive { get; private set; }
        public float SpeedDefence { get; private set; }
        public float SpeedAttack { get; private set; }
        public ControlScheme Control { get; private set; }

        private float _aiSteer = Mathf.PI;

        public Marionette(ControlScheme control, Direction section, float speedDefence, float speedAttack)
            : base(GameObject.CreatePrimitive(PrimitiveType.Sphere))
        {
            Alive = true;
            SpeedDefence = speedDefence;
            SpeedAttack = speedAttack;
            Control = control;
            Section = section;
            Transform.localScale = Vector3.one * 5f;

            GameObject.name = GetType().Name + "(" + Control + ")";

            WorldPosition = Main.GetStartingPos(section);
        }

        public void ResetPosition()
        {
            WorldPosition = Main.GetStartingPos(Section);
        }

        public IEnumerator DoTurn(Marionette defender)
        {
            //TODO choose pattern
            Debug.Log(this + " start turn vs " + defender);

            var pattern = TinyCoro.SpawnNext(() => BulletPattern.TripleSpray(this, defender, Section.getOther()), "pattern");
            var attackMove = TinyCoro.SpawnNext(() => this.DoMove(Role.Attacking), "pattern");
            var defendMove = TinyCoro.SpawnNext(() => defender.DoMove(Role.Defending), "pattern");

            while(pattern.Alive && defender.Alive)
            {
                yield return null;
            }

            pattern.Kill();
            defendMove.Kill();
            attackMove.Kill();
        }

        public IEnumerator DoMove(Role role)
        {
            while (true)
            {
                var speed = role == Role.Attacking ? SpeedAttack : SpeedDefence;

                Vector3 input;
                if(Control == ControlScheme.Ai)
                {
                    _aiSteer += UnityEngine.Random.Range(-10f, 10f) * Time.deltaTime;
                    input = new Vector3(Mathf.Sin(_aiSteer), Mathf.Cos(_aiSteer), 0f);
                }
                else
                {
                    input = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f);
                    //Debug.Log("player move " + input + " speed " + speed);
                }

                var speedMod = Input.GetKey(KeyCode.LeftShift) ? 0.5f : 1f;
                WorldPosition += input * speed * speedMod * Time.deltaTime;
                WorldPosition = Main.ClampToMap(WorldPosition, Section);

                if (role == Role.Defending)
                {
                    for (int i = 0; i < Main.S.ActiveBullets.Count; i++)
                    {
                        var bullet = Main.S.ActiveBullets[i];
                        var coolideAt = (bullet.Transform.localScale.y + Transform.localScale.y) / 2f;

                        if ((WorldPosition - bullet.WorldPosition).sqrMagnitude < coolideAt * coolideAt)
                        {
                            Alive = false;
                            yield break;
                        }
                    }
                }

                yield return null;
            }
        }
        public override string ToString()
        {
            return string.Format("[{0} {1}, sa: {2}, sd {3}, {4} ]", Control, Section, SpeedAttack, SpeedDefence, base.ToString());
        }
    }
}

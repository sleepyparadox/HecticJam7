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

        public float MyHp { get; set; }


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

            TinyCoro selectedPattern = null;
            var selectedSkip = false;

            if(Control == ControlScheme.Ai)
            {
                selectedPattern = AiChoosePattern(defender);
            }
            else
            {
                BuildAndShowTurnMenu(defender, (choice) => selectedPattern = choice, () => selectedSkip = true);
            }

            //Wait for choice
            yield return TinyCoro.WaitUntil(() => selectedPattern != null || selectedSkip);

            Main.S.MenuRenderer.Clear();

            yield return TinyCoro.Wait(0.1f);

            Main.S.MenuRenderer.Clear();


            if (selectedSkip)
                yield break;

            var timeOut = TinyCoro.SpawnNext(() => DoTimer(15f));
            var attackMove = TinyCoro.SpawnNext(() => this.DoMove(Role.Attacking), "pattern");
            var defendMove = TinyCoro.SpawnNext(() => defender.DoMove(Role.Defending), "pattern");

            while(selectedPattern.Alive && defender.Alive && timeOut.Alive)
            {
                yield return null;
            }

            timeOut.Kill();
            selectedPattern.Kill();
            defendMove.Kill();
            attackMove.Kill();
        }

        TinyCoro AiChoosePattern(Marionette defender)
        {
            return TinyCoro.SpawnNext(() => BulletPattern.TripleSpray(this, defender, Section.getOther()), "pattern");
        }

        void BuildAndShowTurnMenu(Marionette defender, Action<TinyCoro> onPatternChoice, Action onSkip)
        {
            var mainMenu = new Menu("Main");
            var statsMenu = new Menu("Stats");
            var patternMenu = new Menu("Attack");

            mainMenu[0].Set("Attack", () => Main.S.MenuRenderer.Render(patternMenu));
            mainMenu[1].Set("Stats", () => Main.S.MenuRenderer.Render(statsMenu));
            //
            mainMenu[3].Set("Skip", () => onSkip());

            statsMenu[0].Set("AtkSd " + SpeedAttack, null);
            statsMenu[1].Set("DefSd " + SpeedDefence, null);
            //
            statsMenu[3].Set("Back", () => Main.S.MenuRenderer.Render(mainMenu));

            for (int i = 0; i < 3; i++)
            {
                patternMenu[i].Set("Pattern " + i, () => onPatternChoice(TinyCoro.SpawnNext(() => BulletPattern.TripleSpray(this, defender, Section.getOther()), "pattern")));
            }
            patternMenu[3].Set("Back", () => Main.S.MenuRenderer.Render(mainMenu));

            Main.S.MenuRenderer.Render(mainMenu);
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

        IEnumerator DoTimer(float duration)
        {
            var timeRemaining = duration;
            while (timeRemaining > 0f)
            {
                Main.S.Timer = timeRemaining;
                yield return null;
                timeRemaining -= Time.deltaTime;
            }
            Main.S.Timer = 0f;
        }

        public override string ToString()
        {
            return string.Format("[{0} {1}, sa: {2}, sd {3}, {4} ]", Control, Section, SpeedAttack, SpeedDefence, base.ToString());
        }
    }
}

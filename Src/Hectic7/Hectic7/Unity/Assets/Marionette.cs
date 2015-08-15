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

        public static Vector3 SpriteSize { get { return new Vector3(16f, 16f, 0f); } }
        public static Vector3 HitPosLocal { get { return new Vector3(7.5f, 7.5f, 0f); } }
        public static float HitRadius { get { return 2f; } }

        public float MyHp { get; set; }
        private float _aiSteer = Mathf.PI;

        public Vector3 RealPosition;
        public Vector3 Size = new Vector3(16, 16, 0);
        public Vector3 Center
        {
            get
            {
                return RealPosition + (Size * 0.5f);
            }
            set
            {
                RealPosition = value - (Size * 0.5f);
            }
        }

        public Marionette(ControlScheme control, Direction section, PrefabAsset sprite,  float speedDefence, float speedAttack)
            : base(sprite)
        {
            Alive = true;
            SpeedDefence = speedDefence;
            SpeedAttack = speedAttack;
            Control = control;
            Section = section;

            GameObject.name = GetType().Name + "(" + Control + ")";

            Center = Main.GetStartingPos(section);
            WorldPosition = RealPosition.Snap();
        }

        public void ResetPosition()
        {
            Center = Main.GetStartingPos(Section);
            WorldPosition = RealPosition.Snap();
        }

        public IEnumerator DoTurn(Marionette defender)
        {
            //TODO choose pattern
            Debug.Log(this + " start turn vs " + defender);

            IBulletPattern selectedPattern = null;
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
            
            var pattern = TinyCoro.SpawnNext(() => selectedPattern.DoPreform(this, defender, Section.GetOther()));
            var timeOut = TinyCoro.SpawnNext(() => DoTimer(15f));
            var attackMove = TinyCoro.SpawnNext(() => this.DoMove(Role.Attacking), "pattern");
            var defendMove = TinyCoro.SpawnNext(() => defender.DoMove(Role.Defending), "pattern");

            while(pattern.Alive && defender.Alive && timeOut.Alive)
            {
                yield return null;
            }

            if(!pattern.Alive)
            {
                Debug.Log("Turn end because !pattern.Alive");
            }
            if (!defender.Alive)
            {
                Debug.Log("Turn end because !defender.Alive");
            }
            if (!timeOut.Alive)
            {
                Debug.Log("Turn end because !timeOut.Alive");
            }

            timeOut.Kill();
            pattern.Kill();
            defendMove.Kill();
            attackMove.Kill();
        }

        IBulletPattern AiChoosePattern(Marionette defender)
        {
            return BulletPatterns.LToR;
        }

        void BuildAndShowTurnMenu(Marionette defender, Action<IBulletPattern> onPatternChoice, Action onSkip)
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
                patternMenu[i].Set("Pattern " + i, () => onPatternChoice(BulletPatterns.LToR));
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
                RealPosition += input * speed * speedMod * Time.deltaTime;

                if(Control == ControlScheme.Player)
                {
                    Debug.Log(">> player move " + (input * speed * speedMod) + " to " + RealPosition);
                }

                RealPosition = Main.ClampToMap(RealPosition, Section, Size);
                WorldPosition = RealPosition.Snap();

                if (role == Role.Defending)
                {
                    var myPos = WorldPosition + HitPosLocal;
                    for (int i = 0; i < Main.S.ActiveBullets.Count; i++)
                    {
                        var bullet = Main.S.ActiveBullets[i];
                        var coolideAt = (bullet.Transform.localScale.y + HitRadius) / 2f;

                        if ((myPos - bullet.WorldPosition).sqrMagnitude < coolideAt * coolideAt)
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
                Main.S.Timer.Time = timeRemaining;
                yield return null;
                timeRemaining -= Time.deltaTime;
            }
            Main.S.Timer.Time = 0f;
        }

        public override string ToString()
        {
            return string.Format("[{0} {1}, sa: {2}, sd {3}, {4} ]", Control, Section, SpeedAttack, SpeedDefence, base.ToString());
        }
    }
}

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
                RealPosition.z = 1f;
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

            Debug.Log("Set " + this + " to " + WorldPosition);
        }

        public IEnumerator DoTurn(Marionette defender)
        {
            //TODO choose pattern
            Debug.Log(this + " start turn vs " + defender);

            IBulletPattern[] selectedPattern = null;
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

            if (selectedSkip)
                yield break;

            var pattern = TinyCoro.SpawnNext(() => DoPatternSequence(defender, selectedPattern));
            var timeOut = TinyCoro.SpawnNext(() => DoTimer(BulletPatterns.Duration * selectedPattern.Length));
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

        IEnumerator DoPatternSequence(Marionette defender, IBulletPattern[] selectedPattern)
        {
            var patterns = new List<TinyCoro>();
            TinyCoro.Current.OnFinished += (c, r) =>
            {
                foreach (var p in patterns)
                    p.Kill();
            };
            
            for (var i = 0; i < selectedPattern.Length; ++i)
            {
                var pattern = selectedPattern[i];
                patterns.Add(TinyCoro.SpawnNext(() => pattern.DoPreform(this, defender, Section.GetOther())));

                yield return TinyCoro.Wait(BulletPatterns.Duration);
            }

            yield return TinyCoro.WaitUntil(() => patterns.All(p => !p.Alive));
        }

        IBulletPattern[] AiChoosePattern(Marionette defender)
        {
            return new IBulletPattern[]
            {
                BulletPatterns.LToR,
                BulletPatterns.LToR,
                BulletPatterns.LToR,
            };
        }

        void BuildAndShowTurnMenu(Marionette defender, Action<IBulletPattern[]> onPatternChoice, Action onSkip)
        {
            var mainDialog = new DialogPopup(Assets.Dialogs.TinyDialogPrefab, false);

            if(WorldPosition.x + (Size.x / 2f) < Main.Left + (Main.MapSize.x / 2f))
            {
                //Sprite on left, show dialog on right
                mainDialog.WorldPosition = WorldPosition + new Vector3(+24, -8, mainDialog.WorldPosition.z);
            }
            else
            {
                //Sprite on right, show dialog on left
                mainDialog.WorldPosition = WorldPosition + new Vector3(-5 * 8, -8, mainDialog.WorldPosition.z);
            }

            if(mainDialog.WorldPosition.y < Main.Bottom)
            {
                mainDialog.WorldPosition += new Vector3(0, 24, 0);
            }
            if (mainDialog.WorldPosition.y + 24 > Main.Top)
            {
                mainDialog.WorldPosition += new Vector3(0, Main.Top - 24, 0);
            }

            DialogPopup spellDialog;

            mainDialog[0].Set("Spells", () =>
            {
                spellDialog = new DialogPopup(Assets.Dialogs.BigDialogPrefab);
                for (int i = 0; i < spellDialog._items.Count; i++)
                {
                    spellDialog[i].Set("Pattern " + i, () =>
                    {
                        Debug.Log("Pattern selected");
                        mainDialog.Dispose();
                        spellDialog.Dispose();

                        var choice = new IBulletPattern[]
                        {
                            BulletPatterns.LToR,
                            BulletPatterns.LToR,
                            BulletPatterns.LToR,
                        };

                        onPatternChoice(choice);
                    });
                }
            });

            mainDialog[1].Set("Edit", () =>
            {
                spellDialog = new DialogPopup(Assets.Dialogs.BigDialogPrefab);
                spellDialog[0].Set("Nope", null);
            });
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

                //if(Control == ControlScheme.Player)
                //{
                //    Debug.Log(">> player move " + (input * speed * speedMod) + " to " + RealPosition);
                //}

                RealPosition = Main.ClampToMap(RealPosition, Section, Size);
                RealPosition.z = 1f;
                WorldPosition = RealPosition.Snap();

                if (role == Role.Defending)
                {
                    var myPos = WorldPosition + HitPosLocal;
                    for (int i = 0; i < Main.S.ActiveBullets.Count; i++)
                    {
                        var bullet = Main.S.ActiveBullets[i];
                        var bulletCenter = bullet.WorldPosition + (bullet.Size / 2f) + new Vector3(0.5f, 0.5f, 0);
                        var collideRadius = bullet.Size.x / 2f;

                        collideRadius -= 0.1f;

                        var hitStart = WorldPosition + Size / 2f;
                        for(var xHit = 0; xHit < 2; ++xHit)
                        {
                            for (var yHit = 0; yHit < 2; ++yHit)
                            {
                                var hitPos = hitStart + new Vector3(xHit, yHit, 0);

                                if((bulletCenter - hitPos).sqrMagnitude < (collideRadius * collideRadius))
                                {
                                    Alive = false;
                                    yield break;
                                }
                            }
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

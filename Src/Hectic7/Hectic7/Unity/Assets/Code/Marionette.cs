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
        Player = 0,
        Ai = 1,
    }

    public class Marionette : UnityObject
    {
        public static bool EditUnlocked;
        public DirVertical Section { get; private set; }
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
        public List<BetterPattern> Patterns;

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
        public string[] Chat;
        public string Name;
        float _randWeight;
        float _homeWeight;
        float _avoidWeight;

        public Marionette(string name, ControlScheme control, DirVertical section, PrefabAsset sprite,  float speedDefence, float speedAttack, string[] chat, float randWeight, float homeWeight, float avoidWeight)
            : base(sprite)
        {
            _randWeight = randWeight;
            _homeWeight = homeWeight;
            _avoidWeight = avoidWeight;

            Name = name;
            Chat = chat;
            var defaultSets = BetterPatterns.GeneratePatternSets() ;

            if(control == ControlScheme.Player)
            {
                Patterns = defaultSets.Take(1).ToList();
            }
            else
            {
                Patterns = new List<BetterPattern>();
                while (defaultSets.Any() && Patterns.Count < 4)
                {
                    var index = UnityEngine.Random.Range(0, defaultSets.Count);
                    Patterns.Add(defaultSets[index]);
                    defaultSets.RemoveAt(index);
                }
            }

            Alive = true;
            SpeedDefence = speedDefence;
            SpeedAttack = speedAttack;
            Control = control;
            Section = section;

            GameObject.name = GetType().Name + "(" + Control + ")";

            Center = Main.GetStartingPos(this);
            WorldPosition = RealPosition.Snap();
        }

        public void ResetPosition()
        {
            Center = Main.GetStartingPos(this);
            WorldPosition = RealPosition.Snap();
        }

        public IEnumerator DoTurn(Marionette defender, Marionette[] defendingParty)
        {
            Main.S.Timer.InfiniteTime = true;
            //TODO choose pattern

            BetterPattern selectedPattern = null;
            var selectedSkip = false;

            if(Control == ControlScheme.Ai)
            {
                string patternName;
                selectedPattern = AiChoosePattern(defender, out patternName);

                var text = new string[]
                {
                    "Opponent used",
                    patternName
                };

                var patternMessage = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(text));
                yield return TinyCoro.Join(patternMessage);
            }
            else
            {
                Menu.ShowMenu(this, (choice) => selectedPattern = choice, () => selectedSkip = true, spellsOnly: defender == defendingParty.First());
            }

            //Wait for choice
            yield return TinyCoro.WaitUntil(() => selectedPattern != null || selectedSkip);

            if (selectedSkip)
                yield break;

            var pattern = TinyCoro.SpawnNext(() => selectedPattern.DoPreform(this, defender, Section.GetOther()));
            var timeOut = TinyCoro.SpawnNext(() => DoTimer(BetterPattern.PhaseDuration * selectedPattern.Phases.Count));
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
            if (!defender.Alive
                && defender.Control == ControlScheme.Ai)
            {
                var text = new string[]
                {
                    defender.Name,
                    "was defeated",
                };
                var patternMessage = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(text));
                yield return TinyCoro.Join(patternMessage);
            }
            if (!timeOut.Alive)
            {
                Debug.Log("Turn end because !timeOut.Alive");
            }

            timeOut.Kill();
            pattern.Kill();
            defendMove.Kill();
            attackMove.Kill();

            Main.S.Timer.InfiniteTime = true;
        }

        BetterPattern AiChoosePattern(Marionette defender, out string patternName)
        {
            var set = Patterns[UnityEngine.Random.Range(0, Patterns.Count)];
            patternName = set.Name;
            return set;
        }

        public IEnumerator DoMove(Role role, Action onSkip = null, Action onReady = null)
        {
            while (true)
            {
                while (DialogPopup.Stack.Count > 0)
                    yield return null;

                if (Control == ControlScheme.Player
                    && DialogPopup._backKeys.Any(key => Input.GetKeyUp(key)))
                {
                    Menu.ShowMenu(this, null, onSkip, false, onReady);
                    continue;
                }

                var speed = role == Role.Attacking ? SpeedAttack : SpeedDefence;

                Vector3 input;
                if(Control == ControlScheme.Ai)
                {
                    _aiSteer += UnityEngine.Random.Range(-10f, 10f) * Time.deltaTime;
                    var rand = new Vector3(Mathf.Sin(_aiSteer), Mathf.Cos(_aiSteer), 0f) * _randWeight;


                    var home = new Vector3(68, 100);
                    var goHome = (home - this.WorldPosition).normalized * _homeWeight;


                    var flee = Vector3.zero;
                    if (Marionette.EditUnlocked)
                    {
                        var closest = Main.S.ActiveBullets.OrderBy(b => (b.Center - this.Center).sqrMagnitude).FirstOrDefault();
                        if (closest != null)
                        {
                            var dist = (this.Center - closest.Center);
                            if (dist.magnitude < 25)
                                flee = (this.Center - closest.Center).normalized;
                        }
                    }
                    flee *= _avoidWeight;

                    input = (goHome + rand + flee).normalized;
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
                    if(Input.GetKeyUp(KeyCode.Alpha0))
                    {
                        Alive = false;
                        yield break;
                    }

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

                if(DialogPopup.Stack.Count == 0)
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityTools_4_6;

namespace Hectic7
{
    public class Main : MonoBehaviour
    {
        public static Vector3 MapSize = new Vector3(160, 144);
        public static Vector3 ClampToMap(Vector3 src, Direction section)
        {
            if (section == Direction.Up)
            {
                return new Vector3(Mathf.Clamp(src.x, Left, Right), Mathf.Clamp(src.y, Middle, Top), src.z);
            }
            else
            {
                return new Vector3(Mathf.Clamp(src.x, Left, Right), Mathf.Clamp(src.y, Bottom, Middle), src.z);
            }
        }
        public static Vector3 GetStartingPos(Direction section)
        {
            return new Vector3((section == Direction.Up ? Right : Left) * 0.9f, (section == Direction.Up ? Top : Bottom) * 0.9f);
        }

        public static float Left { get { return MapSize.x * -0.5f; } }
        public static float Right { get { return MapSize.x * 0.5f; } }
        public static float Top { get { return MapSize.y * 0.5f; } }
        public static float Bottom { get { return MapSize.y * -0.5f; } }
        public static float Middle { get { return 0; } }

        public static Main S;

        public List<Bullet> ActiveBullets = new List<Bullet>();
        public float Timer
        {
            get { return _time; }
            set
            {
                _time = value;
                _timerText.text = Mathf.CeilToInt(_time).ToString();
            }
        }

        public MenuRenderer MenuRenderer { get; private set; }

        float _time;
        private TextMesh _timerText;

        void Awake()
        {
            S = this;
            TinyCoro.SpawnNext(DoGame);
        }

        void Update()
        {
            TinyCoro.StepAllCoros();
        }

        public IEnumerator DoGame()
        {
            MenuRenderer = new MenuRenderer();

            _timerText = GameObject.Find("TimerText").GetComponent<TextMesh>();
            var parties = new Marionette[][]
            {
                new Marionette[]
                {
                    new Marionette(ControlScheme.Player, Direction.Down, 50f, 40f),
                    new Marionette(ControlScheme.Player, Direction.Down, 50f, 40f),
                    new Marionette(ControlScheme.Player, Direction.Down, 50f, 40f),
                },
                new Marionette[]
                {
                    new Marionette(ControlScheme.Ai, Direction.Up, 50f, 40f),
                    new Marionette(ControlScheme.Ai, Direction.Up, 50f, 40f),
                    new Marionette(ControlScheme.Ai, Direction.Up, 50f, 40f),
                },
            };

            foreach (var p in parties)
            {
                foreach (var m in p)
                {
                    m.ResetPosition();
                    m.SetActive(false);
                }
            }

            //While both parties can fight
            while (parties.All(p => p.Any(m => m.Alive)))
            {
                for (int iParty = 0; iParty < parties.Length; iParty++)
                {
                    foreach (var p in parties)
                    {
                        foreach (var m in p)
                        {
                            m.SetActive(false);
                        }
                    }

                    var party = parties[iParty];
                    var otherParty = parties[iParty == 0 ? 1 : 0];

                    var attacker = party.FirstOrDefault(m => m.Alive);
                    var defender = otherParty.FirstOrDefault(m => m.Alive);

                    if (attacker == null || defender == null)
                        break;

                    attacker.SetActive(true);
                    defender.SetActive(true);

                    var attackerTurn = TinyCoro.SpawnNext(() => attacker.DoTurn(defender));
                    yield return TinyCoro.Join(attackerTurn);

                    var bullets = new List<Bullet>(Main.S.ActiveBullets);
                    foreach (var b in bullets)
                        b.Dispose();
                }
            }

            Debug.Log("Game over");
            //Stop all movement
            for (int j = 0; j < Main.S.ActiveBullets.Count; j++)
            {
                Main.S.ActiveBullets[j].UnityFixedUpdate = null;
            }

            yield return TinyCoro.Wait(1);

            Application.LoadLevel(0);
        }
    }
}

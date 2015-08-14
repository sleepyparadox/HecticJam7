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
        public static Vector3 ClampToMap (Vector3 src, Direction section)
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
            return new Vector3((section == Direction.Up ? Middle : Middle) * 0.9f, (section == Direction.Up ? Top : Bottom) * 0.9f);
        }

        public static float Left { get { return MapSize.x * -0.5f; } }
        public static float Right { get { return MapSize.x * 0.5f; } }
        public static float Top { get { return MapSize.y * 0.5f; } }
        public static float Bottom { get { return MapSize.y * -0.5f; } }
        public static float Middle { get { return 0; } }

        public static Main S;

        public List<Bullet> ActiveBullets = new List<Bullet>();

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
            var parties = new Marionette[][]
            {
                new Marionette[]
                {
                    new Marionette(ControlScheme.Player, Direction.Down, 50f, 40f),
                },
                new Marionette[]
                {
                    new Marionette(ControlScheme.Ai, Direction.Up, 10f, 10f),
                },
            };


            foreach(var party in parties)
            {
                foreach(var m in party)
                {
                    m.ResetPosition();
                }
            }
            //attacker.ResetPosition();
            //defender.ResetPosition();

            //While both parties can fight
            while (parties.All(p => p.Any(m => m.Alive)))
            {
                for (int iParty = 0; iParty < parties.Length; iParty++)
                {
                    var party = parties[iParty];
                    var otherParty = parties[iParty == 0 ? 1: 0];


                    var attacker = party.FirstOrDefault(m => m.Alive);
                    var defender = otherParty.FirstOrDefault(m => m.Alive);

                    if (attacker == null || defender == null)
                        break;



                    var attackerTurn = TinyCoro.SpawnNext(() => attacker.DoTurn(defender));
                    yield return TinyCoro.Join(attackerTurn);
                }
            }

            Debug.Log("Game over");
            //Stop all movement
            for (int j = 0; j < Main.S.ActiveBullets.Count; j++)
            {
                Main.S.ActiveBullets[j].UnityFixedUpdate = null;
            }

            yield return  TinyCoro.Wait(30);

            Application.LoadLevel(0);
        }

    }
}

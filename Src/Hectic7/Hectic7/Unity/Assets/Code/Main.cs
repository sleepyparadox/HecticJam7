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
        public static Vector3 ClampToMap(Vector3 src, DirVertical section, Vector3 size)
        {
            return new Vector3(Mathf.Clamp(src.x, Left , Right - size.x ), Mathf.Clamp(src.y, Bottom , Top - size.x), src.z);
            
        }
        public static Vector3 GetStartingPos(DirVertical section)
        {
            return new Vector3((section == DirVertical.Up ? 0.9f : 0.1f) * MapSize.x, (section == DirVertical.Up ? 0.9f : 0.1f) * MapSize.y);
        }

        public static float Left { get { return 0; } }
        public static float Right { get { return MapSize.x; } }
        public static float Top { get { return MapSize.y; } }
        public static float Bottom { get { return 0; } }

        public static Main S;

        public List<Bullet> ActiveBullets = new List<Bullet>();
        
        public Timer Timer { get; private set; }

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
            Timer = new Timer();

            var playerSpeed = 75f;
            var botSpeed = 30f;

            var parties = new Marionette[][]
            {
                new Marionette[]
                {
                    new Marionette(ControlScheme.Player, DirVertical.Down, Assets.Mars.Mar00Prefab, playerSpeed, playerSpeed),
                    new Marionette(ControlScheme.Player, DirVertical.Down, Assets.Mars.Mar00Prefab, playerSpeed, playerSpeed),
                    new Marionette(ControlScheme.Player, DirVertical.Down, Assets.Mars.Mar00Prefab, playerSpeed, playerSpeed),
                },
                new Marionette[]
                {
                    new Marionette(ControlScheme.Ai, DirVertical.Up, Assets.Mars.Mar01Prefab, botSpeed, botSpeed),
                    new Marionette(ControlScheme.Ai, DirVertical.Up, Assets.Mars.Mar01Prefab, botSpeed, botSpeed),
                    new Marionette(ControlScheme.Ai, DirVertical.Up, Assets.Mars.Mar01Prefab, botSpeed, botSpeed),
                },
            };

            yield return null;

            foreach (var p in parties)
            {
                foreach (var m in p)
                {
                    m.ResetPosition();
                    m.SetActive(false);
                }
            }

            yield return null;

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

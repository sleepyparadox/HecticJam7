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
        public static Vector3 GetStartingPos(Marionette mario)
        {
            return new Vector3((MapSize.x / 2f) - (mario.Size.x / 2f), (mario.Section == DirVertical.Up ? 0.8f : 0.1f) * MapSize.y);
        }

        public static float Left { get { return 0; } }
        public static float Right { get { return MapSize.x; } }
        public static float Top { get { return MapSize.y; } }
        public static float Bottom { get { return 0; } }

        public static Main S;

        public List<Bullet> ActiveBullets = new List<Bullet>();
        
        public Timer Timer { get; private set; }
        public MapScroller AutoScroller { get; private set; }

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
            var willInto = true;
            if (willInto)
            {
                var introText = new string[]
                {
                "Hello there!", "","","",
                "Welcome to the land", "of puppets","","",
                "Puppets are amazing", "puppets that can","puppet anything","",
                "Here you will learn", "","","",
                "grow", "","","",
                "and puppet", "","","",
                "Good luck!", "","","",
                };

                var introMsg = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(introText));
                yield return TinyCoro.Join(introMsg);
            }

            AutoScroller = new MapScroller(new Vector3(0, 0, 50));

            Timer = new Timer();
            Timer.InfiniteTime = true;

            var playerSpeed = 75f;
            var botSpeed = 30f;

            var parties = new Marionette[][]
            {
                new Marionette[]
                {
                    new Marionette(ControlScheme.Player, DirVertical.Down, Assets.Mars.Mar00Prefab, playerSpeed, playerSpeed),
                },
                new Marionette[]
                {
                    new Marionette(ControlScheme.Ai, DirVertical.Up, Assets.Mars.Mar01Prefab, botSpeed, botSpeed),
                    new Marionette(ControlScheme.Ai, DirVertical.Up, Assets.Mars.Mar01Prefab, botSpeed, botSpeed),
                    new Marionette(ControlScheme.Ai, DirVertical.Up, Assets.Mars.Mar01Prefab, botSpeed, botSpeed),
                    new Marionette(ControlScheme.Ai, DirVertical.Up, Assets.Mars.Mar01Prefab, botSpeed, botSpeed),
                    new Marionette(ControlScheme.Ai, DirVertical.Up, Assets.Mars.Mar01Prefab, botSpeed, botSpeed),
                },
            };

            var player = parties[(int)ControlScheme.Player].First();
            player.ResetPosition();
            player.SetActive(transform);

            foreach (var m in parties[(int)ControlScheme.Ai])
            {
                m.ResetPosition();
                m.SetActive(false);
            }

            if (willInto)
            {
                //Chill for a bit
                var freeRoam = TinyCoro.SpawnNext(() => player.DoMove(Role.Attacking));
                yield return TinyCoro.Wait(8f);
                freeRoam.Kill();
            }

            //First puppet msg
            {
                var msg = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(new[] { "A puppet appeared!" }));
                yield return TinyCoro.Join(msg);
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

                    var attackingParty = parties[iParty];
                    var defendingParty = parties[iParty == 0 ? 1 : 0];

                    var attacker = attackingParty.FirstOrDefault(m => m.Alive);
                    var defender = defendingParty.FirstOrDefault(m => m.Alive);

                    if (attacker == null || defender == null)
                        break;

                    attacker.SetActive(true);
                    defender.SetActive(true);

                    var attackerTurn = TinyCoro.SpawnNext(() => attacker.DoTurn(defender));
                    yield return TinyCoro.Join(attackerTurn);

                    var bullets = new List<Bullet>(Main.S.ActiveBullets);
                    foreach (var b in bullets)
                        b.Dispose();

                    defender.SetActive(defender.Alive);

                    if (!defender.Alive
                        && defender.Control == ControlScheme.Ai)
                    {
                        if (defendingParty.Any(m => m.Alive))
                        {
                            //Chill for a bit
                            var freeRoam = TinyCoro.SpawnNext(() => attacker.DoMove(Role.Attacking));
                            yield return TinyCoro.Wait(5f);
                            freeRoam.Kill();

                            var msg = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(new[] { "Another puppet appeared!" }));
                            yield return TinyCoro.Join(msg);
                        }
                        else
                        {
                            var msg = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(new[] { "You win!" }));
                            yield return TinyCoro.Join(msg);
                        }
                    }
                    else if (!defender.Alive
                      && defender.Control == ControlScheme.Ai)
                    {
                        var msg = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(new[] { "Game Over" }));
                        yield return TinyCoro.Join(msg);
                    }
                }
            }

            //Retry msg
            {
                var msg = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(new[] { "Retry?" }));
                yield return TinyCoro.Join(msg);
            }

            Application.LoadLevel(0);
        }
    }
}

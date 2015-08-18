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
        public AudioBehaviour Music { get; private set; }

        void Awake()
        {
            DialogPopup.Stack.Clear();
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
            //willInto = false;

            if (willInto)
            {
                var tutorialCoro = TinyCoro.SpawnNext(Tutorial.DoTutorial);
                yield return TinyCoro.Join(tutorialCoro);
            }

            Music = gameObject.GetComponentsInChildren<AudioBehaviour>().First();
            Music.SetTrack(AudioTrack.Menu);

            AutoScroller = new MapScroller(new Vector3(0, 0, 50));

            Timer = new Timer();
            Timer.InfiniteTime = true;

            var playerSpeed = 75f;
            var botSpeed = 30f;

            var parties = new Marionette[][]
            {
                new Marionette[]
                {
                    new Marionette(ControlScheme.Player, DirVertical.Down, Assets.Mars.Mar00Prefab, playerSpeed, playerSpeed, null),
                },
                new Marionette[]
                {
                    new Marionette(ControlScheme.Ai, DirVertical.Up, Assets.Mars.Mar01Prefab, botSpeed, botSpeed,
                    new[]
                    {
                        "#My aggression", "needs an outlet", "and you are", "the first",
                        "#person I have", "found", "", "",
                    }),
                    new Marionette(ControlScheme.Ai, DirVertical.Up, Assets.Mars.Mar01Prefab, botSpeed, botSpeed,
                    new[]
                    {
                        "#Look!", "no strings on", "me", "",
                        "I will just", "blast you out", "of the sky then", "",
                        "#Eeeek!", "", "", "",
                    }),
                    new Marionette(ControlScheme.Ai, DirVertical.Up, Assets.Mars.Mar01Prefab, botSpeed, botSpeed,
                    new[]
                    {
                        "Is your sprite", "a cleric or", "a ninja?", "",
                        "#I am a", "penguin", "puppet", "",
                        "Really?!", "Ha", "", "",
                        "#Well", "", "", "",
                        "#What are", "you then?", "", "",
                        "#Some kind of", "talking mushroom?", "", "",
                        "No", "thats", "just a pony tail", "",
                    }),
                    new Marionette(ControlScheme.Ai, DirVertical.Up, Assets.Mars.Mar01Prefab, botSpeed, botSpeed,
                    new[]
                    {
                        "#TODO: WRITE CLEVER", "LINE HERE", "", "",
                        "Whoops", "", "", "",
                    }),

                    new Marionette(ControlScheme.Ai, DirVertical.Up, Assets.Mars.Mar01Prefab, botSpeed, botSpeed,
                    new[]
                    {
                        "Where did you", "learn all these", "spells?", "",
                        "#Harvard", "", "", "",
                        "Penguins can", "go to havard?", "", "",
                        "#I AM A", "NINJA", "", "",
                        "#And my ", "classmates never", "saw me", "",
                        "You didn't", "attend classes", "", "did you?",
                        "#...", "", "", "",
                    }),
                    new Marionette(ControlScheme.Ai, DirVertical.Up, Assets.Mars.Mar01Prefab, botSpeed, botSpeed,
                    new[]
                    {
                        "So ", "you are the", "leader?", "",
                        "#Yes!", "you have done", "well to get", "this far miss",
                        "#mushroom", "", "", "",
                        "I AM NOT A", "MUSHROOM", "", "",
                    }),
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

                yield return TinyCoro.Wait(5f);
                
                var introText = new string[]
                {
                    "I feel safe", "at the ","bottom of the","screen",
                };

                var tip = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(introText));
                yield return TinyCoro.Join(tip);

                yield return TinyCoro.Wait(5f);

                freeRoam.Kill();
            }


            var intoDone = false;
            
            //While both parties can fight
            while (parties.All(p => p.Any(m => m.Alive)))
            {
                Music.SetTrack(AudioTrack.Combat);
                for (int iParty = 0; iParty < parties.Length; /*iterated below*/)
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

                    if(!intoDone)
                    {
                        intoDone = true;
                        
                        Debug.Log("Start free roam");
                        var freeRoam = TinyCoro.SpawnNext(() => player.DoMove(Role.Attacking));
                        var freeRoamFor = 5f;
                        while (freeRoamFor > 0f)
                        {
                            yield return null;

                            if (DialogPopup.Stack.Count == 0)
                                freeRoamFor -= Time.deltaTime;

                            Timer.Time = freeRoamFor;
                        }
                        Debug.Log("Stop free roam");
                        freeRoam.Kill();

                        //Puppet chat
                        defender.SetActive(true);

                        var msg = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(defender.Chat));
                        yield return TinyCoro.Join(msg);
                    }
                    else
                    {
                        defender.SetActive(true);
                    }


                    var attackerTurn = TinyCoro.SpawnNext(() => attacker.DoTurn(defender));
                    yield return TinyCoro.Join(attackerTurn);

                    var bullets = new List<Bullet>(Main.S.ActiveBullets);
                    foreach (var b in bullets)
                        b.Dispose();

                    defender.SetActive(defender.Alive);

                    if (!defender.Alive
                        && defender.Control == ControlScheme.Ai)
                    {
                        Music.SetTrack(AudioTrack.Menu);

                        //Find prize
                        BetterPattern prize = defender.Patterns.FirstOrDefault(dp => attacker.Patterns.All(ap => ap.Name != dp.Name));
                        if(prize == null)
                        {
                            var allPatterns = BetterPatterns.GeneratePatternSets();
                            prize = allPatterns.FirstOrDefault(dp => attacker.Patterns.All(ap => ap.Name != dp.Name));
                        }

                        if(prize != null)
                        {
                            if(attacker.Patterns.Count >= 14)
                                attacker.Patterns.RemoveAt(13);

                            attacker.Patterns.Add(prize);

                            var prizeDialog = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(new[] { "You learned", prize.Name }));
                            yield return TinyCoro.Join(prizeDialog);
                        }

                        if (defendingParty.Any(m => m.Alive))
                        {
                            //Chill for a bit
                            var freeRoam = TinyCoro.SpawnNext(() => attacker.DoMove(Role.Attacking));
                            yield return TinyCoro.Wait(5f);
                            freeRoam.Kill();

                            intoDone = false;                           
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
                        Music.SetTrack(AudioTrack.Menu);
                        var msg = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(new[] { "Game Over" }));
                        yield return TinyCoro.Join(msg);
                    }
                    else
                    {
                        iParty++;
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

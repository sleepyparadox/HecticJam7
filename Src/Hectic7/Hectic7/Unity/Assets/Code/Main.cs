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
        static bool tutorial = true;

        public IEnumerator DoGame()
        {
            //willInto = false;
            bool willInto = true;


            Music = gameObject.GetComponentsInChildren<AudioBehaviour>().First();
            Music.SetTrack(AudioTrack.None);

            if (tutorial)
            {
                var tutorialCoro = TinyCoro.SpawnNext(Tutorial.DoTutorial);
                yield return TinyCoro.Join(tutorialCoro);
                tutorial = false;
            }

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
                    new Marionette("Player", ControlScheme.Player, DirVertical.Down, Assets.Mars.Mar00Prefab, playerSpeed, playerSpeed, null, 0,0,0),
                },
                new Marionette[]
                {
                    new Marionette("Mook", ControlScheme.Ai, DirVertical.Up, Assets.Mars.Mar01Prefab, botSpeed, botSpeed, null, 1, 1, 0),
                    new Marionette("Rouge", ControlScheme.Ai, DirVertical.Up, Assets.Mars.Mar01Prefab, botSpeed, botSpeed,
                    new[]
                    {
                        "#My aggression", "needs an outlet", "", "",
                    }, 1, 0.4f, 0),
                    new Marionette("Priest", ControlScheme.Ai, DirVertical.Up, Assets.Mars.Mar01Prefab, botSpeed, botSpeed,
                    new[]
                    {
                        "#You lack of", "faith quite", "disturbing", "",
                    },1, 0.1f, 0.1f),
                    new Marionette("Ninja", ControlScheme.Ai, DirVertical.Up, Assets.Mars.Mar01Prefab, botSpeed, botSpeed,
                    new[]
                    {
                        "#", "", "", "",
                        "???", "", "", "",
                        "#", "", "", "",
                        "Oh,", "", "", "",
                        "I get it", "", "", "",
                    }, 1, 0.1f, 0f),
                    new Marionette("Penguin", ControlScheme.Ai, DirVertical.Up, Assets.Mars.Mar01Prefab, botSpeed, botSpeed,
                    new[]
                    {
                        "#Well you look", "like a mushroom", "", "",
                    }, 0.8f, 0, 0.2f),
                    new Marionette("Surgeon", ControlScheme.Ai, DirVertical.Up, Assets.Mars.Mar01Prefab, botSpeed, botSpeed,
                    new[]
                    {
                        "#TODO: WRITE CLEVER", "BANTER", "", "",
                        "Whoops", "", "", "",
                    }, 0.5f, 0, 0.5f),
                    new Marionette("Dark Lord", ControlScheme.Ai, DirVertical.Up, Assets.Mars.Mar01Prefab, botSpeed, botSpeed,
                    new[]
                    {
                        "#Join me", "and together we", "can rule the", "world as",
                        "#lord and", "mushroom", "", "",
                        "", "", "", "",
                        "Nah", "", "", "",
                    }, 0, 0f, 1f),
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

            var intoDone = false;
            
            //While both parties can fight
            while (parties.All(p => p.Any(m => m.Alive)))
            {
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
                        var freeRoamFor = 5f;
                        var freeRoam = TinyCoro.SpawnNext(() => player.DoMove(Role.Attacking, null, () => freeRoamFor = 0f));
                        while (freeRoamFor > 0f)
                        {
                            yield return null;
                            var bullets2Dispose = new List<Bullet>(Main.S.ActiveBullets);
                            foreach (var b in bullets2Dispose)
                                b.Dispose();

                            if (DialogPopup.Stack.Count == 0)
                                freeRoamFor -= Time.deltaTime;

                            Timer.Time = freeRoamFor;
                        }
                        Debug.Log("Stop free roam");
                        freeRoam.Kill();

                        //Puppet chat
                        defender.SetActive(true);
                        Music.SetTrack(AudioTrack.Combat);

                        var msg = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(new[] { "a violent", defender.Name + " appears" }));
                        yield return TinyCoro.Join(msg);

                        if(defender.Chat != null)
                        {
                            msg = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(defender.Chat));
                            yield return TinyCoro.Join(msg);
                        }
                    }
                    else
                    {
                        defender.SetActive(true);
                    }

                    var attackerTurn = TinyCoro.SpawnNext(() => attacker.DoTurn(defender, defendingParty));
                    yield return TinyCoro.Join(attackerTurn);

                    var bullets = new List<Bullet>(Main.S.ActiveBullets);
                    foreach (var b in bullets)
                        b.Dispose();

                    defender.SetActive(defender.Alive);

                    if (!defender.Alive
                        && defender.Control == ControlScheme.Ai)
                    {
                        intoDone = false;
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
                        else if(! Marionette.EditUnlocked)
                        {
                            Marionette.EditUnlocked = true;
                            var editUnlockedDialog = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(new[] { "Edit unlocked!" }));
                            yield return TinyCoro.Join(editUnlockedDialog);
                        }

                        if (defendingParty.All(m => !m.Alive))
                        {
                            var msg = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(new[] 
                            {
                                "You win!", "","","",
                                "Thanks for playing", "","","",
                                "Created by Don Logan", "", "", "",
                                "Music by Jared Hahn", "", "", "",
                                "Made for", "GameBoy Jam 2015" , "Rules: Only 4 colors", "Rules: 160px x 144px",
                                "Made for", "Hectic Jam 7", "Aug 15 to 17", "Theme: Puppet Master", "",
                            }));
                            yield return TinyCoro.Join(msg);
                            break;
                        }
                    }
                    else if (!defender.Alive
                      && defender.Control == ControlScheme.Player)
                    {
                        Music.SetTrack(AudioTrack.Menu);
                        var msg = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(new[] 
                        {
                            "Game Over", "", "","",
                             "Thanks for playing", "","","",
                                "Created by Don Logan", "", "", "",
                                "Music by Jared Hahn", "", "", "",
                                "Made for", "GameBoy Jam 2015" , "Rules: Only 4 colors", "Rules: 160px x 144px",
                                "Made for", "Hectic Jam 7", "Aug 15 to 17", "Theme: Puppet Master", ""
                        }));
                        yield return TinyCoro.Join(msg);
                    }
                    else
                    {
                        iParty++;
                    }
                }
            }

            yield return TinyCoro.Wait(1f);

            //Retry msg
            {
                var msg = TinyCoro.SpawnNext(() => ChattyDialog.DoChattyDialog(new[] { "Retry?" }));
                yield return TinyCoro.Join(msg);
            }

            Application.LoadLevel(0);
        }
    }
}

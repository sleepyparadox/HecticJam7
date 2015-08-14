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
        public static Vector3 ClampToMap (Vector3 src)
        {
            return new Vector3(Mathf.Clamp(src.x, MapSize.x * -0.5f, MapSize.x * 0.5f), Mathf.Clamp(src.y, MapSize.y * -0.5f, MapSize.y * 0.5f), src.z);
        }

        public static float Left { get { return MapSize.x * -0.5f; } }
        public static float Right { get { return MapSize.x * 0.5f; } }
        public static float Top { get { return MapSize.y * 0.5f; } }
        public static float Bottom { get { return MapSize.y * -0.5f; } }

        public static Vector3 PlayerSpot { get { return new Vector3(0, Bottom * 0.9f);  } }
        public static Vector3 FoeSpot { get { return new Vector3(0, Top * 0.9f); } }

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
            var player = new Player();
            var foe = new Foe();

            while (true)
            {
                player.WorldPosition = PlayerSpot;
                foe.WorldPosition = FoeSpot;
                var playerTurn = TinyCoro.SpawnNext(() => player.DoTurn(foe));
                yield return TinyCoro.Join(playerTurn);

                if (foe.Dead)
                    foe.Dead = false;

                if (player.Dead)
                    break;

                player.WorldPosition = PlayerSpot;
                foe.WorldPosition = FoeSpot;
                var foeTurn = TinyCoro.SpawnNext(() => foe.DoTurn(player));
                yield return TinyCoro.Join(foeTurn);

                if (foe.Dead)
                    foe.Dead = false;

                if (player.Dead)
                    break;
            }

            Application.LoadLevel(0);
        }

    }
}

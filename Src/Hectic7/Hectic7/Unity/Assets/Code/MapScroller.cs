using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Hectic7
{
    public class MapScroller : UnityObject
    {
        Vector3 RealPos;
        float Speed = 50f;
        public MapScroller(Vector3 pos)
        {
            RealPos = pos;
            WorldPosition = RealPos.Snap();

            for(var x = 0; x < 20; ++x)
            {
                for (var y = 0; y < 40; ++y)
                {
                    if(UnityEngine.Random.Range(0, 100) < 10)
                    {
                        var tile = new UnityObject(Assets.Tiles.TilePrefab);
                        tile.Parent = this;
                        tile.LocalPosition = new Vector3(x, y, 0) * 8f;
                        tile.UnityUpdate += AutoRespawnTile;
                    }

                }
            }

            UnityUpdate += MoveDown;
        }

        void MoveDown(UnityObject me)
        {
            RealPos.y -= Speed * Time.deltaTime;
            WorldPosition = RealPos.Snap();
        }

        static void AutoRespawnTile(UnityObject tile)
        {
            if(tile.WorldPosition.y < -16)
            {
                tile.LocalPosition += new Vector3(0, 160, 0);
            }
        }
    }
}

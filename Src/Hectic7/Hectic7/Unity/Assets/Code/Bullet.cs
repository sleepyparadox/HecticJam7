﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityTools_4_6;
using UnityEngine;

namespace Hectic7
{
    
    public class Bullet : UnityObject
    {
        public Vector3 RealPosition;
        public Vector3 Size;
        public Vector3 Center
        {
            get
            {
                return RealPosition + (Size * 0.5f);
            }
            set
            {
                RealPosition = value - (Size * 0.5f);
                WorldPosition = RealPosition.Snap();
            }
        }

        public readonly UnityObject Owner;
        public Vector3 Velocity;

        public Bullet(UnityObject owner, BulletType bulletType)
            : base(PrefabFromType(bulletType))
        {
            if (bulletType == BulletType.BulletSmall)
                Size = new Vector3(8, 8, 0);
            else
                Size = new Vector3(16, 16, 0);

            Owner = owner;
            UnityFixedUpdate += FixedUpdate;

            Main.S.ActiveBullets.Add(this);
            OnDispose += (me) => Main.S.ActiveBullets.Remove(this);
        }
        private void FixedUpdate(UnityObject me)
        {
            if (DialogPopup.Stack.Count > 0)
                return;         
            RealPosition += Velocity * Time.fixedDeltaTime;

            if (RealPosition.x  < Main.Left && Velocity.x < 0f)
                Velocity.x *= -1f;
            if (RealPosition.x + Size.x  > Main.Right && Velocity.x > 0f)
                Velocity.x *= -1f;

            if((RealPosition.y + Size.y < Main.Bottom && Velocity.y < 0)
                || (RealPosition.y > Main.Top && Velocity.y > 0))
            {
                Dispose();
            }

            WorldPosition = RealPosition.Snap();
        }

        static PrefabAsset PrefabFromType(BulletType type)
        {
            if (type == BulletType.BulletSmall)
                return Assets.Bullets.Bullet8Prefab;
            else
                return Assets.Bullets.Bullet16Prefab;
        }
    }
}

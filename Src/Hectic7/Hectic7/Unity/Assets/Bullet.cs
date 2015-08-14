using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityTools_4_6;
using UnityEngine;

namespace Hectic7
{
    public class Bullet : UnityObject
    {
        public readonly UnityObject Owner;
        public Vector3 Velocity;
        public Bullet(UnityObject owner, float scale = 50f)
            : base(GameObject.CreatePrimitive(PrimitiveType.Sphere))
        {
            Owner = owner;
            Transform.localScale = Vector3.one * scale;
            UnityFixedUpdate += FixedUpdate;

            Main.S.ActiveBullets.Add(this);
            OnDispose += (me) => Main.S.ActiveBullets.Remove(this);
        }
        private void FixedUpdate(UnityObject me)
        {
            WorldPosition += Velocity * Time.fixedDeltaTime;

            if (WorldPosition.x < Main.Left)
                Velocity.x *= -1f;
            if (WorldPosition.x > Main.Right)
                Velocity.x *= -1f;

            if(WorldPosition.y < Main.Bottom
                || WorldPosition.y > Main.Top)
            {
                Dispose();
            }
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Test25.World;
using System.Collections.Generic;

namespace Test25.Entities
{
    public class ExplosiveProjectile : Projectile
    {
        public ExplosiveProjectile(Vector2 position, Vector2 velocity, Texture2D texture)
            : base(position, velocity, texture)
        {
        }

        // Uses default OnHit which explodes
    }
}

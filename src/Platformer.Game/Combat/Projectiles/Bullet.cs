using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer.Game.Combat.Projectiles;

public class Bullet : Projectile
{
    public Bullet(Vector2 position, Vector2 velocity, int damage)
        : base(position, velocity, 12, 4, damage)
    {
        
    }

    public override void Draw(
        SpriteBatch spriteBatch,
        Texture2D pixel
    )
    {
        spriteBatch.Draw(
            pixel,
            Hitbox,
            Color.Yellow
        );
    }
}
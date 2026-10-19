using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer.Game.Combat.Projectiles;

public abstract class Projectile
{
    public Vector2 Position { get; protected set; }
    public Vector2 Velocity { get; protected set; }

    public int Width { get; }
    public int Height { get; }
    public int Damage { get; protected set; }

    public bool IsActive { get; protected set; } = true;

    public Rectangle Hitbox => new(
            (int)Position.X,
            (int)Position.Y,
            Width,
            Height
    );

    protected Projectile(
        Vector2 position,
        Vector2 velocity,
        int width,
        int height,
        int damage
    )
    {
        Position = position;
        Velocity = velocity;
        Width = width;
        Height = height;
        Damage = damage;
    }
    public virtual void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        Position += Velocity * deltaTime;
    }

    public virtual void OnCollision()
    {
        IsActive = false;
    }

    public abstract void Draw(SpriteBatch spriteBatch, Texture2D pixel);
}
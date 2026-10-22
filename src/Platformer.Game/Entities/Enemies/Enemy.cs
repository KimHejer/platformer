using Microsoft.Xna.Framework;
using Platformer.Game.Physics;

namespace Platformer.Game.Entities.Enemies;

public abstract class Enemy
{
    public Vector2 Position { get; protected set; }
    public Vector2 Velocity { get; protected set; }

    public int Width { get; }
    public int Height { get; }

    public Health Health { get; }

    public int ContactDamage { get; }

    public bool IsDead => Health.IsDead;

    public Rectangle Hitbox => new(
        (int)Position.X,
        (int)Position.Y,
        Width,
        Height
    );

    protected Enemy(
        Vector2 position,
        int width,
        int height,
        int maxHealth,
        int contactDamage
    )
    {
        Position = position;
        Width = width;
        Height = height;
        Health = new Health(maxHealth);
        ContactDamage = contactDamage;
    }

    public virtual void Update(GameTime gameTime)
    {
        if (IsDead)
            return;
        
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        Velocity = Gravity.Apply(Velocity, dt);
    }

    public void TakeDamage(int amount)
    {
        if (!IsDead)
            Health.TakeDamage(amount);
    }

    public void SetPosition(Vector2 position)
    {
        Position = position;
    }

    public void SetVelocity(Vector2 velocity)
    {
        Velocity = velocity;
    }

    public virtual void Kill()
    {
        Health.Kill();
    }

    public abstract void Reset();
}
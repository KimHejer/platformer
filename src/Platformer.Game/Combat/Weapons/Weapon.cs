using System;
using Microsoft.Xna.Framework;
using Platformer.Game.Combat.Projectiles;

namespace Platformer.Game.Combat.Weapons;

public abstract class Weapon
{
    public float FireRate { get; }
    public int Damage { get; }

    private float _cooldown;

    protected Weapon(float fireRate, int damage)
    {
        if (fireRate <= 0f)
            throw new ArgumentOutOfRangeException(nameof(fireRate));

        FireRate = fireRate;
        Damage = damage;
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _cooldown = MathF.Max(0f, _cooldown - deltaTime);
    }

    public Projectile? TryFire(Vector2 origin, Vector2 direction)
    {
        if (_cooldown > 0f)
            return null;
        
        if (direction == Vector2.Zero)
            return null;
        
        _cooldown = 1f / FireRate;

        return CreateProjectile(origin, Vector2.Normalize(direction));
    }

    protected abstract Projectile CreateProjectile(
        Vector2 origin,
        Vector2 direction
    );
}
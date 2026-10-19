using Microsoft.Xna.Framework;
using Platformer.Game.Combat.Projectiles;

namespace Platformer.Game.Combat.Weapons;

public class Rifle : Weapon
{
    private const float BulletSpeed = 800f;

    public Rifle() : base(
        fireRate: 8f,
        damage: 20
    ) 
    {

    }

    protected override Projectile CreateProjectile(
        Vector2 origin,
        Vector2 direction
    )
    {
        return new Bullet(
            origin,
            direction * BulletSpeed,
            Damage
        );
    }
}
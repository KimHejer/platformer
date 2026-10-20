using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Game.Combat.Projectiles;
using Platformer.Game.World;
using Platformer.Game.Hazards;
using Platformer.Game.Entities.Enemies;

namespace Platformer.Game.Combat;

public class ProjectileManager
{
    private readonly List<Projectile> _projectiles = new();

    public void Add(Projectile projectile)
    {
        _projectiles.Add(projectile);
    }

    public void Update(
        GameTime gameTime,
        IReadOnlyList<IPlatform> platforms,
        IReadOnlyList<Spike> spikes,
        IReadOnlyList<Enemy> enemies
    )
    {
        foreach (Projectile projectile in _projectiles)
        {
            projectile.Update(gameTime);

            foreach (IPlatform platform in platforms)
            {
                if (platform.IsOneWay)
                    continue;

                if (projectile.Hitbox.Intersects(platform.Bounds))
                {
                    projectile.OnCollision();
                    break;
                }
            }

            if (!projectile.IsActive)
                continue;
            
            foreach (Spike spike in spikes)
            {
                if (spike.Touches(projectile.Hitbox))
                {
                    projectile.OnCollision();
                    break;
                }
            }

            if (!projectile.IsActive)
                continue;
            
            foreach (Enemy enemy in enemies)
            {
                if (enemy.IsDead)
                    continue;

                if (projectile.Hitbox.Intersects(enemy.Hitbox)) 
                {
                    enemy.TakeDamage(projectile.Damage);
                    projectile.OnCollision();
                    break;
                }
            }
        }

        _projectiles.RemoveAll(p => !p.IsActive);
    }

    public void Draw(
        SpriteBatch spriteBatch,
        Texture2D pixel
    )
    {
        foreach (Projectile projectile in _projectiles)
        {
            projectile.Draw(spriteBatch, pixel);
        }
    }

    public void Clear()
    {
        _projectiles.Clear();
    }
}

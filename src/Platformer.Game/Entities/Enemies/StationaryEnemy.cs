using Microsoft.Xna.Framework;

namespace Platformer.Game.Entities.Enemies;

public class StationaryEnemy : Enemy
{
    public StationaryEnemy(
        Vector2 position,
        int width,
        int height,
        int maxHealth,
        int contactDamage
    ) : base(
        position,
        width,
        height,
        maxHealth,
        contactDamage
    )
    {
        Velocity = Vector2.Zero;
    }
}
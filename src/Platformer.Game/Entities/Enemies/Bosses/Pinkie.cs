
using Microsoft.Xna.Framework;
using Platformer.Game.World;

namespace Platformer.Game.Entities.Enemies.Bosses;

public sealed class Pinkie : Enemy
{
    private readonly FloatingPlatform _platform;

    public Pinkie(
        FloatingPlatform platform,
        int width,
        int height,
        int maxHealth
    ) : base(
        Vector2.Zero,
        width,
        height,
        maxHealth,
        0,
        "Pinkie"
    )
    {
        _platform = platform;
        SyncPosition();
    }

    public override void Update(GameTime gameTime)
    {
        // No gravity or independent movement.
        SyncPosition();
    }

    private void SyncPosition()
    {
        Rectangle platformBounds = _platform.Bounds;

        Position = new Vector2(
            platformBounds.Center.X - Width / 2f,
            platformBounds.Top - Height
        );

        Velocity = Vector2.Zero;
    }

    public override void Reset()
    {
        Health.Reset();
        SyncPosition();
    }
}

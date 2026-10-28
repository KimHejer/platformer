using Microsoft.Xna.Framework;

namespace Platformer.Game.World.Platforms.Movement;

public sealed class LinearMovement : IPlatformMovement
{
    public Vector2 Velocity { get; set; }

    public LinearMovement(Vector2 velocity)
    {
        Velocity = velocity;
    }

    public Vector2 Update(
        Vector2 currentPosition,
        GameTime gameTime)
    {
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        return currentPosition + Velocity * dt;
    }

    public void Reset()
    {
    }
}
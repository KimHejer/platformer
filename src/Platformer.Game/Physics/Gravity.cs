using Microsoft.Xna.Framework;

namespace Platformer.Game.Physics;

public static class Gravity
{
    public const float Acceleration = 1200f;

    public static Vector2 Apply(
        Vector2 velocity,
        float deltaTime,
        float multiplier = 1f
    )
    {
        velocity.Y += Acceleration * multiplier * deltaTime;
        return velocity;
    }
}
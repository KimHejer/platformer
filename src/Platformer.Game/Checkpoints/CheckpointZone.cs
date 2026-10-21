using Microsoft.Xna.Framework;

namespace Platformer.Game.Checkpoints;

public class CheckpointZone
{
    public Vector2 Center { get; }
    public float Radius { get; }

    public CheckpointZone(Vector2 center, float radius)
    {
        Center = center;
        Radius = radius;
    }

    public bool Contains(Rectangle hitbox)
    {
        float closestX = MathHelper.Clamp(
            Center.X, hitbox.Left, hitbox.Right
        );

        float closestY = MathHelper.Clamp(
            Center.Y, hitbox.Top, hitbox.Bottom
        );

        Vector2 closesPoint = new(closestX, closestY);

        return Vector2.DistanceSquared(Center, closesPoint) <= Radius * Radius;
    }
}
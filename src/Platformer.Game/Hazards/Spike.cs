using Microsoft.Xna.Framework;
using Platformer.Game.Physics;

namespace Platformer.Game.Hazards;

public class Spike
{
    public Triangle Shape { get; }

    public Spike(Vector2 a, Vector2 b, Vector2 c)
    {
        Shape = new Triangle(a, b, c);
    }

    public bool Touches(Rectangle playerHitbox)
    {
        return TriangleCollision.Intersects(Shape, playerHitbox);
    }
}
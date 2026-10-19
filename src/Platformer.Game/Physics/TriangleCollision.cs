using System;
using Microsoft.Xna.Framework;

namespace Platformer.Game.Physics;

public static class TriangleCollision
{
    public static bool Intersects(Triangle triangle, Rectangle rect)
    {
        Vector2[] trianglePoints =
        {
            triangle.A,
            triangle.B,
            triangle.C
        };

        Vector2[] rectanglePoints =
        {
            new(rect.Left, rect.Top),
            new(rect.Right, rect.Top),
            new(rect.Right, rect.Bottom),
            new(rect.Left, rect.Bottom)
        };

        Vector2[] axes =
        {
            Vector2.UnitX,
            Vector2.UnitY,
            Perpendicular(triangle.B - triangle.A),
            Perpendicular(triangle.C - triangle.B),
            Perpendicular(triangle.A - triangle.C)
        };

        foreach (Vector2 axis in axes)
        {
            Project(trianglePoints, axis, out float minA, out float maxA);
            Project(rectanglePoints, axis, out float minB, out float maxB);

            if (maxA <= minB || maxB < minA)
                return false;
        }

        return true;
    }

    private static Vector2 Perpendicular(Vector2 edge)
    {
        return new Vector2(-edge.Y, edge.X);
    }

    private static void Project( Vector2[] points, Vector2 axis, out float min, out float max)
    {
        min = float.PositiveInfinity;
        max = float.NegativeInfinity;

        foreach (Vector2 point in points)
        {
            float projection = Vector2.Dot(point, axis);

            min = MathF.Min(min, projection);
            max = MathF.Max(max, projection);
        }
    }
}
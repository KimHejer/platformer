using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Platformer.Game.Entities;
using Platformer.Game.World;

namespace Platformer.Game.Physics;

public static class COllisionResolver
{
    public static void Move(
        Player player,
        IReadOnlyList<Platform> platforms,
        float deltaTime)
    {
        Vector2 position = player.Position;
        Vector2 velocity = player.Velocity;

        bool grounded = false;

        float moveX = velocity.X * deltaTime;

        foreach (Platform platform in platforms)
        {
            Rectangle bounds = platform.Bounds;

            bool vericalOverlap = 
                position.Y < bounds.Bottom &&
                position.Y + Player.Height > bounds.Top;

            if (!vericalOverlap)
                continue;

            if (moveX > 0 && position.X + Player.Width <= bounds.Left)
            {
                float distance = bounds.Left - (position.X + Player.Width);

                moveX = Math.Min(moveX, distance);
            }
        }

        if (moveX != velocity.X * deltaTime)
            velocity.X = 0;

        position.X += moveX;

        float moveY = velocity.Y * deltaTime;

        foreach (Platform platform in platforms)
        {
            Rectangle bounds = platform.Bounds;

            bool horizontalOverlap = 
                position.X < bounds.Right &&
                position.X + Player.Width > bounds.Left;

            if (!horizontalOverlap)
                continue;
            
            if (moveY > 0 && position.Y + Player.Height <= bounds.Top)
            {
                float distance = bounds.Top - (position.Y + Player.Height);

                moveY = Math.Min(moveY, distance);
            }
            else if (moveY < 0 && position.Y >= bounds.Bottom)
            {
                float distance = bounds.Bottom - position.Y;

                moveY = Math.Max(moveY, distance);
            }

            if (moveY != velocity.Y * deltaTime)
            {
                grounded = velocity.Y > 0;
                velocity.Y = 0;
            }

            position.Y += moveY;

            player.SetPosition(position);
            player.SetVelocity(velocity);
            player.SetgGrounded(grounded);
        }
    }
}
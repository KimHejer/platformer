using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;

using Platformer.Game.Entities;
using Platformer.Game.World;

namespace Platformer.Game.Physics;

public static class CollisionResolver
{
    public static void Move(
        Player player,
        IReadOnlyList<IPlatform> platforms,
        float deltaTime)
    {
        Vector2 position = player.Position;
        Vector2 velocity = player.Velocity;

        bool grounded = false;

        float moveX = velocity.X * deltaTime;

        foreach (IPlatform platform in platforms)
        {
            if (platform.IsOneWay)
                continue;

            Rectangle bounds = platform.Bounds;

            bool verticalOverlap = 
                position.Y < bounds.Bottom &&
                position.Y + Player.Height > bounds.Top;

            if (!verticalOverlap)
                continue;

            if (moveX > 0)
            {
                float playerRight = position.X + Player.Width;

                if (playerRight <= bounds.Left && playerRight + moveX >= bounds.Left)
                {
                    moveX = Math.Min(moveX, bounds.Left - playerRight);
                }
            }
            else if (moveX < 0f)
            {
                float playerLeft = position.X;

                if (playerLeft >= bounds.Right && playerLeft + moveX <= bounds.Right)
                {
                    moveX = Math.Max(moveX, bounds.Right - playerLeft);
                }
            }
        }

        if (moveX != velocity.X * deltaTime)
            velocity.X = 0;

        position.X += moveX;

        float moveY = velocity.Y * deltaTime;

        foreach (IPlatform platform in platforms)
        {
            if (platform.IsOneWay && player.WantsToDrop)
                continue;

            Rectangle bounds = platform.Bounds;

            bool horizontalOverlap = 
                position.X < bounds.Right &&
                position.X + Player.Width > bounds.Left;

            if (!horizontalOverlap)
                continue;
            
            if (moveY > 0)
            {
                float playerBottom = position.Y + Player.Height;

                if (playerBottom <= bounds.Top && playerBottom + moveY >= bounds.Top)
                {
                    moveY = Math.Min(moveY, bounds.Top - playerBottom);
                }
            }
            else if (moveY < 0f && !platform.IsOneWay)
            {
                float playerTop = position.Y;

                if (playerTop >= bounds.Bottom && playerTop + moveY <= bounds.Bottom)
                {
                    moveY = Math.Max(moveY, bounds.Bottom - playerTop);
                }
            }
        }

        if (moveY != velocity.Y * deltaTime)
        {
            grounded = velocity.Y > 0;
            velocity.Y = 0;
        }

        position.Y += moveY;

        player.SetPosition(position);
        player.SetVelocity(velocity);
        player.SetGrounded(grounded);
    }
}
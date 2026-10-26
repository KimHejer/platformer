using Microsoft.Xna.Framework;

using Platformer.Game.Entities;
using Platformer.Game.World;
using Platformer.Game.Entities.Enemies;

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

        IPlatform? previousSupport = player.StandingPlatform;

        Vector2 carry = GetPlatformMovement(player);

        // Detach when the player jumps or drops through.
        if (player.WantsToDrop || velocity.Y < 0f)
        {
            previousSupport = null;
            carry = Vector2.Zero;
        }

        // Include platform displacement in horizontal movement.
        float requestedMoveX = velocity.X * deltaTime + carry.X;
        float moveX = requestedMoveX;

        foreach (IPlatform platform in platforms)
        {
            if (platform.IsOneWay)
                continue;

            if (platform is BrittlePlatform brittle && !brittle.IsActive)
                continue;

            Rectangle bounds = platform.Bounds;

            bool verticalOverlap =
                position.Y < bounds.Bottom &&
                position.Y + Player.Height > bounds.Top;

            if (!verticalOverlap)
                continue;

            if (moveX > 0f)
            {
                float playerRight = position.X + Player.Width;

                if (playerRight <= bounds.Left &&
                    playerRight + moveX >= bounds.Left)
                {
                    moveX = Math.Min(
                        moveX,
                        bounds.Left - playerRight
                    );
                }
            }
            else if (moveX < 0f)
            {
                float playerLeft = position.X;

                if (playerLeft >= bounds.Right &&
                    playerLeft + moveX <= bounds.Right)
                {
                    moveX = Math.Max(
                        moveX,
                        bounds.Right - playerLeft
                    );
                }
            }
        }

        if (moveX != requestedMoveX)
            velocity.X = 0f;

        position.X += moveX;

        float requestedMoveY = velocity.Y * deltaTime + carry.Y;
        float moveY = requestedMoveY;

        IPlatform? landingPlatform = null;
        bool grounded = false;

        foreach (IPlatform platform in platforms)
        {
            if (platform.IsOneWay && player.WantsToDrop)
                continue;

            if (platform is BrittlePlatform brittle && !brittle.IsActive)
                continue;

            Rectangle bounds = platform.Bounds;

            bool horizontalOverlap =
                position.X < bounds.Right &&
                position.X + Player.Width > bounds.Left;

            if (!horizontalOverlap)
                continue;

            float playerBottom = position.Y + Player.Height;

            if (platform == previousSupport &&
                velocity.Y >= 0f)
            {
                float previousPlatformTop = bounds.Top;

                if (platform is FloatingPlatform moving)
                    previousPlatformTop -= moving.Delta.Y;

                bool wasStanding =
                    MathF.Abs(playerBottom - previousPlatformTop) <= 2f;

                if (wasStanding)
                {
                    float allowedMoveY = bounds.Top - playerBottom;

                    if (allowedMoveY <= moveY)
                    {
                        moveY = allowedMoveY;
                        landingPlatform = platform;
                        grounded = true;
                    }
                }
            }

            if (velocity.Y >= 0f)
            {
                float previousTop = bounds.Top;

                if (platform is FloatingPlatform moving)
                    previousTop -= moving.Delta.Y;

                float relativeMoveY =
                    velocity.Y * deltaTime +
                    previousTop - bounds.Top;

                bool crossedTop =
                    playerBottom <= previousTop + 0.01f &&
                    playerBottom + relativeMoveY >= previousTop;

                if (crossedTop)
                {
                    float allowedMoveY =
                        bounds.Top - playerBottom;

                    if (allowedMoveY <= moveY)
                    {
                        moveY = allowedMoveY;
                        landingPlatform = platform;
                        grounded = true;
                    }
                }
            }
            else if (!platform.IsOneWay)
            {
                float playerTop = position.Y;

                if (playerTop >= bounds.Bottom &&
                    playerTop + moveY <= bounds.Bottom)
                {
                    moveY = Math.Max(
                        moveY,
                        bounds.Bottom - playerTop
                    );

                    velocity.Y = 0f;
                }
            }
        }

        position.Y += moveY;

        if (grounded)
        {
            velocity.Y = 0f;

            if (landingPlatform is BrittlePlatform brittle)
                brittle.StartBreaking();
        }

        player.SetPosition(position);
        player.SetVelocity(velocity);
        player.SetGrounded(grounded);
        player.SetStandingPlatform(
            grounded ? landingPlatform : null
        );
    }

    public static void Move(
        Enemy enemy,
        IReadOnlyList<IPlatform> platforms,
        float deltaTime
    )
    {
        if (enemy.IsDead)
            return;
        
        Vector2 position = enemy.Position;
        Vector2 velocity = enemy.Velocity;

        float moveY = velocity.Y * deltaTime;

        if (moveY > 0f)
        {
            foreach (IPlatform platform in platforms)
            {
                if (platform is BrittlePlatform brittle && !brittle.IsActive)
                    continue;
                
                Rectangle bounds = platform.Bounds;

                bool horizontalOverlap =
                    position.X < bounds.Right &&
                    position.X + enemy.Width > bounds.Left;

                if (!horizontalOverlap)
                    continue;
                
                float enemyBottom = position.Y + enemy.Height;

                if (enemyBottom <= bounds.Top &&
                    enemyBottom + moveY >= bounds.Top)
                {
                    float allowedMoveY = bounds.Top - enemyBottom;

                    if (allowedMoveY <= moveY)
                    {
                        moveY = allowedMoveY;
                    }
                }
            }
        }

        if (moveY != velocity.Y * deltaTime)
        {
            velocity.Y = 0f;
        }

        position.Y += moveY;

        enemy.SetPosition(position);
        enemy.SetVelocity(velocity);
    }

    private static Vector2 GetPlatformMovement(Player player)
    {
        if (player.StandingPlatform is not FloatingPlatform moving)
            return Vector2.Zero;
        
        if (!player.IsGrounded || player.WantsToDrop)
            return Vector2.Zero;
        
        if (player.Velocity.Y <0f)
            return Vector2.Zero;
        
        return moving.Delta;
    }
}
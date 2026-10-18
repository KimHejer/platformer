using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Platformer.Game.Entities;

public class Player
{
    public Vector2 Position { get; private set; }
    public Vector2 Velocity { get; private set; }

    public const int Width = 32;
    public const int Height = 32;

    private const float MoveSpeed = 200f;
    private const float Gravity = 1200f;

    public bool IsGrounded { get; private set; }

    public Rectangle Hitbox => new Rectangle(
        (int)Position.X,
        (int)Position.Y,
        Width,
        Height
    );

    public Player(Vector2 startPosition)
    {
        Position = startPosition;
        Velocity = Vector2.Zero;
    }

    public void Update(GameTime gameTime, KeyboardState keyboard)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        float horizontalInput = 0f;

        if (keyboard.IsKeyDown(Keys.A))
            horizontalInput -= 1f;

        if (keyboard.IsKeyDown(Keys.D))
            horizontalInput += 1f;

        Velocity = new Vector2(
            horizontalInput * MoveSpeed,
            Velocity.Y + Gravity * deltaTime
        );

    }

    public void SetPosition(Vector2 position)
    {
        Position = position;
    }

    public void SetVelocity(Vector2 velocity)
    {
        Velocity = velocity;
    }

    public void SetgGrounded(bool grounded)
    {
        IsGrounded = grounded;
    }
}
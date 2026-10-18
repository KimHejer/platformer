using System;
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
    private const float JumpReleaseGravityMultiplier = 2.5f;
    private const float FallGravityMultiplier = 1.5f;

    private const float FirstJumpHeight = 300f;
    private const float SecondJumpHeightRatio = 0.4f;
    private const float JumpCutMultiplier = 0.45f;
    private const int MaxJumps = 2;
    private int _jumpsUsed = 0;
    private bool _wasJumpHeld = false;

    private float _dropThrougTimer = 0f;
    public bool IgnoreOneWayPlatforms => _dropThrougTimer > 0f;
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

        _dropThrougTimer = MathF.Max(0f, _dropThrougTimer - deltaTime);

        if (keyboard.IsKeyDown(Keys.A))
            horizontalInput -= 1f;

        if (keyboard.IsKeyDown(Keys.D))
            horizontalInput += 1f;

        Velocity = new Vector2(
            horizontalInput * MoveSpeed,
            Velocity.Y
        );

        bool jumpHeld = keyboard.IsKeyDown(Keys.W);

        bool jumpPressed = jumpHeld && !_wasJumpHeld;
        bool jumpReleased = !jumpHeld && _wasJumpHeld;

        bool dropPressed = keyboard.IsKeyDown(Keys.S) && jumpPressed;

        if (dropPressed && IsGrounded)
        {
            _dropThrougTimer = 0.2f;
            IsGrounded = false;
            _jumpsUsed = 1;
            Velocity = new Vector2(Velocity.X, 100f);
        }
        else if (jumpPressed && _jumpsUsed < MaxJumps)
        {
            Jump();
        }

        float gravityMultiplier = 1f;

        if (Velocity.Y < 0f && !jumpHeld)
        {
            gravityMultiplier = JumpReleaseGravityMultiplier;
        }
        else if (Velocity.Y > 0f)
        {
            gravityMultiplier = FallGravityMultiplier;
        }

        _wasJumpHeld = jumpHeld;

        Velocity = new Vector2(
            Velocity.X, 
            Velocity.Y + Gravity * gravityMultiplier * deltaTime
        );

    }

    private void Jump()
    {
        float jumpHeight = _jumpsUsed == 0 
            ? FirstJumpHeight 
            : FirstJumpHeight * SecondJumpHeightRatio;
        
        float jumpSpeed = MathF.Sqrt(
            2f * Gravity * jumpHeight
            );
        
        Velocity = new Vector2(Velocity.X, -jumpSpeed);

        _jumpsUsed++;
        IsGrounded = false;
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

        if (grounded)
        {
            _jumpsUsed = 0;
        }
        else if (_jumpsUsed == 0)
        {
            _jumpsUsed = 1;
        }
    }
}
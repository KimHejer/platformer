using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Platformer.Game.Checkpoints;
using Platformer.Game.Physics;
using Platformer.Game.World;

namespace Platformer.Game.Entities;

public class Player
{
    public Vector2 Position { get; private set; }
    public Vector2 Velocity { get; private set; }

    public const int Width = 35;
    public const int Height = 60;

    private const float MoveSpeed = 300f;
    private const float GravityAcceleration = Gravity.Acceleration;
    private const float JumpReleaseGravityMultiplier = 2.5f;
    private const float FallGravityMultiplier = 1.5f;

    private const float FirstJumpHeight = 300f;
    private const float SecondJumpHeightRatio = 0.4f;
    private const int MaxJumps = 2;
    private int _jumpsUsed = 0;
    private bool _wasJumpHeld = false;

    public bool WantsToDrop { get; private set; }
    public bool IsGrounded { get; private set; }
    public IPlatform? StandingPlatform { get; private set; }

    public Health Health { get; } = new Health(100);
    public bool IsDead => Health.IsDead;
    private float _invulnerabilityTimer = 0f;
    private const float InvulnerabilityDuration = 0.75f;

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

        _invulnerabilityTimer = MathF.Max(0f, _invulnerabilityTimer - deltaTime);

        if (IsDead)
            return;

        float horizontalInput = 0f;

        if (keyboard.IsKeyDown(Keys.A))
            horizontalInput -= 1f;

        if (keyboard.IsKeyDown(Keys.D))
            horizontalInput += 1f;

        Velocity = new Vector2(
            horizontalInput * MoveSpeed,
            Velocity.Y
        );

        WantsToDrop = keyboard.IsKeyDown(Keys.S);

        bool jumpHeld = keyboard.IsKeyDown(Keys.W);
        bool jumpPressed = jumpHeld && !_wasJumpHeld;

        if (
            jumpPressed && 
            !WantsToDrop &&
            _jumpsUsed < MaxJumps
        )
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

        Velocity = Gravity.Apply(Velocity, deltaTime, gravityMultiplier);

    }

    private void Jump()
    {
        float jumpHeight = _jumpsUsed == 0 
            ? FirstJumpHeight 
            : FirstJumpHeight * SecondJumpHeightRatio;
        
        float jumpSpeed = MathF.Sqrt(
            2f * GravityAcceleration * jumpHeight
            );
        
        Velocity = new Vector2(Velocity.X, -jumpSpeed);

        _jumpsUsed++;
        IsGrounded = false;
        StandingPlatform = null;
    }

    public void SetPosition(Vector2 position)
    {
        Position = position;
    }

    public void SetVelocity(Vector2 velocity)
    {
        Velocity = velocity;
    }

    public void SetGrounded(bool grounded)
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

    public void TakeDamage(int amount)
    {
        if (IsDead || _invulnerabilityTimer > 0f)
            return;
        
        Health.TakeDamage(amount);

        if (!IsDead)
            _invulnerabilityTimer = InvulnerabilityDuration;
    }

    public void Kill()
    {
        if (IsDead)
            return;
        
        Health.Kill();
    }

    public void Respawn(Vector2 position)
    {
        Position = position;
        Velocity = Vector2.Zero;

        Health.Reset();

        IsGrounded = false;
        WantsToDrop = false;
        StandingPlatform = null;

        _jumpsUsed = 1;
        _wasJumpHeld = false;
        _invulnerabilityTimer = 0f;
    }

    public PlayerSnapshot CaptureState()
    {
        return new PlayerSnapshot(
            Position,
            Velocity,
            Health.Current,
            IsGrounded,
            WantsToDrop,
            _jumpsUsed,
            _wasJumpHeld,
            _invulnerabilityTimer
        );
    }

    public void RestoreState(PlayerSnapshot snapshot)
    {
        Position = snapshot.Position;
        Velocity = snapshot.Velocity;

        Health.Restore(snapshot.CurrentHealth);

        IsGrounded = snapshot.IsGrounded;
        WantsToDrop = snapshot.WantsToDrop;
        StandingPlatform = null;

        _jumpsUsed = snapshot.JumpsUsed;
        _wasJumpHeld = snapshot.WasJumpHeld;
        _invulnerabilityTimer = snapshot.InvulnerabilityTimer;
    }

    public bool RestoreJump()
    {
        if (_jumpsUsed <= 0)
            return false;

        _jumpsUsed--;

        return true;
    }

    public void SetStandingPlatform(IPlatform? platform)
    {
        StandingPlatform = platform;
    }
}
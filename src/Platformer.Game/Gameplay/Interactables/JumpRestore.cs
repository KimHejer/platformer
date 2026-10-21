using Microsoft.Xna.Framework;

namespace Platformer.Game.Gameplay.Interactables;

public class JumpRestore
{
    public Vector2 Position { get; }
    public float Radius { get; }

    public bool IsActive { get; private set; } = true;
    private const float RestoreDelay = 1f;
    private float _restoreCooldown;

    public JumpRestore(Vector2 position, float radius)
    {
        Position = position;
        Radius = radius;
    }

    public void Update(GameTime gameTime)
    {
        if (IsActive)
            return;
        
        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        _restoreCooldown += dt;

        if (_restoreCooldown >= RestoreDelay)
        {
            ReActivate();
        }
    }

    public void Activate()
    {
        if (!IsActive)
            return;
        
        IsActive = false;
        _restoreCooldown = 0f;
    }

    private void ReActivate()
    {
        _restoreCooldown = 0f;
        IsActive = true;
    }

    public bool Touches(Rectangle hitbox)
    {
        float closestX = MathHelper.Clamp(
            Position.X,
            hitbox.Left,
            hitbox.Right
        );

        float closestY = MathHelper.Clamp(
            Position.Y,
            hitbox.Top,
            hitbox.Bottom
        );

        float dx = Position.X - closestX;
        float dy = Position.Y - closestY;

        return dx * dx + dy * dy <= Radius * Radius;
    }
}
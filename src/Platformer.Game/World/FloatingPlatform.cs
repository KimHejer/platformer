using Microsoft.Xna.Framework;
using Platformer.Game.World.Platforms.Movement;

namespace Platformer.Game.World;

public class FloatingPlatform : SoftPlatform
{
    private readonly Vector2 _startPosition;
    private readonly Point _collisionOffset;
    private readonly IPlatformMovement _movement;

    public Vector2 Position { get; private set; }
    public Vector2 Delta { get; private set; }

    public FloatingPlatform(
        int x,
        int y,
        int width,
        int height,
        IPlatformMovement movement,
        Point collisionOffset = default
    ) : base(
        x + collisionOffset.X, 
        y + collisionOffset.Y, 
        width, 
        height)
    {
        _collisionOffset = collisionOffset;
        _startPosition = new Vector2(x, y);
        Position = _startPosition;
        _movement = movement;
    }

    // Compatibility constructor for existing JSON levels.
    public FloatingPlatform(
        int x,
        int y,
        int width,
        int height,
        Vector2 velocity
    ) : this(
        x,
        y,
        width,
        height,
        new LinearMovement(velocity))
    {
    }

    public void Update(GameTime gameTime)
    {
        Vector2 previousPosition = new(
            Bounds.X,
            Bounds.Y
        );

        Position = _movement.Update(Position, gameTime);

        Bounds = new Rectangle(
            (int)MathF.Round(Position.X) + _collisionOffset.X,
            (int)MathF.Round(Position.Y) + _collisionOffset.Y,
            Bounds.Width,
            Bounds.Height
        );

        Delta = new Vector2(
            Bounds.X,
            Bounds.Y
        ) - previousPosition;
    }

    public void Reset()
    {
        _movement.Reset();

        Position = _startPosition;
        Delta = Vector2.Zero;

        Bounds = new Rectangle(
            (int)MathF.Round(Position.X) + _collisionOffset.X,
            (int)MathF.Round(Position.Y) + _collisionOffset.Y,
            Bounds.Width,
            Bounds.Height
        );
    }
}
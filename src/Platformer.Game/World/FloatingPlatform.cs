using Microsoft.Xna.Framework;

namespace Platformer.Game.World;

public class FloatingPlatform : SoftPlatform
{
    private readonly Vector2 _startPosition;

    public Vector2 Position { get; private set; }
    public Vector2 Velocity { get; set; }
    public Vector2 Delta { get; private set; }

    public FloatingPlatform(int x, 
    int y, 
    int width, 
    int height, 
    Vector2 velocity
    ) : base(x, y, width, height)
    {
        _startPosition = new Vector2(x, y);

        Position = _startPosition;
        Velocity = velocity;
    }

    public void Update(GameTime gameTime)
    {
        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        Vector2 previousPosition = new Vector2(
            Bounds.X,
            Bounds.Y
        );

        Position += Velocity * deltaTime;

        Bounds = new Rectangle(
            (int)MathF.Round(Position.X),
            (int)MathF.Round(Position.Y),
            Bounds.Width,
            Bounds.Height
        );

        Delta = new Vector2(Bounds.X, Bounds.Y) - previousPosition;
    }

    public void Reset()
    {
        Position = _startPosition;
        Delta = Vector2.Zero;

        Bounds = new Rectangle(
            (int)_startPosition.X,
            (int)_startPosition.Y,
            Bounds.Width,
            Bounds.Height
        );
    }
}
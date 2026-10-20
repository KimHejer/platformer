using System.Runtime.Intrinsics.X86;
using Microsoft.Xna.Framework;

namespace Platformer.Game.Graphics;

public class Camera2D
{
    public Vector2 Position { get; private set; }

    private readonly int _viewportWidth;
    private readonly int _viewportHeight;

    private const float LeftBoundary = 0.30f;
    private const float RightBoundary = 0.70f;
    private const float TopBoundary = 0.25f;
    private const float BottomBoundary = 0.75f;

    public Camera2D(int viewportWidth, int viewportHeight)
    {
        _viewportWidth = viewportWidth;
        _viewportHeight = viewportHeight;
    }

    public void Update(Rectangle target, int levelWidth, int levelHeight)
    {
        float targetX = target.Center.X;
        float targetY = target.Center.Y;

        float left = Position.X + _viewportWidth * LeftBoundary;
        float right = Position.X + _viewportWidth * RightBoundary;

        float top = Position.Y + _viewportHeight * TopBoundary;
        float bottom = Position.Y + _viewportHeight * BottomBoundary;

        if (targetX < left)
        {
            Position = new Vector2(
                targetX - _viewportWidth * LeftBoundary,
                Position.Y
            );
        }
        else if (targetX > right)
        {
            Position = new Vector2(
                targetX - _viewportWidth * RightBoundary,
                Position.Y
            );
        }

        if (targetY < top)
        {
            Position = new Vector2(
                Position.X,
                targetY - _viewportHeight * TopBoundary
            );
        }
        else if (targetY > bottom)
        {
            Position = new Vector2(
                Position.X,
                targetY - _viewportHeight * BottomBoundary
            );
        }

        float maxX = MathHelper.Max(0, levelWidth - _viewportWidth);
        float maxY = MathHelper.Max(0, levelHeight - _viewportHeight);

        Position = new Vector2(
            MathHelper.Clamp(Position.X, 0, maxX),
            MathHelper.Clamp(Position.Y, 0, maxY)
        );
    }

    public void Reset()
    {
        Position = Vector2.Zero;
    }

    public Matrix Transform =>
        Matrix.CreateTranslation(
            -Position.X,
            -Position.Y,
            0f
        );
}
using Microsoft.Xna.Framework;

namespace Platformer.Game.World;

public class Platform
{
    public Rectangle Bounds { get; }

    public Platform(int x, int y, int width, int height)
    {
        Bounds = new Rectangle(x, y, width, height);
    }
}
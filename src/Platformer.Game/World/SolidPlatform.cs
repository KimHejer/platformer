using Microsoft.Xna.Framework;

namespace Platformer.Game.World;

public class SolidPlatform : Platform
{
    public Rectangle Bounds { get; }
    public bool IsOneWay => false;

    public SolidPlatform(int x, int y, int width, int height)
    {
        Bounds = new Rectangle(x, y, width, height);
    }
}
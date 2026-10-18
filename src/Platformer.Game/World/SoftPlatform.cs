using Microsoft.Xna.Framework;

namespace Platformer.Game.World;

public class SoftPlatform : IPlatform
{
    public Rectangle Bounds { get; }
    public bool IsOneWay => true;

    public SoftPlatform(int x, int y, int width, int height)
    {
        Bounds = new Rectangle(x, y, width, height);
    }
}
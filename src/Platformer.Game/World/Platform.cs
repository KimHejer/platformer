using Microsoft.Xna.Framework;

namespace Platformer.Game.World;

public interface Platform
{
    Rectangle Bounds { get; }
    bool IsOneWay { get; }
}
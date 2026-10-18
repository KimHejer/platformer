using Microsoft.Xna.Framework;

namespace Platformer.Game.World;

public interface IPlatform
{
    Rectangle Bounds { get; }
    bool IsOneWay { get; }
}
using Microsoft.Xna.Framework;

namespace Platformer.Game.World.Transitions;

public class LevelTransition
{
    public Rectangle Bounds { get; }
    public string TargetLevel { get; }

    public LevelTransition(Rectangle bounds, string targetLevel)
    {
        Bounds = bounds;
        TargetLevel = targetLevel;
    }

    public bool IsTriggered(Rectangle playerHitbox)
    {
        return Bounds.Intersects(playerHitbox);
    }
}
using Microsoft.Xna.Framework;

namespace Platformer.Game.World.Platforms;

public sealed record PlatformDefinition(
    string Id,
    Point Size,
    Rectangle CollisionBounds,
    PlatformVisual Visual
);
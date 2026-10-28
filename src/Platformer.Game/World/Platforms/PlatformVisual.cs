namespace Platformer.Game.World.Platforms;

public abstract record PlatformVisual;

public sealed record SpritePlatformVisual(
    string TexturePath
) : PlatformVisual;

public sealed record TiledPlatformVisual(
    uint RawGid
) : PlatformVisual;
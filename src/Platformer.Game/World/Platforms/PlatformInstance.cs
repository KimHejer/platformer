namespace Platformer.Game.World.Platforms;

public sealed class PlatformInstance
{
    public FloatingPlatform Platform { get; }
    public PlatformDefinition Definition { get; }

    public PlatformInstance(
        FloatingPlatform platform,
        PlatformDefinition definition)
    {
        Platform = platform;
        Definition = definition;
    }
}
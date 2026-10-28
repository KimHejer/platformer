namespace Platformer.Game.World.Platforms.Spawning;

public sealed class PlatformSpawnPattern
{
    public string Id { get; set; } = "";
    public bool Loop { get; set; } = true;
    public float Duration { get; set; }
    public List<PlatformSpawnEvent> Events { get; set; } = new();
}

public sealed class PlatformSpawnEvent
{
    public float Time { get; set; }
    public string PlatformId { get; set; } = "";

    public float X { get; set; }
    public float Y { get; set; }

    public float VelocityX { get; set; }
    public float VelocityY { get; set; }
}
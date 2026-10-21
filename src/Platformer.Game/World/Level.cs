using Microsoft.Xna.Framework;
using Platformer.Game.Checkpoints;
using Platformer.Game.Entities.Enemies;
using Platformer.Game.Hazards;

namespace Platformer.Game.World;

public class Level
{
    public string Name { get; }
    public int Width { get; }
    public int Height { get; }

    public Vector2 SpawnPosition { get; }

    public List<IPlatform> Platforms { get; } = new();
    public List<Spike> Spikes { get; } = new();
    public List<Enemy> Enemies { get; } = new();
    public List<CheckpointZone> Checkpoints { get; } = new();

    public Level(
        string name,
        int width,
        int height,
        Vector2 spawnPosition
    )
    {
        Name = name;
        Width = width;
        Height = height;
        SpawnPosition = spawnPosition;
    }
}
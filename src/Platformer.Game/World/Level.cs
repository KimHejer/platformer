using Microsoft.Xna.Framework;
using Platformer.Game.Checkpoints;
using Platformer.Game.Entities.Enemies;
using Platformer.Game.Gameplay.Interactables;
using Platformer.Game.Hazards;
using Platformer.Game.World.Transitions;
using Platformer.Game.World.Platforms;

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
    public List<LevelTransition> LevelTransitions { get; } = new();
    public List <JumpRestore> JumpRestores { get; } = new();

    public string? TileMapPath { get; set; }
    public HashSet<IPlatform> TiledPlatforms { get; } = new();
    public HashSet<Spike> TiledSpikes { get; } = new();
    public Dictionary<BrittlePlatform, List<(int Layer, int Index)>> 
        TiledBrittleTiles { get; } = new();

    public HashSet<JumpRestore> TiledJumpRestores { get; } = new();

    public HashSet<CheckpointZone> TiledCheckpoints { get; } = new();

    public HashSet<LevelTransition> TiledTransitions { get; } = new();

    public Dictionary<(int Layer, int Index), JumpRestore>
        TiledRestoreTiles { get; } = new();

    public List<PlatformInstance> PlatformInstances { get; } = new();

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

    public void AddPlatform(PlatformInstance instance)
    {
        PlatformInstances.Add(instance);
        Platforms.Add(instance.Platform);
    }

    public bool RemovePlatform(PlatformInstance instance)
    {
        bool removed = PlatformInstances.Remove(instance);

        if (removed)
            Platforms.Remove(instance.Platform);

        return removed;
    }
}
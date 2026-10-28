using Microsoft.Xna.Framework;
using Platformer.Game.Checkpoints;
using Platformer.Game.Entities.Enemies;
using Platformer.Game.Gameplay.Interactables;
using Platformer.Game.Hazards;
using Platformer.Game.World.Transitions;
using Platformer.Game.World.Platforms;
using Platformer.Game.Entities;

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
    public PlatformFactory? PlatformFactory { get; set; }
    public string? PlatformSpawnPattern { get; set; }

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

    public bool RemovePlatform(
        PlatformInstance instance,
        Player? player = null)
    {
        if (!PlatformInstances.Remove(instance))
            return false;

        if (player?.StandingPlatform == instance.Platform)
            player.SetStandingPlatform(null);

        Platforms.Remove(instance.Platform);

        return true;
    }

    public void UpdateMovingPlatforms(GameTime gameTime)
    {
        foreach (IPlatform platform in Platforms)
        {
            if (platform is FloatingPlatform floating)
                floating.Update(gameTime);
        }
    }

    public void ResetMovingPlatforms()
    {
        foreach (IPlatform platform in Platforms)
        {
            if (platform is FloatingPlatform floating)
                floating.Reset();
        }
    }
}
using System.Text.Json;
using Microsoft.Xna.Framework;
using Platformer.Game.Entities.Enemies;
using Platformer.Game.Hazards;
using Platformer.Game.Checkpoints;
using Platformer.Game.World.Transitions;
using Platformer.Game.Gameplay.Interactables;
using Platformer.Game.World.Tiled;

namespace Platformer.Game.World;

public static class LevelLoader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static Level Load(string path)
    {
        string json = File.ReadAllText(path);

        LevelData data = JsonSerializer.Deserialize<LevelData>(
            json,
            JsonOptions
        ) ?? throw new InvalidDataException($"Unable to read level: {path}");

        if (data.Width <= 0 || data.Height <= 0)
            throw new InvalidDataException("Invalid level dimensions.");
        
        Level level = new(
            data.Name,
            data.Width,
            data.Height,
            new Vector2(data.Spawn.X, data.Spawn.Y)
        );

        if (!string.IsNullOrWhiteSpace(data.TileMap))
        {
            string mapPath = Path.GetFullPath(Path.Combine(
                Path.GetDirectoryName(path)!,
                "..",
                "Maps",
                data.TileMap
            ));

            if (!File.Exists(mapPath))
                throw new FileNotFoundException(
                    "Tiled map not found.", mapPath);

            TiledMapData map = TiledMapData.Load(mapPath);

            level.TileMapPath = mapPath;

            TiledCollisionBuilder.AddToLevel(map, level);
            TiledGameplayBuilder.AddToLevel(map, level);
        }

        foreach (LevelObjectData obj in data.Objects)
        {
            if (obj.Type is "checkpoint" or "jumpRestore")
            {
                if (obj.Radius <= 0)
                    throw new InvalidDataException(
                        "Checkpoint radius must be greater than zero."
                    );
            }
            else if (obj.Width <= 0 || obj.Height <= 0)
                throw new InvalidDataException(
                    $"Invalid dimensions for '{obj.Type}'.");

            switch (obj.Type)
            {
                case "solidPlatform":
                    level.Platforms.Add(
                        new SolidPlatform(
                            obj.X, obj.Y,
                            obj.Width, obj.Height
                        )
                    );
                    break;
                
                case "softPlatform":
                    level.Platforms.Add(
                        new SoftPlatform(
                            obj.X, obj.Y,
                            obj.Width, obj.Height
                        )
                    );
                    break;

                case "brittlePlatform":
                    level.Platforms.Add(
                        new BrittlePlatform(
                            obj.X, obj.Y,
                            obj.Width, obj.Height
                        )
                    );
                    break;

                case "spike":
                    AddSpikes(level, obj);
                    break;

                case "enemy":
                    switch (obj.EnemyType)
                    {
                        case "stationary":
                            level.Enemies.Add(
                                new StationaryEnemy(
                                    new Vector2(obj.X, obj.Y),
                                    obj.Width,
                                    obj.Height,
                                    obj.Health,
                                    obj.Damage,
                                    obj.Sprite,
                                    obj.RespawnAble
                                )
                            );
                            break;

                        default:
                            throw new InvalidDataException(
                                $"Unknown enemy type: {obj.EnemyType}"
                            );
                    }
                    break;
                
                case "checkpoint":
                    level.Checkpoints.Add(
                        new CheckpointZone(
                            new Vector2(obj.X, obj.Y),
                            obj.Radius
                        )
                    );
                    break;

                case "levelTransition":
                    if (string.IsNullOrWhiteSpace(obj.TargetLevel))
                    {
                        throw new InvalidDataException(
                            "Level transition must specify a targetlevel."
                        );
                    }

                    level.LevelTransitions.Add(
                        new LevelTransition(
                            new Rectangle(obj.X, obj.Y, obj.Width, obj.Height),
                            obj.TargetLevel
                        )
                    );
                    break;
                
                case "jumpRestore":
                    if (obj.Radius <= 0) {
                        throw new InvalidDataException(
                            "JumpRestore radius must be greater than zero."
                        );
                    }

                    level.JumpRestores.Add(
                        new JumpRestore(
                            new Vector2(obj.X, obj.Y),
                            obj.Radius
                        )
                    );
                    break;
                
                default:
                    throw new InvalidDataException(
                        $"Unknown level object type: {obj.Type}."
                    );
            }
        }

        return level;
    }

    private static void AddSpikes(
        Level level,
        LevelObjectData obj
    )
    {
        if (obj.Direction != "up")
            throw new NotSupportedException(
                $"Unsupported spike direction: {obj.Direction}"
            );
        
        if (obj.Count < 1)
            throw new InvalidDataException(
                "Spike count must be at least 1."
            );
        
        for (int i = 0; i < obj.Count; i++)
        {
            int x = obj.X + i * obj.Width;
            int y = obj.Y;

            level.Spikes.Add(
                new Spike(
                    new Vector2(x, y + obj.Height),
                    new Vector2(x + obj.Width / 2f, y),
                    new Vector2(x + obj.Width, y + obj.Height)
                )
            );
        }
    }
}
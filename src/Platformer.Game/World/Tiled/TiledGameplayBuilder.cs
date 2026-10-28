
using Microsoft.Xna.Framework;
using Platformer.Game.Checkpoints;
using Platformer.Game.Gameplay.Interactables;
using Platformer.Game.World.Transitions;
using Platformer.Game.World.Platforms;

namespace Platformer.Game.World.Tiled;

public static class TiledGameplayBuilder
{
    public static void AddToLevel(
        TiledMapData map,
        Level level,
        PlatformFactory factory)
    {
        for (int layerIndex = 0; layerIndex < map.Layers.Count; layerIndex++)
        {
            var layer = map.Layers[layerIndex];

            for (int index = 0; index < layer.Tiles.Length; index++)
            {
                uint gid = layer.Tiles[index] & TiledMapData.GidMask;

                if (!map.ObjectTypes.TryGetValue(gid, out string? type))
                    continue;

                int x = (index % layer.Width) * map.TileWidth;
                int y = (index / layer.Width) * map.TileHeight;

                Vector2 center = new(
                    x + map.TileWidth / 2f,
                    y + map.TileHeight / 2f
                );

                switch (type)
                {
                    case "jumpRestore":
                    {
                        var restore = new JumpRestore(center, 12f);

                        level.JumpRestores.Add(restore);
                        level.TiledJumpRestores.Add(restore);
                        level.TiledRestoreTiles[(layerIndex, index)] = restore;

                        break;
                    }

                    case "checkpoint":
                    {
                        var checkpoint = new CheckpointZone(center, 48f);

                        level.Checkpoints.Add(checkpoint);
                        level.TiledCheckpoints.Add(checkpoint);

                        break;
                    }

                    case "levelTransition":
                    {
                        if (!map.ObjectTargets.TryGetValue(gid, out string? target) ||
                            string.IsNullOrWhiteSpace(target))
                        {
                            throw new InvalidDataException(
                                $"Level transition at ({x}, {y}) " +
                                $"in layer '{layer.Name}' has no destination. {gid}");
                        }

                        const int portalWidth = 96;
                        const int portalHeight = 128;

                        var bounds = new Rectangle(
                            (int)center.X - portalWidth / 2,
                            (int)center.Y - portalHeight / 2,
                            portalWidth,
                            portalHeight
                        );

                        var transition = new LevelTransition(
                            bounds,
                            target.Trim()
                        );

                        level.LevelTransitions.Add(transition);
                        level.TiledTransitions.Add(transition);

                        break;
                    }
                }
            }
        }

        AddMovingPlatforms(map, level, factory);
    }

    private static void AddMovingPlatforms(
        TiledMapData map,
        Level level,
        PlatformFactory factory)
    {
        foreach (TiledMapData.MapObject obj in map.Objects)
        {
            if (obj.Type is not ("MovingPlatform" or "floatingPlatform"))
                continue;

            Vector2 velocity = new(
                obj.GetFloat("velocityX"),
                obj.GetFloat("velocityY")
            );

            string platformId = obj.GetString("platformId");

            PlatformInstance instance;

            if (!string.IsNullOrWhiteSpace(platformId))
            {
                // Rectangle object referencing a reusable definition.
                instance = factory.Create(
                    platformId,
                    new Vector2(obj.X, obj.Y),
                    velocity
                );
            }
            else
            {
                // Backward compatibility with Tiled tile objects.
                if (obj.Gid == 0)
                {
                    throw new InvalidDataException(
                        $"MovingPlatform '{obj.Name}' must specify " +
                        "a platformId or use a tile object."
                    );
                }

                uint gid = obj.Gid & TiledMapData.GidMask;
                TiledMapData.Tileset tileset = map.GetTileset(gid);

                int width = (int)MathF.Round(
                    obj.Width > 0 ? obj.Width : tileset.TileWidth
                );

                int height = (int)MathF.Round(
                    obj.Height > 0 ? obj.Height : tileset.TileHeight
                );

                if (width <= 0 || height <= 0)
                    throw new InvalidDataException(
                        $"Invalid size for MovingPlatform '{obj.Name}'."
                    );

                // Orthogonal Tiled tile objects are bottom-left anchored.
                Vector2 position = new(
                    obj.X,
                    obj.Y - height
                );

                var definition = new PlatformDefinition(
                    $"tiled_object_{obj.Id}",
                    new Point(width, height),
                    new Rectangle(0, 0, width, height),
                    new TiledPlatformVisual(obj.Gid)
                );

                instance = factory.Create(
                    definition,
                    position,
                    velocity
                );
            }

            level.AddPlatform(instance);
        }
    }
}

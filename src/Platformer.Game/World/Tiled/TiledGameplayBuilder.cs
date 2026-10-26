
using Microsoft.Xna.Framework;
using Platformer.Game.Checkpoints;
using Platformer.Game.Gameplay.Interactables;
using Platformer.Game.World.Transitions;

namespace Platformer.Game.World.Tiled;

public static class TiledGameplayBuilder
{
    public static void AddToLevel(TiledMapData map, Level level)
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
    }
}

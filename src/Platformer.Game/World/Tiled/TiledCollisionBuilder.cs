
using Microsoft.Xna.Framework;
using Platformer.Game.Hazards;

namespace Platformer.Game.World.Tiled;

public static class TiledCollisionBuilder
{
    public static void AddToLevel(
        TiledMapData map,
        Level level)
    {
        int width = map.Width;
        int height = map.Height;

        // The collision type occupying each map cell.
        string?[,] types = new string?[height, width];

        // Tracks which visual tile represents the collision cell.
        (int Layer, int Index)?[,] sources =
            new (int Layer, int Index)?[height, width];

        for (int layerIndex = 0;
             layerIndex < map.Layers.Count;
             layerIndex++)
        {
            var layer = map.Layers[layerIndex];

            if (!layer.Collision)
                continue;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;

                    uint gid = layer.Tiles[index]
                        & TiledMapData.GidMask;

                    if (!map.CollisionTypes.TryGetValue(
                        gid, out string? type))
                    {
                        continue;
                    }

                    if (types[y, x] != null)
                    {
                        throw new InvalidDataException(
                            $"Overlapping collision tiles at " +
                            $"({x}, {y}), layer '{layer.Name}'.");
                    }

                    types[y, x] = type;
                    sources[y, x] = (layerIndex, index);
                }
            }
        }

        bool[,] visited = new bool[height, width];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                string? type = types[y, x];

                if (type == null || visited[y, x])
                    continue;

                int runWidth = 0;

                while (x + runWidth < width &&
                       !visited[y, x + runWidth] &&
                       types[y, x + runWidth] == type)
                {
                    runWidth++;
                }

                int runHeight = 1;

                // Solid and spike areas can merge vertically.
                if (type is "solid" or "spike")
                {
                    while (y + runHeight < height)
                    {
                        bool valid = true;

                        for (int dx = 0; dx < runWidth; dx++)
                        {
                            if (visited[y + runHeight, x + dx] ||
                                types[y + runHeight, x + dx] != type)
                            {
                                valid = false;
                                break;
                            }
                        }

                        if (!valid)
                            break;

                        runHeight++;
                    }
                }

                var occupiedTiles =
                    new List<(int Layer, int Index)>();

                for (int dy = 0; dy < runHeight; dy++)
                {
                    for (int dx = 0; dx < runWidth; dx++)
                    {
                        int tx = x + dx;
                        int ty = y + dy;

                        visited[ty, tx] = true;

                        if (sources[ty, tx] is { } source)
                            occupiedTiles.Add(source);
                    }
                }

                int px = x * map.TileWidth;
                int py = y * map.TileHeight;
                int pw = runWidth * map.TileWidth;
                int ph = runHeight * map.TileHeight;

                switch (type)
                {
                    case "solid":
                    {
                        var platform = new SolidPlatform(
                            px, py, pw, ph);

                        level.Platforms.Add(platform);
                        level.TiledPlatforms.Add(platform);
                        break;
                    }

                    case "soft":
                    {
                        var platform = new SoftPlatform(
                            px, py, pw, ph);

                        level.Platforms.Add(platform);
                        level.TiledPlatforms.Add(platform);
                        break;
                    }

                    case "brittle":
                    {
                        var platform = new BrittlePlatform(
                            px, py, pw, ph);

                        level.Platforms.Add(platform);
                        level.TiledPlatforms.Add(platform);

                        level.TiledBrittleTiles[platform] =
                            occupiedTiles;

                        break;
                    }

                    case "spike":
                    {
                        // One upward triangle spanning the
                        // complete merged rectangle.
                        var spike = new Spike(
                            new Vector2(px, py + ph),
                            new Vector2(px + pw / 2f, py),
                            new Vector2(px + pw, py + ph)
                        );

                        level.Spikes.Add(spike);
                        level.TiledSpikes.Add(spike);
                        break;
                    }
                }
            }
        }
    }
}

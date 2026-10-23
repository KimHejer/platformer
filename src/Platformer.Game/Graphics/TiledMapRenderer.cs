
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Game.World;
using Platformer.Game.World.Tiled;

namespace Platformer.Game.Graphics;

public sealed class TiledMapRenderer : IDisposable
{
    private readonly TiledMapData _map;
    private readonly Dictionary<uint, Texture2D> _textures = new();
    private readonly Dictionary<(int Layer, int Index),
        BrittlePlatform> _brittleOwners = new();
    private readonly Dictionary<
        (int Layer, int Index),
        Platformer.Game.Gameplay.Interactables.JumpRestore
    > _restoreOwners;

    public TiledMapRenderer(
        GraphicsDevice graphicsDevice,
        TiledMapData map,
        Level level)
    {
        _map = map;

        _restoreOwners = level.TiledRestoreTiles;

        foreach (var tileset in map.Tilesets)
        {
            using Stream stream = File.OpenRead(
                tileset.ImagePath);

            _textures[tileset.FirstGid] =
                Texture2D.FromStream(graphicsDevice, stream);
        }

        foreach (var pair in level.TiledBrittleTiles)
        {
            foreach (var cell in pair.Value)
                _brittleOwners[cell] = pair.Key;
        }
    }

    public void Draw(
        SpriteBatch batch,
        bool foreground)
    {
        for (int layerIndex = 0;
             layerIndex < _map.Layers.Count;
             layerIndex++)
        {
            var layer = _map.Layers[layerIndex];

            if (!layer.Visible ||
                layer.Foreground != foreground)
            {
                continue;
            }

            for (int y = 0; y < layer.Height; y++)
            {
                for (int x = 0; x < layer.Width; x++)
                {
                    int index = y * layer.Width + x;

                    if (_brittleOwners.TryGetValue(
                        (layerIndex, index),
                        out BrittlePlatform? brittle) &&
                        !brittle.IsActive)
                    {
                        continue;
                    }

                    if (_restoreOwners.TryGetValue(
                        (layerIndex, index),
                        out var restore) &&
                        !restore.IsActive)
                    {
                        continue;
                    }

                    uint rawGid = layer.Tiles[index];
                    uint gid = rawGid & TiledMapData.GidMask;

                    if (gid == 0)
                        continue;

                    var tileset = _map.GetTileset(gid);

                    uint localId = gid - tileset.FirstGid;

                    int column = (int)(
                        localId % (uint)tileset.Columns);

                    int row = (int)(
                        localId / (uint)tileset.Columns);

                    Rectangle source = new(
                        tileset.Margin +
                            column * (tileset.TileWidth +
                                      tileset.Spacing),
                        tileset.Margin +
                            row * (tileset.TileHeight +
                                   tileset.Spacing),
                        tileset.TileWidth,
                        tileset.TileHeight
                    );

                    const uint flipH = 0x80000000u;
                    const uint flipV = 0x40000000u;
                    const uint flipD = 0x20000000u;

                    if ((rawGid & flipD) != 0)
                    {
                        throw new NotSupportedException(
                            "Diagonal tile flips are not yet supported.");
                    }

                    SpriteEffects effects = SpriteEffects.None;

                    if ((rawGid & flipH) != 0)
                        effects |= SpriteEffects.FlipHorizontally;

                    if ((rawGid & flipV) != 0)
                        effects |= SpriteEffects.FlipVertically;

                    Rectangle destination = new(
                        x * _map.TileWidth,
                        y * _map.TileHeight,
                        tileset.TileWidth,
                        tileset.TileHeight
                    );

                    batch.Draw(
                        _textures[tileset.FirstGid],
                        destination,
                        source,
                        Color.White * layer.Opacity,
                        0f,
                        Vector2.Zero,
                        effects,
                        0f
                    );
                }
            }
        }
    }

    public void Dispose()
    {
        foreach (Texture2D texture in _textures.Values)
            texture.Dispose();

        _textures.Clear();
    }
}

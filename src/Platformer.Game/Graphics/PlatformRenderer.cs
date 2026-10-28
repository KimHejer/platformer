using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Platformer.Game.World;
using Platformer.Game.World.Platforms;
using Platformer.Game.World.Tiled;

namespace Platformer.Game.Graphics;

/// <summary>
/// Renders platform instances independently of the level tilemap.
/// </summary>
public sealed class PlatformRenderer : IDisposable
{
    private readonly GraphicsDevice _graphicsDevice;
    private readonly string _contentDirectory;

    private readonly Dictionary<string, Texture2D> _spriteTextures =
        new(StringComparer.Ordinal);

    private readonly TiledMapData? _map;

    private readonly IReadOnlyDictionary<uint, Texture2D>?
        _tilesetTextures;

    public PlatformRenderer(
        GraphicsDevice graphicsDevice,
        string contentDirectory,
        TiledMapData? map = null,
        IReadOnlyDictionary<uint, Texture2D>? tilesetTextures = null)
    {
        _graphicsDevice = graphicsDevice;
        _contentDirectory = Path.GetFullPath(contentDirectory);
        _map = map;
        _tilesetTextures = tilesetTextures;
    }

    public void Draw(
        SpriteBatch batch,
        IEnumerable<PlatformInstance> instances)
    {
        foreach (PlatformInstance instance in instances)
        {
            Rectangle collision = instance.Definition.CollisionBounds;
            Rectangle bounds = instance.Platform.Bounds;

            Rectangle destination = new(
                bounds.X - collision.X,
                bounds.Y - collision.Y,
                instance.Definition.Size.X,
                instance.Definition.Size.Y
            );

            switch (instance.Definition.Visual)
            {
                case SpritePlatformVisual sprite:
                    batch.Draw(
                        GetSprite(sprite.TexturePath),
                        destination,
                        Color.White
                    );
                    break;

                case TiledPlatformVisual tiled:
                    DrawTile(batch, tiled.RawGid, destination);
                    break;

                default:
                    throw new NotSupportedException(
                        $"Unsupported platform visual: " +
                        $"{instance.Definition.Visual.GetType().Name}"
                    );
            }
        }
    }

    private Texture2D GetSprite(string relativePath)
    {
        if (_spriteTextures.TryGetValue(
            relativePath,
            out Texture2D? existing))
        {
            return existing;
        }

        string fullPath = Path.GetFullPath(
            Path.Combine(_contentDirectory, relativePath)
        );

        string relative = Path.GetRelativePath(
            _contentDirectory,
            fullPath
        );

        if (Path.IsPathRooted(relative) ||
            relative == ".." ||
            relative.StartsWith(
                ".." + Path.DirectorySeparatorChar,
                StringComparison.Ordinal))
        {
            throw new InvalidDataException(
                $"Platform texture outside Content: {relativePath}"
            );
        }

        using Stream stream = File.OpenRead(fullPath);

        Texture2D texture = Texture2D.FromStream(
            _graphicsDevice,
            stream
        );

        _spriteTextures.Add(relativePath, texture);

        return texture;
    }

    private void DrawTile(
        SpriteBatch batch,
        uint rawGid,
        Rectangle destination)
    {
        if (_map is null || _tilesetTextures is null)
        {
            throw new InvalidOperationException(
                "Tiled platform visuals require a Tiled map."
            );
        }

        const uint flipH = 0x80000000u;
        const uint flipV = 0x40000000u;
        const uint flipD = 0x20000000u;

        if ((rawGid & flipD) != 0)
        {
            throw new NotSupportedException(
                "Diagonal Tiled flips are not supported."
            );
        }

        uint gid = rawGid & TiledMapData.GidMask;

        TiledMapData.Tileset tileset = _map.GetTileset(gid);

        uint localId = gid - tileset.FirstGid;

        int column = (int)(localId % (uint)tileset.Columns);
        int row = (int)(localId / (uint)tileset.Columns);

        Rectangle source = new(
            tileset.Margin +
                column * (tileset.TileWidth + tileset.Spacing),
            tileset.Margin +
                row * (tileset.TileHeight + tileset.Spacing),
            tileset.TileWidth,
            tileset.TileHeight
        );

        SpriteEffects effects = SpriteEffects.None;

        if ((rawGid & flipH) != 0)
        {
            effects |= SpriteEffects.FlipHorizontally;
        }

        if ((rawGid & flipV) != 0)
        {
            effects |= SpriteEffects.FlipVertically;
        }

        batch.Draw(
            _tilesetTextures[tileset.FirstGid],
            destination,
            source,
            Color.White,
            0f,
            Vector2.Zero,
            effects,
            0f
        );
    }

    public void Dispose()
    {
        foreach (Texture2D texture in _spriteTextures.Values)
        {
            texture.Dispose();
        }

        _spriteTextures.Clear();
    }
}

using System.Globalization;
using System.Xml.Linq;

namespace Platformer.Game.World.Tiled;

public sealed class TiledMapData
{
    public sealed record Tileset(
        uint FirstGid,
        uint TileCount,
        int Columns,
        int TileWidth,
        int TileHeight,
        int Margin,
        int Spacing,
        string ImagePath
    );

    public sealed class TileLayer
    {
        public required string Name { get; init; }
        public required uint[] Tiles { get; init; }
        public required int Width { get; init; }
        public required int Height { get; init; }
        public required bool Collision { get; init; }
        public required bool Foreground { get; init; }
        public required bool Visible { get; init; }
        public required float Opacity { get; init; }
    }

    public sealed class MapObject
    {
        public required int Id { get; init; }
        public required string LayerName { get; init; }
        public required string Name { get; init; }
        public required string Type { get; init; }

        public required float X { get; init; }
        public required float Y { get; init; }
        public required float Width { get; init; }
        public required float Height { get; init; }

        public uint Gid { get; init; }

        public Dictionary<string, string> Properties { get; init; } = new();

        public List<Microsoft.Xna.Framework.Vector2> Polyline { get; init; } 
            = new();

        public string GetString(string name, string fallback = "")
        {
            return Properties.TryGetValue(name, out string? value)
                ? value
                : fallback;
        }

        public float GetFloat(string name, float fallback = 0f)
        {
            return Properties.TryGetValue(name, out string? value)
                ? float.Parse(value, CultureInfo.InvariantCulture)
                : fallback;
        }
    }
    public int Width { get; private init; }
    public int Height { get; private init; }
    public int TileWidth { get; private init; }
    public int TileHeight { get; private init; }

    public List<Tileset> Tilesets { get; } = new();
    public List<TileLayer> Layers { get; } = new();
    public List<MapObject> Objects { get; } = new();

    public Dictionary<uint, string> CollisionTypes { get; } = new();
    public Dictionary<uint, string> SpikeDirections { get; } = new();

    public Dictionary<uint, string> ObjectTypes { get; } = new();
    public Dictionary<uint, string> ObjectTargets { get; } = new();

    public const uint GidMask = 0x0FFFFFFFu;

    public static TiledMapData Load(string path)
    {
        XElement map = XDocument.Load(path).Root
            ?? throw new InvalidDataException("Invalid TMX.");

        if ((string?)map.Attribute("orientation") != "orthogonal" ||
            (string?)map.Attribute("infinite") == "1")
        {
            throw new NotSupportedException(
                "Only finite orthogonal Tiled maps are supported.");
        }

        var result = new TiledMapData
        {
            Width = (int)map.Attribute("width")!,
            Height = (int)map.Attribute("height")!,
            TileWidth = (int)map.Attribute("tilewidth")!,
            TileHeight = (int)map.Attribute("tileheight")!
        };

        string directory = Path.GetDirectoryName(
            Path.GetFullPath(path))!;

        foreach (XElement reference in map.Elements("tileset"))
        {
            uint firstGid = (uint)reference.Attribute("firstgid")!;

            string source = (string?)reference.Attribute("source")
                ?? throw new NotSupportedException(
                    "External TSX tilesets are required.");

            string tsxPath = Path.GetFullPath(
                Path.Combine(directory, source));

            XElement tsx = XDocument.Load(tsxPath).Root
                ?? throw new InvalidDataException("Invalid TSX.");

            string imageSource = (string?)tsx
                .Element("image")?.Attribute("source")
                ?? throw new InvalidDataException(
                    $"Missing tileset image: {source}");

            string imagePath = Path.GetFullPath(
                Path.Combine(
                    Path.GetDirectoryName(tsxPath)!,
                    imageSource));

            result.Tilesets.Add(new Tileset(
                firstGid,
                (uint)tsx.Attribute("tilecount")!,
                (int)tsx.Attribute("columns")!,
                (int)tsx.Attribute("tilewidth")!,
                (int)tsx.Attribute("tileheight")!,
                (int?)tsx.Attribute("margin") ?? 0,
                (int?)tsx.Attribute("spacing") ?? 0,
                imagePath
            ));

            foreach (XElement tile in tsx.Elements("tile"))
            {
                uint localId = (uint)tile.Attribute("id")!;

                XElement? tileProperties = tile.Element("properties");

                string? ReadTileProperty(string name)
                {
                    XElement? entry = tileProperties?.Elements("property")
                        .FirstOrDefault(p =>
                            (string?)p.Attribute("name") == name);

                    return entry == null
                        ? null
                        : (string?)entry.Attribute("value") ?? entry.Value;
                }

                string? objectType = ReadTileProperty("objectType");

                // Support individual properties such as:
                // <property name="jumpRestore" value=""/>
                if (string.IsNullOrWhiteSpace(objectType))
                {
                    objectType = tileProperties?.Elements("property")
                        .Select(p => (string?)p.Attribute("name"))
                        .FirstOrDefault(name =>
                            name is "jumpRestore" or "checkpoint" or "levelTransition");
                }

                if (!string.IsNullOrWhiteSpace(objectType))
                {
                    objectType = objectType.Trim();

                    if (objectType is not (
                        "jumpRestore" or "checkpoint" or "levelTransition"))
                    {
                        throw new InvalidDataException(
                            $"Unknown objectType '{objectType}' in {source}, tile {localId}.");
                    }

                    result.ObjectTypes[firstGid + localId] = objectType;

                    if (objectType == "levelTransition")
                    {
                        string? target = ReadTileProperty("levelTransition");

                        if (!string.IsNullOrWhiteSpace(target))
                        {
                            result.ObjectTargets[firstGid + localId] = target.Trim();
                        }
                    }
                }

                XElement? property = tile
                    .Element("properties")?
                    .Elements("property")
                    .FirstOrDefault(p =>
                        (string?)p.Attribute("name")
                        == "collisionType");

                // Also recognize your existing TSX properties.
                string? type = property == null
                    ? null
                    : (string?)property.Attribute("value")
                      ?? property.Value;

                if (string.IsNullOrWhiteSpace(type))
                {
                    type = tile.Element("properties")?
                        .Elements("property")
                        .Select(p => (string?)p.Attribute("name"))
                        .FirstOrDefault(name =>
                            name is "solid" or "soft"
                                or "brittle" or "spike");
                }

                if (string.IsNullOrWhiteSpace(type))
                    continue;

                type = type.Trim().ToLowerInvariant();

                if (type is not ("solid" or "soft"
                    or "brittle" or "spike"))
                {
                    throw new InvalidDataException(
                        $"Unknown collisionType '{type}' " +
                        $"in {source}, tile {localId}.");
                }

                result.CollisionTypes[firstGid + localId] = type;
                if (type == "spike")
                {
                    string direction = (
                        ReadTileProperty("spike") ??
                        ReadTileProperty("direction") ??
                        "up"
                    ).Trim().ToLowerInvariant();

                    if (string.IsNullOrWhiteSpace(direction))
                        direction = "up";

                    if (direction is not ("up" or "down" or "left" or "right"))
                    {
                        throw new InvalidDataException(
                            $"Invalid spike direction '{direction}' " +
                            $"in {source}, tile {localId}.");
                    }

                    result.SpikeDirections[firstGid + localId] = direction;
                }
            }
        }

        foreach (XElement layer in map.Elements("layer"))
        {
            string name = (string?)layer.Attribute("name") ?? "";

            XElement data = layer.Element("data")
                ?? throw new InvalidDataException(
                    $"Missing data in layer '{name}'.");

            if ((string?)data.Attribute("encoding") != "csv")
                throw new NotSupportedException(
                    $"Layer '{name}' must use CSV encoding.");

            uint[] gids = data.Value.Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries |
                    StringSplitOptions.TrimEntries)
                .Select(s => uint.Parse(
                    s, CultureInfo.InvariantCulture))
                .ToArray();

            int width = (int?)layer.Attribute("width")
                ?? result.Width;
            int height = (int?)layer.Attribute("height")
                ?? result.Height;

            if (width != result.Width ||
                height != result.Height ||
                gids.Length != width * height)
            {
                throw new InvalidDataException(
                    $"Invalid dimensions in layer '{name}'.");
            }

            XElement? props = layer.Element("properties");

            bool HasProperty(string propertyName)
            {
                return props?.Elements("property")
                    .Any(p =>
                        (string?)p.Attribute("name") == propertyName)
                    ?? false;
            }

            result.Layers.Add(new TileLayer
            {
                Name = name,
                Tiles = gids,
                Width = width,
                Height = height,
                Collision = HasProperty("collision"),
                Foreground = HasProperty("foreground") ||
                    name.Equals(
                        "Foreground",
                        StringComparison.OrdinalIgnoreCase),
                Visible = (string?)layer.Attribute("visible") != "0",
                Opacity = float.Parse(
                    (string?)layer.Attribute("opacity") ?? "1",
                    CultureInfo.InvariantCulture)
            });
        }

        foreach (XElement group in map.Elements("objectgroup"))
        {
            foreach (XElement obj in group.Elements("object"))
            {
                var properties = new Dictionary<string, string>();

                foreach (XElement property in
                        obj.Element("properties")?.Elements("property")
                        ?? Enumerable.Empty<XElement>())
                {
                    string name = (string?)property.Attribute("name") ?? "";

                    string value =
                        (string?)property.Attribute("value")
                        ?? property.Value;

                    properties[name] = value;
                }

                result.Objects.Add(new MapObject
                {
                    Id = (int?)obj.Attribute("id") ?? 0,
                    Name = (string?)obj.Attribute("name") ?? "",
                    LayerName = (string?)group.Attribute("name") ?? "",
                    Type =
                        (string?)obj.Attribute("class") is { Length: > 0 } className
                            ? className
                            : (string?)obj.Attribute("type") ?? "",

                    X = (float?)obj.Attribute("x") ?? 0f,
                    Y = (float?)obj.Attribute("y") ?? 0f,
                    Width = (float?)obj.Attribute("width") ?? 0f,
                    Height = (float?)obj.Attribute("height") ?? 0f,

                    Gid = (uint?)obj.Attribute("gid") ?? 0u,

                    Polyline = ParsePolyline(obj),

                    Properties = properties
                });
            }
        }

        return result;
    }

    public Tileset GetTileset(uint gid)
    {
        return Tilesets
            .Where(t => gid >= t.FirstGid &&
                        gid - t.FirstGid < t.TileCount)
            .OrderByDescending(t => t.FirstGid)
            .FirstOrDefault()
            ?? throw new InvalidDataException(
                $"No tileset contains GID {gid}.");
    }

    private static List<Microsoft.Xna.Framework.Vector2> ParsePolyline(
        XElement obj)
    {
        string? points = (string?)obj.Element("polyline")?.Attribute("points");

        if (string.IsNullOrWhiteSpace(points))
            return new();

        return points
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(point =>
            {
                string[] coordinates = point.Split(',');

                if (coordinates.Length != 2)
                    throw new InvalidDataException(
                        $"Invalid polyline point '{point}'.");

                return new Microsoft.Xna.Framework.Vector2(
                    float.Parse(
                        coordinates[0],
                        CultureInfo.InvariantCulture),
                    float.Parse(
                        coordinates[1],
                        CultureInfo.InvariantCulture)
                );
            })
            .ToList();
    }
}

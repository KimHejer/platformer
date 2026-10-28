using System.Text.Json;
using Microsoft.Xna.Framework;

namespace Platformer.Game.World.Platforms;

public static class PlatformDefinitionLoader
{
    private sealed class CatalogData
    {
        public List<DefinitionData> Platforms { get; set; } = new();
    }

    private sealed class DefinitionData
    {
        public string Id { get; set; } = "";
        public string Texture { get; set; } = "";

        public int Width { get; set; }
        public int Height { get; set; }

        public int CollisionX { get; set; }
        public int CollisionY { get; set; }
        public int CollisionWidth { get; set; }
        public int CollisionHeight { get; set; }
    }

    public static IReadOnlyDictionary<string, PlatformDefinition> Load(
        string path)
    {
        var data = JsonSerializer.Deserialize<CatalogData>(
            File.ReadAllText(path),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        ) ?? throw new InvalidDataException(
            $"Invalid platform catalog: {path}");

        var definitions =
            new Dictionary<string, PlatformDefinition>(
                StringComparer.Ordinal
            );

        foreach (DefinitionData entry in data.Platforms)
        {
            if (string.IsNullOrWhiteSpace(entry.Id) ||
                string.IsNullOrWhiteSpace(entry.Texture) ||
                entry.Width <= 0 ||
                entry.Height <= 0 ||
                entry.CollisionWidth <= 0 ||
                entry.CollisionHeight <= 0)
            {
                throw new InvalidDataException(
                    $"Invalid platform definition '{entry.Id}'.");
            }

            var definition = new PlatformDefinition(
                entry.Id,
                new Point(entry.Width, entry.Height),
                new Rectangle(
                    entry.CollisionX,
                    entry.CollisionY,
                    entry.CollisionWidth,
                    entry.CollisionHeight
                ),
                new SpritePlatformVisual(entry.Texture)
            );

            if (!definitions.TryAdd(entry.Id, definition))
            {
                throw new InvalidDataException(
                    $"Duplicate platform definition '{entry.Id}'.");
            }
        }

        return definitions;
    }
}
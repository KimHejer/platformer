using System.Text.Json;

namespace Platformer.Game.World.Platforms.Spawning;

public static class PlatformSpawnPatternLoader
{
    public static PlatformSpawnPattern Load(string path)
    {
        return JsonSerializer.Deserialize<PlatformSpawnPattern>(
            File.ReadAllText(path),
            new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }
        ) ?? throw new InvalidDataException(
            $"Invalid platform spawn pattern: {path}");
    }
}
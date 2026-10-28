using Microsoft.Xna.Framework;

namespace Platformer.Game.World.Platforms;

public sealed class PlatformFactory
{
    private readonly IReadOnlyDictionary<string, PlatformDefinition>
        _definitions;

    public PlatformFactory(
        IReadOnlyDictionary<string, PlatformDefinition> definitions)
    {
        _definitions = definitions;
    }

    public PlatformInstance Create(
        string definitionId,
        Vector2 position,
        Vector2 velocity)
    {
        if (!_definitions.TryGetValue(
            definitionId,
            out PlatformDefinition? definition))
        {
            throw new InvalidDataException(
                $"Unknown platform definition '{definitionId}'.");
        }

        return Create(definition, position, velocity);
    }

    public PlatformInstance Create(
        PlatformDefinition definition,
        Vector2 position,
        Vector2 velocity)
    {
        Rectangle collision = definition.CollisionBounds;

        if (definition.Size.X <= 0 ||
            definition.Size.Y <= 0 ||
            collision.Width <= 0 ||
            collision.Height <= 0 ||
            collision.X < 0 ||
            collision.Y < 0 ||
            collision.Right > definition.Size.X ||
            collision.Bottom > definition.Size.Y)
        {
            throw new InvalidDataException(
                $"Invalid dimensions for platform '{definition.Id}'.");
        }

        var platform = new FloatingPlatform(
            (int)MathF.Round(position.X),
            (int)MathF.Round(position.Y),
            collision.Width,
            collision.Height,
            new Movement.LinearMovement(velocity),
            new Point(collision.X, collision.Y)
        );

        return new PlatformInstance(platform, definition);
    }
}
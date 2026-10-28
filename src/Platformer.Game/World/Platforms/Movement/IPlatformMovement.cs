using Microsoft.Xna.Framework;

namespace Platformer.Game.World.Platforms.Movement;

public interface IPlatformMovement
{
    Vector2 Update(Vector2 currentPosition, GameTime gameTime);

    void Reset();
}
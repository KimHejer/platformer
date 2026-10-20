using System.Xml;
using Microsoft.Xna.Framework;

namespace Platformer.Game.Entities.Enemies;

public class StationaryEnemy : Enemy
{
    private const float RespawnDelay = 0.4f;

    private readonly Vector2 _spawnPosition;
    private float _respawnTimer;
    public StationaryEnemy(
        Vector2 position,
        int width,
        int height,
        int maxHealth,
        int contactDamage
    ) : base(
        position,
        width,
        height,
        maxHealth,
        contactDamage
    )
    {
        _spawnPosition = position;
        Velocity = Vector2.Zero;
    }

    public override void Update(GameTime gameTime)
    {
        if (!IsDead)
        {
            base.Update(gameTime);
            return;
        }

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _respawnTimer += dt;

        if (_respawnTimer >= RespawnDelay)
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        Position = _spawnPosition;
        Velocity = Vector2.Zero;

        Health.Reset();
        _respawnTimer = 0f;
    }
}
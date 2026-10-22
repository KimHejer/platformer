using Microsoft.Xna.Framework;
using Platformer.Game.Hazards;

namespace Platformer.Game.Entities.Enemies;

public class StationaryEnemy : Enemy
{
    public bool RespawnAble { get; }
    private const float RespawnDelay = 0.4f;

    private readonly Vector2 _spawnPosition;
    private float _respawnTimer;
    public StationaryEnemy(
        Vector2 position,
        int width,
        int height,
        int maxHealth,
        int contactDamage,
        bool respawnAble
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
        RespawnAble = respawnAble;
    }

    public override void Update(GameTime gameTime)
    {
        if (!IsDead)
        {
            base.Update(gameTime);
            return;
        }

        if (!RespawnAble)
            return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
        _respawnTimer += dt;

        if (_respawnTimer >= RespawnDelay)
        {
            Reset();
        }
    }

    public override void Reset()
    {
        Position = _spawnPosition;
        Velocity = Vector2.Zero;

        Health.Reset();
        _respawnTimer = 0f;
    }

    public void CheckHazards(
        IReadOnlyList<Spike> spikes,
        int levelWidth,
        int levelHeight
    )
    {
        if (IsDead)
            return;

        foreach (Spike spike in spikes)
        {
            if (spike.Touches(Hitbox))
            {
                Kill();
                return;
            }
        }

        if (Hitbox.Top >= levelHeight ||
            Hitbox.Right <= 0 ||
            Hitbox.Left >= levelWidth)
        {
            Kill();
        }
    }
}
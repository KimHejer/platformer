using Microsoft.Xna.Framework;
using Platformer.Game.Entities;

namespace Platformer.Game.World.Platforms.Spawning;

public sealed class PlatformSpawner
{
    private readonly PlatformFactory _factory;
    private readonly Level _level;
    private readonly PlatformSpawnPattern _pattern;

    private readonly List<PlatformInstance> _spawned = new();

    private float _elapsed;
    private int _nextEvent;
    private bool _finished;

    public float SpeedMultiplier { get; set; } = 1f;

    public PlatformSpawner(
        PlatformFactory factory,
        Level level,
        PlatformSpawnPattern pattern)
    {
        _factory = factory;
        _level = level;
        _pattern = pattern;

        if (pattern.Duration <= 0 ||
            !float.IsFinite(pattern.Duration))
            throw new ArgumentException(
                "Pattern duration must be positive.");

        if (pattern.Events.Any(e =>
            e.Time < 0 ||
            e.Time >= pattern.Duration ||
            !float.IsFinite(e.Time)))
            throw new ArgumentException(
                "Spawn events must be within the pattern duration.");

        pattern.Events = pattern.Events
            .OrderBy(e => e.Time)
            .ToList();
    }

    public void Update(GameTime gameTime, Player player)
    {
        if (_finished)
            return;

        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (dt < 0)
            return;

        float remaining = dt;

        while (remaining > 0f && !_finished)
        {
            float untilEnd = _pattern.Duration - _elapsed;
            float step = MathF.Min(remaining, untilEnd);

            float end = _elapsed + step;

            while (_nextEvent < _pattern.Events.Count &&
                   _pattern.Events[_nextEvent].Time <= end)
            {
                Spawn(_pattern.Events[_nextEvent]);
                _nextEvent++;
            }

            _elapsed = end;
            remaining -= step;

            if (_elapsed >= _pattern.Duration)
            {
                if (_pattern.Loop)
                {
                    _elapsed = 0f;
                    _nextEvent = 0;
                }
                else
                {
                    _finished = true;
                }
            }
        }

        DespawnOutsideLevel(player);
    }

    private void Spawn(PlatformSpawnEvent spawn)
    {
        Vector2 velocity = new(
            spawn.VelocityX * SpeedMultiplier,
            spawn.VelocityY * SpeedMultiplier
        );

        PlatformInstance instance = _factory.Create(
            spawn.PlatformId,
            new Vector2(spawn.X, spawn.Y),
            velocity
        );

        _level.AddPlatform(instance);
        _spawned.Add(instance);
    }

    private void DespawnOutsideLevel(Player player)
    {
        const int margin = 256;

        for (int i = _spawned.Count - 1; i >= 0; i--)
        {
            PlatformInstance instance = _spawned[i];
            Rectangle bounds = instance.Platform.Bounds;

            bool outside =
                bounds.Right < -margin ||
                bounds.Left > _level.Width + margin ||
                bounds.Bottom < -margin ||
                bounds.Top > _level.Height + margin;

            if (!outside)
                continue;

            _level.RemovePlatform(instance, player);
            _spawned.RemoveAt(i);
        }
    }

    public void Reset(Player player)
    {
        foreach (PlatformInstance instance in _spawned)
            _level.RemovePlatform(instance, player);

        _spawned.Clear();

        _elapsed = 0f;
        _nextEvent = 0;
        _finished = false;
        SpeedMultiplier = 1f;
    }
}
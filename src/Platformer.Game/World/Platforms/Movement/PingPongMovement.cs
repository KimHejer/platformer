using Microsoft.Xna.Framework;

namespace Platformer.Game.World.Platforms.Movement;

public sealed class PingPongMovement : IPlatformMovement
{
    private readonly Vector2 _start;
    private readonly Vector2 _end;
    private readonly float _speed;

    private float _distance;
    private float _direction = 1f;

    public PingPongMovement(
        Vector2 start,
        Vector2 end,
        float speed)
    {
        if (speed <= 0 || !float.IsFinite(speed))
            throw new ArgumentOutOfRangeException(nameof(speed));

        if (start == end)
            throw new ArgumentException(
                "Ping-pong endpoints must be different.");

        _start = start;
        _end = end;
        _speed = speed;
    }

    public Vector2 Update(Vector2 currentPosition, GameTime gameTime)
    {
        float length = Vector2.Distance(_start, _end);
        float remaining =
            _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

        remaining %= 2f * length;

        // Reflect at endpoints, including when one update crosses
        // an endpoint more than once.
        while (remaining > 0f)
        {
            float available = _direction > 0
                ? length - _distance
                : _distance;

            if (remaining < available)
            {
                _distance += remaining * _direction;
                break;
            }

            _distance = _direction > 0 ? length : 0f;
            remaining -= available;
            _direction = -_direction;
        }

        return Vector2.Lerp(
            _start,
            _end,
            _distance / length
        );
    }

    public void Reset()
    {
        _distance = 0f;
        _direction = 1f;
    }
}
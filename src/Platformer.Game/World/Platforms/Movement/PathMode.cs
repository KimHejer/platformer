using Microsoft.Xna.Framework;

namespace Platformer.Game.World.Platforms.Movement;

public sealed class PathMovement : IPlatformMovement
{
    private readonly Vector2[] _points;
    private readonly float _speed;
    private readonly PathMode _mode;

    private int _targetIndex = 1;
    private int _direction = 1;
    private Vector2 _position;
    private bool _finished;

    public PathMovement(
        IReadOnlyList<Vector2> points,
        float speed,
        PathMode mode)
    {
        if (points.Count < 2)
            throw new ArgumentException(
                "A movement path requires at least two points.");

        if (speed <= 0 || !float.IsFinite(speed))
            throw new ArgumentOutOfRangeException(nameof(speed));

        _points = points.ToArray();

        for (int i = 0; i < _points.Length; i++)
        {
            if (!float.IsFinite(_points[i].X) ||
                !float.IsFinite(_points[i].Y))
                throw new ArgumentException("Invalid path coordinate.");

            if (i > 0 && _points[i] == _points[i - 1])
                throw new ArgumentException(
                    "Consecutive path points cannot be identical.");
        }

        _speed = speed;
        _mode = mode;
        _position = _points[0];
    }

    public Vector2 Update(Vector2 currentPosition, GameTime gameTime)
    {
        if (_finished)
            return _position;

        float remaining =
            _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

        while (remaining > 0f && !_finished)
        {
            Vector2 target = _points[_targetIndex];
            Vector2 difference = target - _position;
            float distance = difference.Length();

            if (distance > remaining)
            {
                _position += difference / distance * remaining;
                break;
            }

            _position = target;
            remaining -= distance;
            AdvanceTarget();
        }

        return _position;
    }

    private void AdvanceTarget()
    {
        int next = _targetIndex + _direction;

        if (next >= 0 && next < _points.Length)
        {
            _targetIndex = next;
            return;
        }

        switch (_mode)
        {
            case PathMode.Once:
                _finished = true;
                break;

            case PathMode.Loop:
                // Loop includes travel from the last point to
                // the first point, rather than teleporting.
                _targetIndex = 0;
                break;

            case PathMode.PingPong:
                _direction = -_direction;
                _targetIndex += _direction;
                break;
        }
    }

    public void Reset()
    {
        _targetIndex = 1;
        _direction = 1;
        _position = _points[0];
        _finished = false;
    }
}
using Microsoft.Xna.Framework;

namespace Platformer.Game.Checkpoints;

public readonly record struct PlayerSnapshot(
    Vector2 Position,
    Vector2 Velocity,
    int CurrentHealth,
    bool IsGrounded,
    bool WantsToDrop,
    int JumpsUsed,
    bool WasJumpHeld,
    float InvulnerabilityTimer
);
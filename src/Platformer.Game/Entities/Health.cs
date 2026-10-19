using System;

namespace Platformer.Game.Entities;

public class Health
{
    public int Max { get; }
    public int Current { get; private set; }

    public bool IsDead => Current <= 0;

    public Health(int max)
    {
        if (max <= 0)
            throw new ArgumentOutOfRangeException(nameof(max));
        
        Max = max;
        Current = max;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || IsDead)
            return;

        Current = Math.Max(0, Current - amount);
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || IsDead)
            return;
        
        Current = Math.Min(Max, Current + amount);
    }

    public void Kill()
    {
        Current = 0;
    }

    public void Reset()
    {
        Current = Max;
    }
}
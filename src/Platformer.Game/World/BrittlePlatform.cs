using Microsoft.Xna.Framework;

namespace Platformer.Game.World;

public class BrittlePlatform : IPlatform
{
    public Rectangle Bounds { get; }
    public bool IsOneWay => false;
    public bool IsBreaking { get; private set;}
    public bool IsActive { get; private set; } = true;

    private const float BreakDelay = 0.12f;
    private float _breakTimer;

    public BrittlePlatform(int x, int y, int width, int height)
    {
        Bounds = new Rectangle(x, y, width, height);
    }

    public void StartBreaking()
    {
        if (!IsActive || IsBreaking)
            return;
        
        IsBreaking = true;
        _breakTimer = 0f;
    }

    public void Update(GameTime gameTime)
    {
        if (!IsActive || !IsBreaking)
            return;

            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            _breakTimer += dt;

            if (_breakTimer >= BreakDelay)
        {
            IsActive = false;
            IsBreaking = false;
        }
    }

    public void ReActivate()
    {
        IsActive = true;
        IsBreaking = false;
        _breakTimer = 0f;
    }
}
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Platformer.Game.Graphics;

public class SpriteAnimator
{
    private readonly Dictionary<string, Texture2D[]> _animations = new();

    private string _currentAnimation = "";
    private int _currentFrame = 0;
    private float _elapsedTime = 0f;

    public float FramesPerSecond { get; set; } = 10f;
    
    public Texture2D CurrentFrame => _animations[_currentAnimation][_currentFrame];

    public void AddAnimation(string name, Texture2D[] frames)
    {
        if (frames.Length == 0)
            throw new ArgumentException("Animation must have frames.");

        _animations.Add(name, frames);
    }

    public void Play(string name)
    {
        if (_currentAnimation == name)
            return;
        
        if (!_animations.ContainsKey(name))
            throw new ArgumentException($"Unknown animation: {name}");
        
        _currentAnimation = name;
        _currentFrame = 0;
        _elapsedTime = 0f;
    }

    public void Update(GameTime gameTime)
    {
        _elapsedTime += (float)gameTime.ElapsedGameTime.TotalSeconds;

        float frameDuration = 1f / FramesPerSecond;

        while (_elapsedTime >= frameDuration)
        {
            _elapsedTime -= frameDuration;

            _currentFrame = (_currentFrame + 1) % _animations[_currentAnimation].Length;
        }
    }
}
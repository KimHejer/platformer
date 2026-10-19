using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Platformer.Game.Entities;
using Platformer.Game.World;
using Platformer.Game.Physics;
using Platformer.Game.Graphics;
using Platformer.Game.Hazards;
using System;

namespace Platformer.Game;

public class Game1 : Microsoft.Xna.Framework.Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _pixel;
    private Texture2D _spikeTexture;

    private Player _player;
    private List<IPlatform> _platforms;

    private SpriteAnimator _kixAnimator;
    private bool _facingRight = true;

    private List<Spike> _spikes;

    private Vector2 _respawnPosition = new(100, 100);
    private float _deathTimer = 0f;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 1600;
        _graphics.PreferredBackBufferHeight = 900;

        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
            _player = new Player(new Vector2(100, 100));

            _platforms = new List<IPlatform>
            {
                new SolidPlatform(0, 850, 1600, 50),

                new SolidPlatform(150, 650, 250, 30),
                new SolidPlatform(700, 450, 250, 30),

                new SoftPlatform(450, 700, 200, 16),
                new SoftPlatform(1050, 550, 200, 16),
                new SoftPlatform(1150, 300, 200, 16)
            };

            _spikes = new List<Spike>
            {
                new Spike(
                    new Vector2(400, 850),
                    new Vector2(416, 818),
                    new Vector2(432, 850)
                ),

                new Spike(
                    new Vector2(432, 850),
                    new Vector2(448, 818),
                    new Vector2(464, 850)
                )
            };

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

        _spikeTexture = CreateSpikeTexture(32, 32);

        _kixAnimator = new SpriteAnimator();

        _kixAnimator.AddAnimation("Idle", LoadFrames("Idle", "idle"));
        _kixAnimator.AddAnimation("Run", LoadFrames("Run", "run"));
        _kixAnimator.AddAnimation("RunShoot", LoadFrames("Run_Shoot", "run_shoot"));
        _kixAnimator.AddAnimation("Jump", LoadFrames("Jump", "jump"));
        _kixAnimator.AddAnimation("Fall", LoadFrames("Fall", "fall"));
        _kixAnimator.AddAnimation("Land", LoadFrames("Land", "land"));
        _kixAnimator.AddAnimation("Shoot", LoadFrames("Shoot", "shoot"));

        _kixAnimator.Play("Idle");

    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState keyboard = Keyboard.GetState();

        if (keyboard.IsKeyDown(Keys.Escape))
            Exit();

        if (keyboard.IsKeyDown(Keys.R))
            _player.Respawn(_respawnPosition);

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_player.IsDead)
        {
            _deathTimer += deltaTime;

            if (_deathTimer >= 0.4f)
            {
                _player.Respawn(_respawnPosition);
                _deathTimer = 0f;
            }

            base.Update(gameTime);
            return;
        }
            
        _player.Update(gameTime, keyboard);

        CollisionResolver.Move(_player, _platforms, deltaTime);

        bool shooting = keyboard.IsKeyDown(Keys.Space);

        if (_player.Velocity.X > 0f)
            _facingRight = true;
        else if (_player.Velocity.X < 0f)
            _facingRight = false;

        if (!_player.IsGrounded)
        {
            if (_player.Velocity.Y < 0)
                _kixAnimator.Play("Jump");
            else
                _kixAnimator.Play("Fall");
        }
        else if (MathF.Abs(_player.Velocity.X) > 0.1f)
        {
            _kixAnimator.Play(shooting ? "RunShoot" : "Run");
        }
        else
        {
            _kixAnimator.Play(shooting ? "Shoot" : "Idle");
        }

        _kixAnimator.Update(gameTime);

        foreach (Spike spike in _spikes)
        {
            if (spike.Touches(_player.Hitbox))
            {
                _player.Kill();
                break;
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

        foreach (IPlatform platform in _platforms)
        {
            Color color = platform.IsOneWay
                ? Color.LightGreen
                : Color.DarkGray;

            _spriteBatch.Draw(
                _pixel,
                platform.Bounds,
                color
            );
        }

        foreach(Spike spike in _spikes)
        {
            float left = MathF.Min( spike.Shape.A.X, MathF.Min(spike.Shape.B.X, spike.Shape.C.X));

            float top = MathF.Min(spike.Shape.A.Y, MathF.Min(spike.Shape.B.Y, spike.Shape.C.Y));

            _spriteBatch.Draw(_spikeTexture, new Rectangle((int)left, (int)top, 32, 32), Color.White);
        }

        Texture2D frame = _kixAnimator.CurrentFrame;

        const int spriteWidth = 192;
        const int spriteHeight = 96;

        const int bodyAnchorX = 62;
        const int leftOffset = 7;
        const int rightOffset = -12;

        int playerCenterX = (int)_player.Position.X + Player.Width / 2 + leftOffset;

        int spriteX = _facingRight
            ? playerCenterX - bodyAnchorX
            : playerCenterX - (spriteWidth - bodyAnchorX) + rightOffset;

        Rectangle destination = new Rectangle(
            spriteX,
            (int)_player.Position.Y + Player.Height - 90,
            spriteWidth,
            spriteHeight
        );

        SpriteEffects effects = _facingRight
            ? SpriteEffects.None
            : SpriteEffects.FlipHorizontally;

        _spriteBatch.Draw(
            frame,
            destination,
            null,
            Color.White,
            0f,
            Vector2.Zero,
            effects,
            0f
        );

        // Only for testing, shows the player hitbox
        _spriteBatch.Draw(
            _pixel,
            _player.Hitbox,
            Color.Red * 0.35f
        );

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private Texture2D[] LoadFrames(string folder, string prefix)
    {
        Texture2D[] frames = new Texture2D[4];

        for (int i = 0; i < 4; i++)
        {
            string path = Path.Combine(
                "Content",
                "Sprites",
                "Kix",
                folder,
                $"{prefix}_{i + 1:00}.png"
            );

            using var stream = TitleContainer.OpenStream(path);

            frames[i] = Texture2D.FromStream(GraphicsDevice, stream);
        }

        return frames;
    }

    private Texture2D CreateSpikeTexture(int width, int height)
    {
        Texture2D texture = new Texture2D(GraphicsDevice, width, height);

        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            float halfWidt = (y + 0.5f) / height * (width / 2f);
            float centerX = width / 2f;

            for (int x = 0; x < width; x++)
            {
                bool inside = MathF.Abs(x + 0.5f - centerX) <= halfWidt;

                pixels[y * width + x] = inside ? Color.Black : Color.Transparent;
            }
        }

        texture.SetData(pixels);

        return texture;
    }
}

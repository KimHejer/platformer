using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Platformer.Game.Entities;
using Platformer.Game.World;
using Platformer.Game.Physics;
using Platformer.Game.Graphics;
using Platformer.Game.Hazards;
using Platformer.Game.Combat;
using Platformer.Game.Combat.Weapons;
using Platformer.Game.Combat.Projectiles;
using Platformer.Game.Entities.Enemies;
using Platformer.Game.Checkpoints;

namespace Platformer.Game;

public class Game1 : Microsoft.Xna.Framework.Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _pixel;
    private Texture2D _spikeTexture;
    private Texture2D _checkpointTexture;

    private Player _player;
    private Level _level;
    private Camera2D _camera;

    private CheckpointManager _checkpointManager;

    private SpriteAnimator _kixAnimator;
    private bool _facingRight = true;

    private float RespawnDelay = 0.4f;
    private float _deathTimer = 0f;

    private KeyboardState _previousKeyboard;

    private Rifle _rifle;
    private ProjectileManager _projectileManager;

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
        _level = LevelLoader.Load(
            Path.Combine(
                AppContext.BaseDirectory,
                "Content",
                "Levels",
                "level_00.json"
            )
        );

        _player = new Player(_level.SpawnPosition);
        _checkpointManager = new CheckpointManager(_player);

        _camera = new Camera2D(
            GraphicsDevice.Viewport.Width,
            GraphicsDevice.Viewport.Height
        );

        _rifle = new Rifle();
        _projectileManager = new ProjectileManager();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
        _checkpointTexture = CreateCircleTexture(256);

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
        {
            Exit();
            return;
        }

        bool rPressed = keyboard.IsKeyDown(Keys.R) && !_previousKeyboard.IsKeyDown(Keys.R);

        _previousKeyboard = keyboard;

        if (rPressed)
        {
            if (!_player.IsDead && TrySaveCheckpoint())
            {
                // Nothing, only saving checkpoint
            }
            else
            {
                Respawn();
                base.Update(gameTime);
                return;
            }
        }

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (_player.IsDead)
        {
            _deathTimer += deltaTime;

            if (_deathTimer >= RespawnDelay)
            {
                Respawn();
            }

            base.Update(gameTime);
            return;
        }
            
        _player.Update(gameTime, keyboard);

        CollisionResolver.Move(_player, _level.Platforms, deltaTime);

        if (_player.Position.Y > _level.Height)
        {
            _player.Kill();
            base.Update(gameTime);
            return;
        }

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

        foreach (Spike spike in _level.Spikes)
        {
            if (spike.Touches(_player.Hitbox))
            {
                _player.Kill();
                break;
            }
        }

        if (_player.IsDead)
        {
            base.Update(gameTime);
            return;
        }

        _rifle.Update(gameTime);

        Vector2 direction = _facingRight ? Vector2.UnitX : -Vector2.UnitX;


        if (shooting && !_player.IsDead)
        {
            Vector2 bulletOrigin = new(
                _player.Position.X + Player.Width / 2,
                _player.Position.Y + Player.Height / 2
            );

            Projectile? bullet = _rifle.TryFire(
                bulletOrigin,
                direction
            );

            if (bullet != null)
                _projectileManager.Add(bullet);
        }

        _projectileManager.Update(
            gameTime, 
            _level.Platforms, 
            _level.Spikes, 
            _level.Enemies);

        foreach(Enemy enemy in _level.Enemies)
        {
            enemy.Update(gameTime);

            if (enemy.IsDead)
                continue;
            
            if (enemy.Hitbox.Intersects(_player.Hitbox))
            {
                _player.TakeDamage(enemy.ContactDamage);
            }
        }

        _camera.Update(
            _player.Hitbox,
            _level.Width,
            _level.Height
        );

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin(
            samplerState: SamplerState.PointClamp,
            transformMatrix: _camera.Transform);

        foreach (CheckpointZone checkpoint in _level.Checkpoints)
        {
            DrawCheckpoint(checkpoint);
        }

        foreach (IPlatform platform in _level.Platforms)
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

        foreach(Spike spike in _level.Spikes)
        {
            float left = MathF.Min( spike.Shape.A.X, MathF.Min(spike.Shape.B.X, spike.Shape.C.X));

            float top = MathF.Min(spike.Shape.A.Y, MathF.Min(spike.Shape.B.Y, spike.Shape.C.Y));

            _spriteBatch.Draw(_spikeTexture, new Rectangle((int)left, (int)top, 32, 32), Color.White);
        }

        foreach (Enemy enemy in _level.Enemies)
        {
            if (enemy.IsDead)
                continue;
            
            _spriteBatch.Draw(
                _pixel,
                enemy.Hitbox,
                Color.IndianRed
            );

            HealthBarRenderer.Draw(
                _spriteBatch,
                _pixel,
                enemy.Hitbox,
                enemy.Health.Current,
                enemy.Health.Max
            );
        }

        _projectileManager.Draw(_spriteBatch, _pixel);

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

        if (!_player.IsDead)
        {
            HealthBarRenderer.Draw(
                _spriteBatch,
                _pixel,
                _player.Hitbox,
                _player.Health.Current,
                _player.Health.Max
            );
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void Respawn()
    {
        _checkpointManager.Restore(_player);

        _projectileManager.Clear();
        _rifle.Reset();

        _deathTimer = 0f;
        _facingRight = true;

        _kixAnimator.Play("Idle");

        _camera.Reset();
        _camera.Update(
            _player.Hitbox,
            _level.Width,
            _level.Height
        );
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

    private bool TrySaveCheckpoint()
    {
        if (_player.IsDead)
            return false;
        
        foreach (CheckpointZone zone in _level.Checkpoints)
        {
            if (zone.Contains(_player.Hitbox))
            {
                _checkpointManager.Save(_player);
                return true;
            }
        }

        return false;
    }

    private Texture2D CreateCircleTexture(int diameter)
    {
        Texture2D texture = new(
            GraphicsDevice,
            diameter, 
            diameter
        );

        Color[] pixels = new Color[diameter * diameter];

        float radius = diameter / 2f;
        Vector2 center = new(radius, radius);

        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                Vector2 point = new(x + 0.5f, y + 0.5f);

                float distance = Vector2.Distance(point, center);

                pixels[y * diameter + x] = 
                    distance <= radius
                        ? Color.White
                        : Color.Transparent;
            }
        }

        texture.SetData(pixels);

        return texture;
    }

    private void DrawCheckpoint(CheckpointZone zone)
    {
        float diameter = zone.Radius * 2f;

        Rectangle destination = new(
            (int)(zone.Center.X - zone.Radius),
            (int)(zone.Center.Y - zone.Radius),
            (int)diameter,
            (int)diameter
        );

        Color color = Color.Cyan * 0.12f;

        _spriteBatch.Draw(
            _checkpointTexture,
            destination,
            color
        );
    }
}

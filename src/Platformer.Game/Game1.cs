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
using Platformer.Game.World.Transitions;
using Platformer.Game.Gameplay.Interactables;
using Platformer.Game.World.Tiled;

namespace Platformer.Game;

public class Game1 : Microsoft.Xna.Framework.Game
{
    // Graphics
    private readonly GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch = null!;
    private Texture2D _pixel = null!;
    private Texture2D _spikeTexture = null!;
    private Texture2D _checkpointTexture = null!;

    // World
    private Level _level = null!;
    private Camera2D _camera = null!;
    private TiledMapRenderer? _tiledRenderer;

    // Player
    private Player _player = null!;
    private CheckpointManager _checkpointManager = null!;
    private bool _facingRight = true;

    // Animations
    private SpriteAnimator _kixAnimator = null!;

    private readonly Dictionary<Enemy, SpriteAnimator> _enemyAnimators = new();
    private readonly Dictionary<string, Texture2D[]> _animationCache = new();

    // Combat
    private Rifle _rifle = null!;
    private ProjectileManager _projectileManager = null!;

    // Death and respawning
    private const float RespawnDelay = 0.4f;
    private float _deathTimer;

    // Input
    private KeyboardState _previousKeyboard;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreferredBackBufferWidth = 2400;
        _graphics.PreferredBackBufferHeight = 1200;

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
                "level_01.json"
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

        _kixAnimator.AddAnimation("Idle", LoadFrames("Kix", "Idle", "idle"));
        _kixAnimator.AddAnimation("Run", LoadFrames("Kix", "Run", "run"));
        _kixAnimator.AddAnimation("RunShoot", LoadFrames("Kix", "Run_Shoot", "run_shoot"));
        _kixAnimator.AddAnimation("Jump", LoadFrames("Kix", "Jump", "jump"));
        _kixAnimator.AddAnimation("Fall", LoadFrames("Kix", "Fall", "fall"));
        _kixAnimator.AddAnimation("Land", LoadFrames("Kix", "Land", "land"));
        _kixAnimator.AddAnimation("Shoot", LoadFrames("Kix", "Shoot", "shoot"));
        _kixAnimator.AddAnimation("FallShoot", LoadFrames("Kix", "Fall_Shoot", "fall_shoot"));
        _kixAnimator.AddAnimation("Death", LoadFrames("Kix", "Death", "death"));

        _kixAnimator.Play("Idle");
        InitializeEnemyAnimations();
        LoadTiledRenderer();

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
            _kixAnimator.Play("Death");
            _kixAnimator.Update(gameTime);

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

        if (!_player.IsDead && TryLevelTransition())
        {
            base.Update(gameTime);
            return;
        }

        foreach (JumpRestore restore in _level.JumpRestores)
        {
            restore.Update(gameTime);

            if (_player.IsDead || !restore.IsActive)
                continue;
            
            if (restore.Touches(_player.Hitbox))
            {
                if (_player.RestoreJump())
                {
                    restore.Activate();
                }
            }
        }

        foreach (IPlatform platform in _level.Platforms)
        {
            if (platform is BrittlePlatform brittle)
            {
                brittle.Update(gameTime);
            }
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
                _kixAnimator.Play(shooting ? "FallShoot" : "Fall");
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
            SpriteAnimator animator = _enemyAnimators[enemy];

            if (enemy.IsDead) {
                animator.Play("Death");
            }
            else if (enemy.Velocity.Y > 0.1f)
            {
                animator.Play("Fall");
            }
            else
            {
                animator.Play("Idle");
            }

            animator.Update(gameTime);
            enemy.Update(gameTime);

            if (enemy.IsDead)
                continue;


            CollisionResolver.Move(
                enemy,
                _level.Platforms,
                deltaTime
            );

            if (enemy is StationaryEnemy stationary)
            {
                stationary.CheckHazards(
                    _level.Spikes,
                    _level.Width,
                    _level.Height
                );
            }

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
        
        _tiledRenderer?.Draw(_spriteBatch, foreground: false);

        foreach (CheckpointZone checkpoint in _level.Checkpoints)
        {
            if (_level.TiledCheckpoints.Contains(checkpoint))
                continue;

            DrawCheckpoint(checkpoint);
        }

        foreach (LevelTransition transition in _level.LevelTransitions)
        {
            if (_level.TiledTransitions.Contains(transition))
                continue;

            _spriteBatch.Draw(
                _pixel,
                transition.Bounds,
                Color.MediumPurple
            );
        }

        foreach (IPlatform platform in _level.Platforms)
        {
            if (platform is BrittlePlatform brittle && !brittle.IsActive)
                continue;

            if (_level.TiledPlatforms.Contains(platform))
                continue;

            Color color = platform switch
            {
                BrittlePlatform { IsBreaking: true } => Color.OrangeRed,
                BrittlePlatform => Color.Orange,
                SoftPlatform => Color.LightBlue,
                _ => Color.Gray
            };

            _spriteBatch.Draw(
                _pixel,
                platform.Bounds,
                color
            );
        }

        foreach (JumpRestore restore in _level.JumpRestores)
        {
            if (!restore.IsActive ||
                _level.TiledJumpRestores.Contains(restore))
                continue;
            
            float diameter = restore.Radius * 2f;

            Rectangle bounds = new(
                (int)(restore.Position.X - restore.Radius),
                (int)(restore.Position.Y - restore.Radius),
                (int)diameter,
                (int)diameter
            );

            _spriteBatch.Draw(
                _checkpointTexture,
                bounds,
                Color.LimeGreen * 0.7f
            );
        }

        foreach (Spike spike in _level.Spikes)
        {
            if (_level.TiledSpikes.Contains(spike))
                continue;

            float left = MathF.Min(
                spike.Shape.A.X,
                MathF.Min(spike.Shape.B.X, spike.Shape.C.X));

            float top = MathF.Min(
                spike.Shape.A.Y,
                MathF.Min(spike.Shape.B.Y, spike.Shape.C.Y));

            _spriteBatch.Draw(
                _spikeTexture,
                new Rectangle((int)left, (int)top, 32, 32),
                Color.White);
        }

        foreach (Enemy enemy in _level.Enemies)
        {
            if (enemy.IsDead)
                continue;
            
            SpriteAnimator animator = _enemyAnimators[enemy];

            Texture2D enemyFrame = animator.CurrentFrame;

            const int bearSpriteWidth = 48;
            const int bearSpriteHeight = 48;

            Rectangle enemyDestination = new(
                enemy.Hitbox.Center.X - bearSpriteWidth / 2,
                enemy.Hitbox.Bottom - bearSpriteHeight,
                bearSpriteWidth,
                bearSpriteHeight
            );

            _spriteBatch.Draw(
                enemyFrame,
                enemyDestination,
                Color.White
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

        //Only for testing, shows the player hitbox
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

        _tiledRenderer?.Draw(_spriteBatch, foreground: true);

        _spriteBatch.End();

        base.Draw(gameTime);
    }

    private void Respawn()
    {
        _checkpointManager.Restore(_player);

        foreach (Enemy enemy in _level.Enemies)
        {
            enemy.Reset();
        }

        foreach(IPlatform platform in _level.Platforms)
        {
            if (platform is BrittlePlatform brittle)
            {
                brittle.ReActivate();
            }
        }

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

    private Texture2D[] LoadFrames(
        string character, 
        string folder, 
        string prefix)
    {
        Texture2D[] frames = new Texture2D[12];

        for (int i = 0; i < frames.Length; i++)
        {
            string path = Path.Combine(
                "Content",
                "Sprites",
                character,
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

    private void LoadLevel(string filename)
    {
        string path = Path.Combine(
            AppContext.BaseDirectory,
            "Content",
            "Levels",
            filename
        );

        _level = LevelLoader.Load(path);

        LoadTiledRenderer();

        InitializeEnemyAnimations();

        _player.Respawn(_level.SpawnPosition);

        _checkpointManager = new CheckpointManager(_player);

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

    private bool TryLevelTransition()
    {
        foreach (LevelTransition transition in _level.LevelTransitions)
        {
            if (transition.IsTriggered(_player.Hitbox))
            {
                LoadLevel(transition.TargetLevel);
                return true;
            }
        }

        return false;
    }

    private Texture2D[] GetAnimationFrames(
        string character,
        string folder,
        string prefix
    )
    {
        string key = $"{character}/{folder}";

        if (!_animationCache.TryGetValue(key, out var frames))
        {
            frames = LoadFrames(character, folder, prefix);
            _animationCache.Add(key, frames);
        }
        return frames;
    }

    private void InitializeEnemyAnimations()
    {
        _enemyAnimators.Clear();

        foreach (Enemy enemy in _level.Enemies)
        {
            if (string.IsNullOrWhiteSpace(enemy.SpriteName))
                throw new InvalidOperationException(
                    "enemy must specify a sprite."
                );
            
            SpriteAnimator animator = new();

            animator.AddAnimation(
                "Idle",
                GetAnimationFrames(enemy.SpriteName, "Idle", "idle")
            );

            animator.AddAnimation(
                "Fall",
                GetAnimationFrames(enemy.SpriteName, "Fall", "fall")
            );

            animator.AddAnimation(
                "Death",
                GetAnimationFrames(enemy.SpriteName, "Death", "death")
            );

            animator.Play("Idle");

            _enemyAnimators.Add(enemy, animator);
        }
    }

    private void LoadTiledRenderer()
    {
        _tiledRenderer?.Dispose();
        _tiledRenderer = null;

        if (string.IsNullOrWhiteSpace(_level.TileMapPath))
            return;

        TiledMapData map = TiledMapData.Load(
            _level.TileMapPath);

        _tiledRenderer = new TiledMapRenderer(
            GraphicsDevice,
            map,
            _level
        );
    }

    protected override void UnloadContent()
    {
        _tiledRenderer?.Dispose();
        _tiledRenderer = null;

        base.UnloadContent();
    }
}

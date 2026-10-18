using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Platformer.Game.Entities;
using Platformer.Game.World;
using Platformer.Game.Physics;

namespace Platformer.Game;

public class Game1 : Microsoft.Xna.Framework.Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;
    private Texture2D _pixel;

    private Player _player;
    private List<IPlatform> _platforms;

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

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });

    }

    protected override void Update(GameTime gameTime)
    {
        KeyboardState keyboard = Keyboard.GetState();

        if (keyboard.IsKeyDown(Keys.Escape))
            Exit();

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
        _player.Update(gameTime, keyboard);

        CollisionResolver.Move(_player, _platforms, deltaTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

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

        _spriteBatch.Draw(
            _pixel,
            _player.Hitbox,
            Color.Red
        );

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}

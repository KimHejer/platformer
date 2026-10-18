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
    private List<Platform> _platforms;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
            _player = new Player(new Vector2(100, 100));

            _platforms = new List<Platform>
            {
                new Platform(0, 400, 800, 50),
                new Platform(200, 300, 150, 20),
                new Platform(450, 230, 150, 20)
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
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

            KeyboardState keyboard = Keyboard.GetState();

            if (keyboard.IsKeyDown(Keys.Escape))
                Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            
            _player.Update(gameTime, keyboard);

            COllisionResolver.Move(_player, _platforms, deltaTime);

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        foreach (Platform platform in _platforms)
        {
            _spriteBatch.Draw(
                _pixel,
                platform.Bounds,
                Color.DarkGray
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

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using TiledLib;

namespace GravWalker
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class GravWalker : Microsoft.Xna.Framework.Game
    {
        public static GravWalker Instance;
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        ScreenManager screenManager;

        public GravWalker()
        {
            graphics = new GraphicsDeviceManager(this);

            IsMouseVisible = false;

#if WINDOWS_PHONE || WINRT
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 768;
            graphics.IsFullScreen = true;
#endif
#if WINRT
            Windows.UI.Input.PointerVisualizationSettings.GetForCurrentView().IsContactFeedbackEnabled = false;
            Windows.UI.Input.PointerVisualizationSettings.GetForCurrentView().IsBarrelButtonFeedbackEnabled = false;
#endif
#if WINDOWS || LINUX
            IsFixedTimeStep = true;
            //TargetElapsedTime = TimeSpan.FromSeconds(1 / 60);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            graphics.IsFullScreen = false;
            
#endif
            Content.RootDirectory = "GravWalker.Content";

#if WINDOWS_PHONE
            screenManager = new ScreenManager(this, true);
#else
            screenManager = new ScreenManager(this, false);
#endif
            Components.Add(screenManager);
        }

        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            //graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            //graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            //graphics.ApplyChanges();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            graphics.ApplyChanges();

            Instance = this;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

         

            // TODO: use this.Content to load your game content here
            screenManager.AddScreen(new GameplayScreen(), null);
            screenManager.AddScreen(new MainMenuScreen(), null);
            //screenManager.AddScreen(new BackgroundScreen(), null);
            //screenManager.AddScreen(new MainMenuScreen(), null);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
              //  this.Exit();

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            //spriteBatch.Begin();
            //testmap.DrawLayer(spriteBatch, "FG", gameCamera);
            //spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}

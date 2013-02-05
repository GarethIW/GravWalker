#region File Description
//-----------------------------------------------------------------------------
// GameplayScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Threading;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using TiledLib;
#endregion

namespace GravWalker
{
    /// <summary>
    /// This screen implements the actual game logic. It is just a
    /// placeholder to get the idea across: you'll probably want to
    /// put some more interesting gameplay in here!
    /// </summary>
    public class GameplayScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        SpriteFont gameFont;

        Map gameMap;
        Camera gameCamera;

        SpawnerController gameSpawnerController;
        EnemyController gameEnemyController;
        ProjectileManager gameProjectileManager;
        ParticleController gameParticleController;
        WaterController gameWaterController;

        Hero gameHero;

        ParallaxManager parallaxManager;
        //Water gameWater;

        RenderTarget2D gameRenderTarget;

        Vector2 mousePos;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameplayScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            IsStubbourn = true;

            //EnabledGestures = Microsoft.Xna.Framework.Input.Touch.GestureType.FreeDrag;
        }


        /// <summary>
        /// Load graphics content for the game.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "GravWalker.Content");

            //+ (int)(ScreenManager.GraphicsDevice.Viewport.Width/4)
            //gameRenderTarget = new RenderTarget2D(ScreenManager.GraphicsDevice, ScreenManager.GraphicsDevice.Viewport.Width + (int)(ScreenManager.GraphicsDevice.Viewport.Width / 1.2), ScreenManager.GraphicsDevice.Viewport.Height + (int)(ScreenManager.GraphicsDevice.Viewport.Width / 1.2));

            gameFont = content.Load<SpriteFont>("menufont");
          
            gameMap = content.Load<Map>("maps/testmap2");
            GameManager.Map = gameMap;

            gameHero = new Hero();
            gameHero.Initialize();
            gameHero.LoadContent(content);
            GameManager.Hero = gameHero;

            gameSpawnerController = new SpawnerController(gameMap);
            gameEnemyController = new EnemyController();
            gameEnemyController.LoadContent(content, gameSpawnerController.RequiredTypes);

            gameCamera = new Camera(ScreenManager.GraphicsDevice.Viewport, gameMap);
            gameCamera.ClampRect = new Rectangle(0, 5 * gameMap.TileHeight, gameMap.Width * gameMap.TileWidth, gameMap.Height * gameMap.TileHeight);
            //gameCamera.Position = gameHero.Position - (new Vector2(ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height) / 2);
            gameCamera.Position = gameHero.Position;
            gameCamera.Target = gameCamera.Position;
            GameManager.Camera = gameCamera;

            gameProjectileManager = new ProjectileManager();
            gameProjectileManager.Initialize();
            gameProjectileManager.LoadContent(content);
            GameManager.ProjectileManager = gameProjectileManager;

            gameParticleController = new ParticleController();
            gameParticleController.LoadContent(content);
            GameManager.ParticleController = gameParticleController;

            gameWaterController = new WaterController(ScreenManager.GraphicsDevice, gameMap);
            GameManager.WaterController = gameWaterController;

            //gameWater = new Water(ScreenManager.GraphicsDevice, gameMap);
            //GameManager.Water = gameWater;

            parallaxManager = new ParallaxManager(ScreenManager.GraphicsDevice.Viewport);

            ScreenManager.Game.ResetElapsedTime();
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the state of the game. This method checks the GameScreen.IsActive
        /// property, so the game will stop updating when the pause menu is active,
        /// or if you tab away to a different application.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            base.Update(gameTime, otherScreenHasFocus, coveredByOtherScreen);

            if (IsActive)
            {
                gameSpawnerController.Update(gameTime);
                gameEnemyController.Update(gameTime);

                if (GameManager.CurrentScene == 0)
                {
                    //gameCamera.Position = gameHero.Position;
                    gameCamera.Target = gameHero.Position;// -(new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2, ScreenManager.GraphicsDevice.Viewport.Height - 200));
                    gameCamera.RotationTarget = gameHero.spriteRot;
                }
                gameCamera.Update(new Rectangle(0, 0, ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height));

                gameHero.Update(gameTime, mousePos);
                gameProjectileManager.Update(gameTime);
                gameParticleController.Update(gameTime);
            }
           
            parallaxManager.Update(gameTime, gameCamera.Position);
            gameWaterController.Update(gameTime);
        }




        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            // Look up inputs for the active player profile.
            int playerIndex = 0;

            KeyboardState keyboardState = input.CurrentKeyboardStates[playerIndex];
            GamePadState gamePadState = input.CurrentGamePadStates[playerIndex];

            PlayerIndex player;
            if (input.IsPauseGame(ControllingPlayer))
            {
                PauseBackgroundScreen pauseBG = new PauseBackgroundScreen();
                ScreenManager.AddScreen(pauseBG, ControllingPlayer);
                ScreenManager.AddScreen(new PauseMenuScreen(pauseBG), ControllingPlayer);
            }
        
            if(IsActive)
            {
                //Matrix mouseMatrix = Matrix.CreateTranslation(input.LastMouseState.X, input.LastMouseState.Y, 0) * -gameCamera.CameraMatrix;// Matrix.CreateTranslation(input.LastMouseState.X, input.LastMouseState.Y, 0);
                //mouseMatrix += Matrix.CreateTranslation(gameCamera.Position.X, gameCamera.Position.Y, 0);
                //mouseMatrix = mouseMatrix + Matrix.CreateRotationZ(-gameCamera.Rotation);
                //mouseMatrix -= Matrix.CreateTranslation(gameCamera.Width / 2, gameCamera.Height - 200, 0);
                
                //mousePos = Vector2.Transform(mousePos, gameCamera.CameraMatrix);
                

                //Quaternion camRot;
                //Vector3 dummy;
                //gameCamera.CameraMatrix.Decompose(out dummy, out camRot, out dummy);

                

                //mousePos += gameCamera.Position;
                //mousePos -= new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2, ScreenManager.GraphicsDevice.Viewport.Height - 200);

                

                if (input.MouseDragging)
                {
                    
                }


#if WINRT || WINDOWS_PHONE
               // TouchCollection tc = TouchPanel.GetState();


                foreach(TouchLocation tl in TouchPanel.GetState())
                {
                    if (tl.State == TouchLocationState.Pressed|| tl.State == TouchLocationState.Moved)
                    {
                        mousePos = Vector2.Transform(tl.Position, Matrix.Invert(gameCamera.CameraMatrix));
                        gameHero.Fire(mousePos);
                    }
                }
#else



                    mousePos = new Vector2(input.LastMouseState.X, input.LastMouseState.Y);
                    mousePos = Vector2.Transform(mousePos, Matrix.Invert(gameCamera.CameraMatrix));

#endif
               

                if (input.CurrentMouseState.LeftButton == ButtonState.Pressed)
                    gameHero.Fire(mousePos);

                if (input.CurrentKeyboardStates[0].IsKeyDown(Keys.A)) gameHero.MoveBackward();
                if (input.CurrentKeyboardStates[0].IsKeyDown(Keys.D)) gameHero.MoveForward();

                if (Math.Abs(input.AccelerometerVect.X) > 0.15f)
                {
                    if (input.AccelerometerVect.X<0) gameHero.MoveBackward();
                    if (input.AccelerometerVect.X>0) gameHero.MoveForward();
                }

            }
        }


        /// <summary>
        /// Draws the gameplay screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            // This game has a blue background. Why? Because!
            ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
                                               Color.Black, 0, 0);

            //ScreenManager.GraphicsDevice.SetRenderTarget(gameRenderTarget);
            //ScreenManager.GraphicsDevice.Clear(ClearOptions.Target,
            //                                   Color.Black, 0, 0);

            // Our player and enemy are both actually just text strings.
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;

            //Matrix transform = Matrix.CreateTranslation(ScreenManager.GraphicsDevice.Viewport.Width / 2, ScreenManager.GraphicsDevice.Viewport.Height / 2, 0) * Matrix.CreateRotationZ(-gameCamera.Rotation);

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied , SamplerState.PointClamp, null, null, null, gameCamera.CameraMatrix);

            parallaxManager.Draw(spriteBatch);

            gameMap.DrawLayer(spriteBatch, "Collision", gameCamera);
            gameMap.DrawLayer(spriteBatch, "Non-Collision", gameCamera);
            gameMap.DrawLayer(spriteBatch, "Decals", gameCamera);

            gameEnemyController.Draw(spriteBatch);

            gameHero.Draw(spriteBatch);

            //spriteBatch.Draw(gameEnemyController.SpriteSheets[EnemyType.Dude], mousePos, Color.Red);

            gameProjectileManager.Draw(spriteBatch);

            spriteBatch.End();

            gameWaterController.Draw();

            

            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, gameCamera.CameraMatrix);
            gameParticleController.Draw(spriteBatch);
            spriteBatch.End();

            //spriteBatch.Begin();
            //spriteBatch.Draw(gameEnemyController.SpriteSheets[EnemyType.Dude], mousePos, Color.White);
            //spriteBatch.End();

            //ScreenManager.GraphicsDevice.SetRenderTarget(null);

            //spriteBatch.Begin();
            //spriteBatch.Draw(gameRenderTarget, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2, ScreenManager.GraphicsDevice.Viewport.Height-200), null, Color.White, -gameCamera.Rotation, new Vector2(gameRenderTarget.Width, gameRenderTarget.Height) / 2, 1f, SpriteEffects.None, 1);
            //spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition >= 0f)
                ScreenManager.FadeBackBufferToBlack(1f - TransitionAlpha);
        }


        #endregion
    }
}

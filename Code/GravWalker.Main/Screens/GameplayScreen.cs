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
        GravPadController gameGravPadController;
        EnemyController gameEnemyController;
        ProjectileManager gameProjectileManager;
        ParticleController gameParticleController;
        WaterController gameWaterController;

        Hero gameHero;

        HUD gameHUD;

        ParallaxManager parallaxManager;
        //Water gameWater;

        RenderTarget2D gameRenderTarget;

        Vector2 mousePos;

        Vector2 spawnPos;

        float startingTransition = 800f;

        double gameOverTime = 0;
        bool gameOverShown;

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

            AudioController.LoadContent(content);

            gameFont = content.Load<SpriteFont>("menufont");
          
            gameMap = content.Load<Map>("maps/testmap2");
            GameManager.Map = gameMap;

            gameHero = new Hero();
            gameHero.Initialize();
            gameHero.LoadContent(content);
            GameManager.Hero = gameHero;
            spawnPos = gameHero.Position;
            

            gameSpawnerController = new SpawnerController(gameMap);
            gameEnemyController = new EnemyController();
            gameEnemyController.LoadContent(content, gameSpawnerController.RequiredTypes);

            gameCamera = new Camera(ScreenManager.GraphicsDevice.Viewport, gameMap);
            gameCamera.ClampRect = new Rectangle(0, 5 * gameMap.TileHeight, gameMap.Width * gameMap.TileWidth, gameMap.Height * gameMap.TileHeight);
            //gameCamera.Position = gameHero.Position - (new Vector2(ScreenManager.GraphicsDevice.Viewport.Width, ScreenManager.GraphicsDevice.Viewport.Height) / 2);
            gameCamera.Position = gameHero.Position;
            gameCamera.Target = gameCamera.Position;
            gameCamera.Update(ScreenManager.GraphicsDevice.Viewport.Bounds);
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

            gameGravPadController = new GravPadController(gameMap);
            GameManager.GravPadController = gameGravPadController;

            gameHUD = new HUD(ScreenManager.GraphicsDevice.Viewport);
            gameHUD.Alpha = 0f;
            gameHUD.LoadContent(content);

            parallaxManager = new ParallaxManager(ScreenManager.GraphicsDevice.Viewport);
            parallaxManager.Layers.Add(new ParallaxLayer(content.Load<Texture2D>("bg/bg1"), new Vector2(0, 0), 0.4f, false));

            gameHero.Position.Y = spawnPos.Y - startingTransition;

            GameManager.CurrentScene = 0;
            GameManager.SceneTime = 0;

            ScreenManager.Game.ResetElapsedTime();
        }


        /// <summary>
        /// Unload graphics content used by the game.
        /// </summary>
        public override void UnloadContent()
        {
            gameEnemyController.Unload();
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

                if(startingTransition>0f)
                {
                    startingTransition -= 7f;
                    if (startingTransition <= 2f) startingTransition = 0f;
                    gameHero.Position.Y = spawnPos.Y - startingTransition;

                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 speed = ((gameHero.Position - spawnPos)/2) + new Vector2(30f - ((float)gameParticleController.Rand.NextDouble() * 60f), 0);
                        gameParticleController.AddSpark(gameHero.Position+ new Vector2(20f - ((float)gameParticleController.Rand.NextDouble() * 40f),0) , speed * 0.01f);
                    }

                    gameHUD.Alpha = 1f - ((1f / 800f) * startingTransition);

                    if ((spawnPos - gameHero.Position).Length() < 10f) AudioController.PlaySFX("scorestinger");
                }

                if (startingTransition <= 0f)
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
                }


                if(startingTransition<=0f) gameHero.Update(gameTime, mousePos);
                gameProjectileManager.Update(gameTime);
                gameParticleController.Update(gameTime);
                gameGravPadController.Update(gameTime);

                gameHUD.Update(gameTime);

                if (gameHero.HP <= 0 || gameHero.lastSceneComplete==23)
                {
                    if (!gameOverShown)
                    {
                        gameOverTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                        if (gameOverTime >= 2000)
                        {
                            gameOverShown = true;
                            AudioController.StopMusic();

                            ScreenManager.AddScreen(new GameOverScreen(gameHero.lastSceneComplete==23), null);
                        }
                    }
                }
            }
           
            parallaxManager.Update(gameTime, gameCamera.Position);
            gameWaterController.Update(gameTime);

            AudioController.Update(gameTime);
        }




        /// <summary>
        /// Lets the game respond to player input. Unlike the Update method,
        /// this will only be called when the gameplay screen is active.
        /// </summary>
        public override void HandleInput(InputState input)
        {
            if (input == null)
                throw new ArgumentNullException("input");

            if (startingTransition > 0f) return;
              

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
                    if (tl.State == TouchLocationState.Pressed || tl.State == TouchLocationState.Moved)
                    {
                        mousePos = Vector2.Transform(tl.Position, Matrix.Invert(gameCamera.CameraMatrix));
                        gameHero.Fire(mousePos);
                    }
                    else gameHero.NotFiring();
                }
#else



                    mousePos = new Vector2(input.LastMouseState.X, input.LastMouseState.Y);
                    mousePos = Vector2.Transform(mousePos, Matrix.Invert(gameCamera.CameraMatrix));

#endif


                if (input.CurrentMouseState.LeftButton == ButtonState.Pressed)
                    gameHero.Fire(mousePos);
                else
                    gameHero.NotFiring();

                if (input.CurrentKeyboardStates[0].IsKeyDown(Keys.A)) gameHero.MoveBackward();
                if (input.CurrentKeyboardStates[0].IsKeyDown(Keys.D)) gameHero.MoveForward();

                if (input.IsNewKeyPress(Keys.Space, null, out player)) gameHero.DoGravFlip();

                if (Math.Abs(input.AccelerometerVect.Y) > 0.15f)
                {
                    if (input.AccelerometerVect.Y>0) gameHero.MoveBackward();
                    if (input.AccelerometerVect.Y<0) gameHero.MoveForward();
                }

                if (input.CurrentMouseState.Y < 150)
                {
                    if(gameHUD.Alpha>0.2f) gameHUD.Alpha -= 0.02f;
                }
                else
                {
                    if(gameHUD.Alpha<1f) gameHUD.Alpha += 0.02f;
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
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, Matrix.CreateTranslation(-(int)gameCamera.Position.X, -(int)gameCamera.Position.Y, 0) * Matrix.CreateRotationZ(-gameCamera.Rotation) * Matrix.CreateTranslation(gameCamera.Width/2, gameCamera.Height/2, 0));
            parallaxManager.Draw(spriteBatch);           
            spriteBatch.End();




            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied , SamplerState.PointClamp, null, null, null, gameCamera.CameraMatrix);

            

            gameMap.DrawLayer(spriteBatch, "Collision", gameCamera);
            gameMap.DrawLayer(spriteBatch, "Non-Collision", gameCamera);
            gameMap.DrawLayer(spriteBatch, "Decals", gameCamera);

            gameEnemyController.Draw(spriteBatch);

            gameHero.Draw(spriteBatch);

            //spriteBatch.Draw(gameEnemyController.SpriteSheets[EnemyType.Dude], mousePos, Color.Red);

            gameProjectileManager.Draw(spriteBatch);
            gameParticleController.Draw(spriteBatch);

            spriteBatch.End();

            gameWaterController.Draw();

            

            //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, null, null, null, gameCamera.CameraMatrix);
            
            //spriteBatch.End();

            //spriteBatch.Begin();
            //spriteBatch.Draw(gameEnemyController.SpriteSheets[EnemyType.Dude], mousePos, Color.White);
            //spriteBatch.End();

            //ScreenManager.GraphicsDevice.SetRenderTarget(null);

            //spriteBatch.Begin();
            //spriteBatch.Draw(gameRenderTarget, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width / 2, ScreenManager.GraphicsDevice.Viewport.Height-200), null, Color.White, -gameCamera.Rotation, new Vector2(gameRenderTarget.Width, gameRenderTarget.Height) / 2, 1f, SpriteEffects.None, 1);
            //spriteBatch.End();

            spriteBatch.Begin();
            gameHUD.Draw(spriteBatch);
            spriteBatch.End();

            // If the game is transitioning on or off, fade it out to black.
            if (TransitionPosition >= 0f)
                ScreenManager.FadeBackBufferToBlack(1f - TransitionAlpha);
        }


        #endregion
    }
}

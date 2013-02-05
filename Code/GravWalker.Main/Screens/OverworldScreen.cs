#region File Description
//-----------------------------------------------------------------------------
// BackgroundScreen.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using TiledLib;
using System.Collections.Generic;
#endregion

namespace HeroBash
{
    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// </summary>
    /// 

    class OverworldLocation
    {
        public Vector2 Position;
        public int Stage;
        public int Level;
        public bool Available = false;
        public Vector2 ArrowPos = Vector2.Zero;
        public List<OverworldLocation> LinkedLocations = new List<OverworldLocation>();
    }

    public class OverworldScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        Texture2D texBG;
        Texture2D texScoreBG;
        Texture2D texDistance;
        Texture2D texArrow;

        Vector2 heroPos;
        Vector2 princessPos;
        Vector2 princessStartPos;
        Vector2 princessTarget;
        float princessMoveAmount = 0f;

        Camera overworldCamera;
        Map overworldMap;

        Vector2 locationSize = new Vector2(96, 96);
        Vector2 tileSize = new Vector2(64, 64);
        int locationSpacing = 3;

        List<OverworldLocation> Locations = new List<OverworldLocation>();

        OverworldLocation targetLocation;

        bool princessMoving = false;
        double princessMovedTime = 0;

        ScoreBoard Overall;
        ScoreBoard Weekly;
        ScoreBoard MyScores;

        float scoresOffset;

        #endregion

        #region Initialization


     
        public OverworldScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            EnabledGestures = Microsoft.Xna.Framework.Input.Touch.GestureType.Tap;
        }


        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "HeroBashContent");

            overworldMap = content.Load<Map>("maps/overworld");
            overworldCamera = new Camera(ScreenManager.GraphicsDevice.Viewport, overworldMap);
            //overworldCamera.Position = new Vector2(0, (((overworldMap.Height / 2) * overworldMap.TileHeight)+(overworldMap.TileHeight/2)) - (overworldCamera.Height / 2));

            texBG = content.Load<Texture2D>("blank-white");
            texScoreBG = content.Load<Texture2D>("blank");
            texDistance = content.Load<Texture2D>("distance");
            texArrow = content.Load<Texture2D>("overworld-arrow");


            heroPos = new Vector2(0, 0);
            princessPos = new Vector2(0, 0);

            Vector2 locPos = new Vector2((2 * overworldMap.TileWidth) + (overworldMap.TileWidth / 2), (overworldMap.Height * overworldMap.TileHeight) / 2);
            int numLocs = 0;
            for (int stage = 0; stage <= 8; stage++)
            {
                if (stage <= 4)
                    numLocs++;
                else
                    numLocs--;

                locPos.Y = ((overworldMap.Height * overworldMap.TileHeight) / 2) - (((numLocs - 1) * (overworldMap.TileHeight * locationSpacing)) / 2);

                for (int level = 0; level < numLocs; level++)
                {
                    OverworldLocation loc = new OverworldLocation();
                    loc.Position = locPos;
                    loc.Stage = stage;
                    loc.Level = level;
                    Locations.Add(loc);

                    locPos.Y += overworldMap.TileHeight * locationSpacing;

                    if (loc.Stage == GameManager.CurrentStage && loc.Level + 1 == GameManager.CurrentLevel)
                    {
                        overworldCamera.Position = loc.Position - new Vector2(200, (overworldCamera.Height / 2));
                        overworldCamera.Target = loc.Position - new Vector2(200, (overworldCamera.Height / 2));

                        princessPos = loc.Position;
                        princessStartPos = loc.Position;
                    }
                }

                locPos.X += overworldMap.TileWidth * locationSpacing;
                
            }

            foreach (OverworldLocation sourceLoc in Locations)
            {
                foreach (OverworldLocation loc in Locations)
                {
                    if (loc.Stage == sourceLoc.Stage + 1)
                    {
                        if (loc.Stage <= 4)
                        {
                            if (loc.Level == sourceLoc.Level || loc.Level == sourceLoc.Level + 1)
                            {
                                sourceLoc.LinkedLocations.Add(loc);
                            }
                        }
                        else
                        {
                            if (loc.Level == sourceLoc.Level || loc.Level == sourceLoc.Level - 1)
                            {
                                sourceLoc.LinkedLocations.Add(loc);
                            }
                        }
                    }
                }
            }

            Overall = new ScoreBoard(ScoreBoardType.NearbyOverall, ScreenManager.Font, texScoreBG, texBG, GameManager.CurrentPlaythrough, GameManager.CurrentStage, GameManager.Hero.Level, GameManager.CurrentTime);
            Weekly = new ScoreBoard(ScoreBoardType.NearbyWeekly, ScreenManager.Font, texScoreBG, texBG, GameManager.CurrentPlaythrough, GameManager.CurrentStage, GameManager.Hero.Level, GameManager.CurrentTime);
            MyScores = new ScoreBoard(ScoreBoardType.MyNearbyScores, ScreenManager.Font, texScoreBG, texBG, GameManager.CurrentPlaythrough, GameManager.CurrentStage, GameManager.Hero.Level, GameManager.CurrentTime);

            
        }


        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        #endregion

        #region Update and Draw


        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            overworldCamera.Update(ScreenManager.GraphicsDevice.Viewport);

            OverworldLocation currentLoc = null;

            foreach (OverworldLocation loc in Locations)
            {
                if (loc.Stage == GameManager.CurrentStage && loc.Level + 1 == GameManager.CurrentLevel)
                {
                    currentLoc = loc;
                    overworldCamera.Target = loc.Position - new Vector2(200, (overworldCamera.Height / 2));
                }
                foreach (OverworldLocation ll in loc.LinkedLocations) ll.Available = false;
            }


            foreach (OverworldLocation ll in currentLoc.LinkedLocations)
            {
                ll.Available = true;
                ll.ArrowPos = ll.Position + new Vector2(0, -(((float)Math.Sin(gameTime.TotalGameTime.TotalSeconds*6)+1f) * 30f));
            }

            if (princessMoving)
            {
                if (princessMoveAmount >= 1f)
                {
                    princessMovedTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (princessMovedTime >= 500)
                    {
                        if (!this.IsExiting)
                        {
                            LoadingScreen.Load(ScreenManager, false, null, new GameplayScreen());
                        }
                        else
                        {
                            if (TransitionPosition > 0.95f)
                            {
                                GameManager.CurrentStage = targetLocation.Stage;
                                GameManager.CurrentLevel = targetLocation.Level + 1;
                            }
                        }
                    }
                }
                else
                {
                    princessMoveAmount += 0.02f;
                    princessPos = Vector2.Lerp(princessStartPos, princessTarget, princessMoveAmount);
                }
            }

            scoresOffset = (ScreenManager.GraphicsDevice.Viewport.Height / 2) - 315;

            Overall.Position = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width - 220, scoresOffset + 70);
            Weekly.Position = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width - 220, scoresOffset + 265);
            MyScores.Position = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width - 220, scoresOffset + 460);

            base.Update(gameTime, otherScreenHasFocus, false);
        }

        public override void HandleInput(InputState input)
        {
            OverworldLocation currentLoc = null;
            foreach (OverworldLocation loc in Locations)
            {
                if (loc.Stage == GameManager.CurrentStage && loc.Level + 1 == GameManager.CurrentLevel)
                {
                    currentLoc = loc;
                }
            }

            if (input.TapPosition.HasValue)
            {
                foreach (OverworldLocation ll in currentLoc.LinkedLocations)
                {
                    Rectangle testRect = new Rectangle((int)((ll.Position.X - (locationSize.X / 2) - overworldCamera.Position.X)), (int)((ll.Position.Y - (locationSize.Y / 2) - overworldCamera.Position.Y)), (int)locationSize.X, (int)locationSize.Y);
                    if(testRect.Contains(new Point((int)input.TapPosition.Value.X,(int)input.TapPosition.Value.Y)))
                    {
                        targetLocation = ll;
                        princessTarget = ll.Position;
                        princessMoving = true;
                    }
                }
            }

            base.HandleInput(input);
        }

        

        /// <summary>
        /// Draws the background screen.
        /// </summary>
        public override void Draw(GameTime gameTime)
        {
            SpriteBatch spriteBatch = ScreenManager.SpriteBatch;
            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Rectangle fullscreen = new Rectangle(0, 0, viewport.Width, viewport.Height);

            spriteBatch.Begin();

            overworldMap.DrawLayer(spriteBatch, "FG", overworldCamera);

            foreach (OverworldLocation loc in Locations)
            {
                spriteBatch.Draw(texBG, new Rectangle((int)((loc.Position.X - (locationSize.X / 2)) - overworldCamera.Position.X), (int)((loc.Position.Y - (locationSize.Y / 2)) - overworldCamera.Position.Y), (int)locationSize.X, (int)locationSize.Y), null, ((GameManager.CurrentStage==loc.Stage && GameManager.CurrentLevel==loc.Level + 1)?Color.Red:(loc.Available?Color.Yellow:Color.White)) * 0.5f);
                if (loc.Available && !princessMoving)
                {
                    spriteBatch.Draw(texArrow, loc.ArrowPos - overworldCamera.Position, null, Color.White, 0f, new Vector2(32, 64), 1f, SpriteEffects.None, 1);
                }
            }

            spriteBatch.Draw(texDistance, princessPos - overworldCamera.Position, new Rectangle(32, 0, 32, 23), Color.White, 0f, new Vector2(16, 12), 1f, SpriteEffects.None, 1);

            BackgroundBox.Draw(spriteBatch, texScoreBG, new Rectangle(fullscreen.Width - 430, (int)scoresOffset, 420, 50), Color.White * 0.8f);
            spriteBatch.DrawString(ScreenManager.Font, "Current Standings", new Vector2(fullscreen.Width - 220, scoresOffset + 27), Color.White, 0f, ScreenManager.Font.MeasureString("Current Standings") / 2, 1f, SpriteEffects.None, 1);

            Overall.Draw(spriteBatch, 1f);
            Weekly.Draw(spriteBatch, 1f);
            MyScores.Draw(spriteBatch, 1f);

            spriteBatch.End();

            ScreenManager.FadeBackBufferToBlack(1f-TransitionAlpha);
        }


        #endregion
    }
}

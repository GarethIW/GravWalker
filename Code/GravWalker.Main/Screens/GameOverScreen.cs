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
using System.ComponentModel;
#if WINRT
using Windows.System.Threading;
#endif
#endregion

namespace HeroBash
{
    /// <summary>
    /// The background screen sits behind all the other menu screens.
    /// It draws a background image that remains fixed in place regardless
    /// of whatever transitions the screens on top of it may be doing.
    /// </summary>
    public class GameOverScreen : GameScreen
    {
        #region Fields

        ContentManager content;
        Texture2D texBG;
        Texture2D texScoreBG;

        ScoreBoard TopTenOverall;
        ScoreBoard TopTenWeekly;
        ScoreBoard MyScores;

        public static bool overallScoreSubmitted = false;
        public static bool weeklyScoreSubmitted = false;

        float scoresOffset;

        bool shownENS;

        #endregion

        #region Initialization


        /// <summary>
        /// Constructor.
        /// </summary>
        public GameOverScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);

            EnabledGestures = Microsoft.Xna.Framework.Input.Touch.GestureType.Tap;
        }


        /// <summary>
        /// Loads graphics content for this screen. The background texture is quite
        /// big, so we use our own local ContentManager to load it. This allows us
        /// to unload before going from the menus into the game itself, wheras if we
        /// used the shared ContentManager provided by the Game class, the content
        /// would remain loaded forever.
        /// </summary>
        public override void LoadContent()
        {
            if (content == null)
                content = new ContentManager(ScreenManager.Game.Services, "HeroBashContent");

            texBG = content.Load<Texture2D>("blank-white");
            texScoreBG = content.Load<Texture2D>("blank");

            ScoreBoard.LastSubmittedOverallRank = -1;
            ScoreBoard.LastSubmittedWeeklyRank = -1;

            overallScoreSubmitted = false;
            weeklyScoreSubmitted = false;

            shownENS = false;

            GameManager.PlayerName = "Player";
            if (GameManager.PlayerName != "Player")
            {
#if !WINRT
                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += bw_DoWork;
                bw.RunWorkerAsync();
#endif
#if WINRT
                ThreadPool.RunAsync(async delegate { ScoreBoard.Submit(GameManager.CurrentPlaythrough, GameManager.CurrentStage, GameManager.Hero.Level, GameManager.CurrentTime); });
#endif
            }

            
           
        }

        void ens_Done(object sender, PlayerIndexEventArgs e)
        {
#if !WINRT
            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += bw_DoWork;
            bw.RunWorkerAsync();
#endif
#if WINRT
            ThreadPool.RunAsync(async delegate { ScoreBoard.Submit(GameManager.CurrentPlaythrough, GameManager.CurrentStage, GameManager.Hero.Level, GameManager.CurrentTime); });
#endif
        }

#if !WINRT
        void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            ScoreBoard.Submit(GameManager.CurrentPlaythrough, GameManager.CurrentStage, GameManager.Hero.Level, GameManager.CurrentTime);
        }
#endif

        /// <summary>
        /// Unloads graphics content for this screen.
        /// </summary>
        public override void UnloadContent()
        {
            content.Unload();
        }


        #endregion

        #region Update and Draw


        /// <summary>
        /// Updates the background screen. Unlike most screens, this should not
        /// transition off even if it has been covered by another screen: it is
        /// supposed to be covered, after all! This overload forces the
        /// coveredByOtherScreen parameter to false in order to stop the base
        /// Update method wanting to transition off.
        /// </summary>
        public override void Update(GameTime gameTime, bool otherScreenHasFocus,
                                                       bool coveredByOtherScreen)
        {
            if (IsActive)
            {
                if (GameManager.PlayerName == "Player")
                {
                    bool found=false;
                    foreach (GameScreen screen in ScreenManager.GetScreens())
                        if (screen.GetType() == typeof(EnterNameScreen)) found = true;

                    if (!found && !shownENS)
                    {
                        EnterNameScreen ens = new EnterNameScreen();
                        ens.Accepted += ens_Done;
                        ens.Cancelled += ens_Done;
                        ScreenManager.AddScreen(ens, null);

                        shownENS = true;
                    }
                }
            }

            if (overallScoreSubmitted)
            {
                if (ScoreBoard.LastSubmittedOverallRank > -1)
                {
                    if (TopTenOverall == null)
                    {
                        TopTenOverall = new ScoreBoard(ScoreBoardType.NearbyOverall, ScreenManager.Font, texScoreBG, texBG, ScoreBoard.LastSubmittedOverallRank);
                        TopTenOverall.Position = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width + 220, scoresOffset + 70);
                    }
                    if (MyScores == null)
                    {
                        MyScores = new ScoreBoard(ScoreBoardType.MyNearbyScores, ScreenManager.Font, texScoreBG, texBG, ScoreBoard.LastSubmittedOverallRank);
                        MyScores.Position = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width + 220, scoresOffset + 460);
                    }
                }
                else
                {
                    if (TopTenOverall == null)
                    {
                        if (TopTenOverall == null) TopTenOverall = new ScoreBoard(ScoreBoardType.TopTen, ScreenManager.Font, texScoreBG, texBG);
                        TopTenOverall.Position = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width + 220, scoresOffset + 70);
                    }
                    if (MyScores == null)
                    {
                        if (MyScores == null) MyScores = new ScoreBoard(ScoreBoardType.MyScores, ScreenManager.Font, texScoreBG, texBG);
                        MyScores.Position = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width + 220, scoresOffset + 460);

                    }
                }
            }

            if(weeklyScoreSubmitted)
            {
                if (ScoreBoard.LastSubmittedWeeklyRank > -1)
                {


                    if (TopTenWeekly == null)
                    {
                        TopTenWeekly = new ScoreBoard(ScoreBoardType.NearbyWeekly, ScreenManager.Font, texScoreBG, texBG, ScoreBoard.LastSubmittedWeeklyRank);
                        TopTenWeekly.Position = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width + 220, scoresOffset + 265);
                    }

                }
                else
                {
                    if (TopTenWeekly == null)
                    {
                        TopTenWeekly = new ScoreBoard(ScoreBoardType.WeeklyTopTen, ScreenManager.Font, texScoreBG, texBG);
                        TopTenWeekly.Position = new Vector2(ScreenManager.GraphicsDevice.Viewport.Width + 220, scoresOffset + 265);
                    }

                }
            }

            scoresOffset = (ScreenManager.GraphicsDevice.Viewport.Height / 2) - 315;
            if (TopTenOverall != null && TopTenOverall.Position.X > ScreenManager.GraphicsDevice.Viewport.Width - 220) TopTenOverall.Position = Vector2.Lerp(TopTenOverall.Position, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width - 220, TopTenOverall.Position.Y), 0.1f);
            if (TopTenWeekly != null && TopTenWeekly.Position.X > ScreenManager.GraphicsDevice.Viewport.Width - 220) TopTenWeekly.Position = Vector2.Lerp(TopTenWeekly.Position, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width - 220, TopTenWeekly.Position.Y), 0.1f);
            if (MyScores != null && MyScores.Position.X > ScreenManager.GraphicsDevice.Viewport.Width - 220) MyScores.Position = Vector2.Lerp(MyScores.Position, new Vector2(ScreenManager.GraphicsDevice.Viewport.Width - 220, MyScores.Position.Y), 0.1f);

            base.Update(gameTime, otherScreenHasFocus, false);
        }

        public override void HandleInput(InputState input)
        {
            PlayerIndex playerIndex;

            if (input.IsMenuSelect(ControllingPlayer, out playerIndex) || input.IsMenuCancel(ControllingPlayer, out playerIndex) || input.TapPosition.HasValue)
            {
                LoadingScreen.Load(ScreenManager, false, null, new BackgroundScreen(), new MainMenuScreen());
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

            ScreenManager.FadeBackBufferToBlack(TransitionAlpha * 2 / 3);

            spriteBatch.Begin();

            //spriteBatch.Draw(texBG, fullscreen,
              //               Color.White * TransitionAlpha * (0.5f + (0.5f * TransitionPosition)));

            BackgroundBox.Draw(spriteBatch, texScoreBG, new Rectangle(fullscreen.Width - 430, (int)scoresOffset, 420, 50), Color.White * 0.8f);
            spriteBatch.DrawString(ScreenManager.Font, "Final Standings", new Vector2(fullscreen.Width - 220, scoresOffset + 27), Color.White, 0f, ScreenManager.Font.MeasureString("Final Standings") / 2, 1f, SpriteEffects.None, 1);

            spriteBatch.DrawString(ScreenManager.Font, "GAME OVER", new Vector2((viewport.Width-350) / 2, (viewport.Height / 2) / 2), Color.White * TransitionAlpha, 0f, ScreenManager.Font.MeasureString("GAME OVER")/2, 2f, SpriteEffects.None, 1);

            if (overallScoreSubmitted == false && weeklyScoreSubmitted == false)
            {
                spriteBatch.DrawString(ScreenManager.Font, "Submitting High Score...", new Vector2((viewport.Width-350) / 2, ((viewport.Height / 2) / 2)+50), Color.White * TransitionAlpha, 0f, ScreenManager.Font.MeasureString("Submitting High Score...") / 2, 1f, SpriteEffects.None, 1);
            }
            else
            {
                if(TopTenOverall!=null) TopTenOverall.Draw(spriteBatch, TransitionAlpha);
                if (TopTenWeekly != null) TopTenWeekly.Draw(spriteBatch, TransitionAlpha);
                if (MyScores != null) MyScores.Draw(spriteBatch, TransitionAlpha);
            }

            spriteBatch.End();

            //ScreenManager.FadeBackBufferToBlack(1f-TransitionAlpha);
        }


        #endregion
    }
}

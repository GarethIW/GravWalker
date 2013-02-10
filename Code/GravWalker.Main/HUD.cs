using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GravWalker
{
    public enum ScorePartType
    {
        Combo,
        Rope,
        Chopper,
        Stomped,
        Grenade,
        Boat,
        Jeep,
        Spider,
        Repair,
        Flip
    }

    public class ScorePart
    {
        public ScorePartType Type;
        public int Number;
        public float Zoom;
        public double ShakeTime;

        public ScorePart(ScorePartType type)
        {
            Type = type;
            Number = 1;
            Zoom = 5f;
            ShakeTime = 0;
        }

        public void Update(GameTime gameTime)
        {
            if (ShakeTime > 0) ShakeTime -= gameTime.ElapsedGameTime.TotalMilliseconds;

            if(Type!= ScorePartType.Combo || Number>1)
                if (Zoom > 1f) Zoom -= 0.15f;
        }

        public void Increment()
        {
            Number ++;
            ShakeTime = 300;
        }

        public string Text()
        {
            switch (Type)
            {
                case ScorePartType.Combo:
                    return "Combo";
                case ScorePartType.Chopper:
                    return "Chopper Dropper";
                case ScorePartType.Rope:
                    return "Ungrappled";
                case ScorePartType.Stomped:
                    return "Stomped";
                case ScorePartType.Grenade:
                    return "Nice Catch!";
                case ScorePartType.Flip:
                    return "Flippin'eck";
                default:
                    return "-undefined-";
            }
        }
        
    }

    public class HUD
    {
        Viewport viewport;

        Texture2D texHUD;
        SpriteFont fontHUD;

        public float Alpha = 1f;

        public int Score;

        float healthWidth = 100f;
        float heatWidth = 0f;

        Color heatColor = new Color(0, 255, 0);

        Dictionary<ScorePartType, ScorePart> scoreParts = new Dictionary<ScorePartType, ScorePart>();
        double scoreTime = 0f;
        float scoreAlpha = 1f;
        ScorePartType lastAddedScore;

        static Random randomNumber = new Random();

        public HUD(Viewport vp)
        {
            viewport = vp;

            GameManager.HUD = this;
        }

        public void LoadContent(ContentManager content)
        {
            texHUD = content.Load<Texture2D>("hud");
            fontHUD = content.Load<SpriteFont>("hudfont");
        }

        public void Update(GameTime gameTime)
        {
            if(scoreTime>0)
            {
                scoreTime-=gameTime.ElapsedGameTime.TotalMilliseconds;
            }
            else
            {
                if (scoreAlpha > 0f) scoreAlpha -= 0.05f;
                if (scoreAlpha <= 0f) scoreParts.Clear();
            }

            foreach (ScorePart sp in scoreParts.Values)
            {
                sp.Update(gameTime);
            }

            healthWidth = MathHelper.Lerp(healthWidth, ((608f/100f) * GameManager.Hero.HP), 0.1f);
            heatWidth = MathHelper.Lerp(heatWidth, ((553f / 100f) * GameManager.Hero.Heat), 0.1f);

            if(GameManager.Hero.Heat<25f)
                heatColor = Color.Lerp(heatColor, Color.Green, 0.02f);
            else if (GameManager.Hero.Heat < 50f)
                heatColor = Color.Lerp(heatColor, Color.GreenYellow, 0.02f);
            else if (GameManager.Hero.Heat < 85f)
                heatColor = Color.Lerp(heatColor, Color.Yellow, 0.02f);
            else if (GameManager.Hero.Heat <= 105f)
                heatColor = Color.Lerp(heatColor, Color.Red, 0.02f);
          
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            Vector2 barsPanelPos = new Vector2(viewport.Width / 2, 0);

            spriteBatch.Draw(texHUD, barsPanelPos + new Vector2(-302, 3), new Rectangle(46, 128, 608, 21), new Color(100, 0, 0) * Alpha);
            spriteBatch.Draw(texHUD, barsPanelPos + new Vector2(-302, 3), new Rectangle(46, 128, (int)healthWidth, 21), new Color(250, 0, 0) * Alpha);

            spriteBatch.Draw(texHUD, barsPanelPos + new Vector2(-277, 26), new Rectangle(72, 160, 608, 21), new Color((int)(heatColor.R * 0.4), (int)(heatColor.G * 0.4), (int)(heatColor.B * 0.4)) * Alpha);
            spriteBatch.Draw(texHUD, barsPanelPos + new Vector2(-277, 26), new Rectangle(72, 160, (int)heatWidth, 21), heatColor * Alpha);

            spriteBatch.Draw(texHUD, barsPanelPos, new Rectangle(0, 0, 700, 128), Color.White * Alpha, 0f, new Vector2(350,0), 1f, SpriteEffects.None, 1);
           
            string scoreTex = Score.ToString("00000000");
            Vector2 scoreSize = fontHUD.MeasureString(scoreTex);
            ShadowText(spriteBatch, scoreTex, new Vector2(viewport.Width - 280, 10), Color.White, Vector2.Zero, 1f);

            float y = 75;
            foreach (ScorePart sp in scoreParts.Values)
            {
                if (sp.Type != ScorePartType.Combo || sp.Number > 1)
                {
                    Vector2 rand = sp.ShakeTime > 0 ? new Vector2(((float)randomNumber.NextDouble() * 10f) - 5f, ((float)randomNumber.NextDouble() * 10f) - 5f) : Vector2.Zero;
                    ShadowText(spriteBatch, sp.Text(), new Vector2(viewport.Width - 120 - ((fontHUD.MeasureString(sp.Text()).X * 0.75f) / 2), y), Color.LightGray * scoreAlpha, fontHUD.MeasureString(sp.Text()) / 2, sp.Zoom * 0.75f);
                    //ShadowText(spriteBatch, "x", new Vector2(viewport.Width - 100 - ((fontHUD.MeasureString("x").X *0.75f) / 2), y), Color.White * scoreAlpha, fontHUD.MeasureString("x") / 2, sp.Zoom * 0.75f);
                    ShadowText(spriteBatch, "x" + sp.Number.ToString(), new Vector2(viewport.Width - 100, y) + rand, Color.LightGray * scoreAlpha, fontHUD.MeasureString("x" + sp.Number.ToString()) * new Vector2(0,0.5f), sp.Zoom * 0.75f);
                }
                y += 20;
            }
        }

        public void AddScore(EnemyType type)
        {
            AddOrIncrement(ScorePartType.Combo);

            switch (type)
            {
                case EnemyType.Dude:
                    Score += scoreParts[0].Number;
                    break;
                case EnemyType.RopeDude:
                    Score += 5 * scoreParts[0].Number;
                    AddOrIncrement(ScorePartType.Rope);
                    break;
                case EnemyType.Chopper:
                    Score += 20 * scoreParts[0].Number;
                    AddOrIncrement(ScorePartType.Chopper);
                    break;
            }
        }
        public void AddScore(ScorePartType type)
        {
            if (!scoreParts.ContainsKey(ScorePartType.Combo))
            {
                scoreParts.Add(ScorePartType.Combo, new ScorePart(ScorePartType.Combo));
                scoreParts[0].Number = 0;
            }

            AddOrIncrement(type);
            

            switch (type)
            {
                case ScorePartType.Stomped:
                    Score += 3 * (scoreParts[0].Number>0?scoreParts[0].Number:1);
                    break;
                case ScorePartType.Grenade:
                    Score += 5 * (scoreParts[0].Number > 0 ? scoreParts[0].Number : 1);
                    break;
                case ScorePartType.Flip:
                    Score += 10 * (scoreParts[0].Number > 0 ? scoreParts[0].Number : 1);
                    break;
            }
        }

        public void AddOrIncrement(ScorePartType type)
        {
            if (lastAddedScore == ScorePartType.Flip && type == ScorePartType.Flip) return;

            scoreAlpha = 1f;
            scoreTime = 3000;

            if (scoreParts.ContainsKey(type))
            {
                if (type == ScorePartType.Combo && scoreParts[0].Number == 1) AudioController.PlaySFX("scorestinger");
                scoreParts[type].Increment();
            }
            else
            {
                scoreParts.Add(type, new ScorePart(type));
                if (type != ScorePartType.Combo) AudioController.PlaySFX("scorestinger", 0.8f, 0f, 0f);
            }

            lastAddedScore = type;
        }

        void ShadowText(SpriteBatch sb, string text, Vector2 pos, Color col, Vector2 off, float scale)
        {
            sb.DrawString(fontHUD, text, pos + (Vector2.One * 2f), new Color(0,0,0,col.A), 0f, off, scale, SpriteEffects.None, 1);
            sb.DrawString(fontHUD, text, pos, col, 0f, off, scale, SpriteEffects.None, 1);
        }
    }
}

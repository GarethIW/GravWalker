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
   

    public class HUD
    {
        Viewport viewport;

        Texture2D texHUD;
        SpriteFont fontHUD;

        public float Alpha = 1f;

        float healthWidth = 100f;
        float heatWidth = 0f;


        Color heatColor = new Color(0, 255, 0);

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

            spriteBatch.Draw(texHUD, barsPanelPos + new Vector2(-277, 27), new Rectangle(72, 160, 608, 21), new Color((int)(heatColor.R * 0.4), (int)(heatColor.G * 0.4), (int)(heatColor.B * 0.4)) * Alpha);
            spriteBatch.Draw(texHUD, barsPanelPos + new Vector2(-277, 27), new Rectangle(72, 160, (int)heatWidth, 21), heatColor * Alpha);

            spriteBatch.Draw(texHUD, barsPanelPos, new Rectangle(0, 0, 700, 128), Color.White * Alpha, 0f, new Vector2(350,0), 1f, SpriteEffects.None, 1);
           
        }
    }
}

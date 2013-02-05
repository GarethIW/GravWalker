using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using TiledLib;


namespace GravWalker
{
   
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class WaterController
    {
        public static Random randomNumber = new Random();

        public List<Water> Waters = new List<Water>();

        public WaterController(GraphicsDevice gd, Map gameMap) 
        {
            var layer = GameManager.Map.Layers.Where(l => l.Name == "Water").First();
            if (layer != null)
            {
                MapObjectLayer objectlayer = layer as MapObjectLayer;

                foreach (MapObject o in objectlayer.Objects)
                {
                    Color top = Color.White;
                    Color bottom = Color.Black;
                    if (o.Properties.Contains("TopColor"))
                    {
                        top = new Color(byte.Parse(o.Properties["TopColor"].Split(',')[0]), byte.Parse(o.Properties["TopColor"].Split(',')[1]), byte.Parse(o.Properties["TopColor"].Split(',')[2]));
                    }
                    if (o.Properties.Contains("BottomColor"))
                    {
                        bottom = new Color(byte.Parse(o.Properties["BottomColor"].Split(',')[0]), byte.Parse(o.Properties["BottomColor"].Split(',')[1]), byte.Parse(o.Properties["BottomColor"].Split(',')[2]));
                    }

                    Add(gd, gameMap, o.Location, top, bottom);

                }
            }
            GameManager.WaterController = this;
        }

        internal void Add(GraphicsDevice gd, Map gameMap, Rectangle pos, Color topColor, Color bottomColor)
        {

            Waters.Add(new Water(gd, gameMap, pos, topColor, bottomColor));
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            foreach (Water w in Waters)
                w.Update(gameTime);
   
        }

        public void Draw()
        {
            foreach (Water w in Waters)
                w.Draw(GameManager.Camera);
        }


        public void CheckHit(Projectile p)
        {
            foreach (Water w in Waters)
            {
                if (w.bounds.Contains(Helper.VtoP(p.Position)))
                {
                    if (p.Position.Y > w.bounds.Top && p.Position.Y < w.bounds.Top + 15)
                    {
                        w.Splash(p.Position.X, 130);
                    }

                    p.Active = false;
                }
            }
        }
    }
}

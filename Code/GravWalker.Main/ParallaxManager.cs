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
    public class ParallaxLayer
    {
        public Texture2D Texture;
        public Vector2 Position;
        public float ScrollSpeed;
        public bool PositionFromBottom;

        public ParallaxLayer(Texture2D tex, Vector2 pos, float speed, bool bottom)
        {
            Texture = tex;
            Position = pos;
            ScrollSpeed = speed;
            PositionFromBottom = bottom;
        }
    }

    public class ParallaxManager
    {
        public List<ParallaxLayer> Layers = new List<ParallaxLayer>();

        Viewport viewport;

        Vector2 scrollPosition;

        public ParallaxManager(Viewport vp)
        {
            viewport = vp;
        }

        public void Update(GameTime gameTime, Vector2 scrollPos)
        {
            scrollPosition = scrollPos;

            foreach (ParallaxLayer l in Layers)
            {
                l.Position.X = scrollPos.X * l.ScrollSpeed;
            }
        }


        public void Draw(SpriteBatch spriteBatch)
        {

            foreach (ParallaxLayer l in Layers)
            {

                for (float x = l.Position.X; x < scrollPosition.X + spriteBatch.GraphicsDevice.Viewport.Width; x += l.Texture.Width)
                {
                    if (l.Position.X + x > -l.Texture.Width)
                    {
                        spriteBatch.Draw(l.Texture, (l.PositionFromBottom?new Vector2(l.Position.X,spriteBatch.GraphicsDevice.Viewport.Height-l.Position.Y):l.Position) + new Vector2(x, 0), null, new Color(150, 150, 150));
                    }
                }
                  
            }
        }
    }
}

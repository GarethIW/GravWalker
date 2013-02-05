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
    public enum EnemyType
    {
        Dude,
        Tank,
        Chopper,
        RopeDude
    }

    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class EnemyController
    {
        public static Random randomNumber = new Random();

        public List<Enemy> Enemies = new List<Enemy>();

        public Dictionary<EnemyType, Texture2D> SpriteSheets = new Dictionary<EnemyType, Texture2D>();

        public EnemyController() 
        {
            
            GameManager.EnemyController = this;
        }

        public void LoadContent(ContentManager content, List<EnemyType> requiredTypes)
        {
            foreach (EnemyType t in requiredTypes)
            {
                SpriteSheets.Add(t, content.Load<Texture2D>("enemies/" + t.ToString().ToLower()));
            }
        }

        internal void Spawn(EnemyType type, Vector2 position, List<Point> path, int pathNode, int scene)
        {
            switch(type)
            {
                case EnemyType.Dude:
                    Enemies.Add(new Dude(type, position, SpriteSheets[type], path, pathNode, scene));
                    break;
                case EnemyType.RopeDude:
                    Enemies.Add(new RopeDude(type, position, SpriteSheets[type], scene));
                    break;
                case EnemyType.Chopper:
                    Enemies.Add(new Chopper(type, position, SpriteSheets[type], scene));
                    break;
            }
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            foreach (Enemy e in Enemies)
                e.Update(gameTime);

            for (int i = Enemies.Count - 1; i >= 0; i--)
            {           
                 if (!Enemies[i].Active) 
                 {
                     if(Enemies[i].Type!= EnemyType.RopeDude)
                        Enemies.RemoveAt(i);
                     else
                     {
                         ((RopeDude)Enemies[i]).ropeLength-=5f;
                         if (((RopeDude)Enemies[i]).ropeLength <= 0) Enemies.RemoveAt(i);
                     }
                }
            }
                
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Enemy e in Enemies)
                e.Draw(spriteBatch);
        }
 
    }
}

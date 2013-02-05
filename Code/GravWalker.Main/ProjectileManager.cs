using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiledLib;

namespace GravWalker
{
    public enum ProjectileType
    {
        WalkerGun,
        DudePistol
    }

    public class Projectile
    {
        public Vector2 Position;
        public Vector2 Speed;
        public bool OwnedByHero;
        public ProjectileType Type;
        public bool Active;
        public double Life;

        public float alpha;
        public float rot;

        public void Spawn(Vector2 pos, Vector2 speed, double life, bool heroowner, ProjectileType type)
        {
            Active = true;
            Position = pos;
            Speed = speed;
            Life = life;
            OwnedByHero = heroowner;
            Type = type;

            alpha = 1f;
            rot = Helper.V2ToAngle(Speed);
        }
    }

    public class ProjectileManager
    {
        const int MAX_PROJECTILES = 500;

        public static Random randomNumber = new Random();
        
        public List<Projectile> Projectiles = new List<Projectile>();

        public Texture2D spriteSheet;

        Vector2 frameSize = new Vector2(32, 32);

        public ProjectileManager()
        {
            Initialize();
        }

        public void Initialize()
        {
            for (int i = 0; i < MAX_PROJECTILES; i++)
                Projectiles.Add(new Projectile());
        }

        public void LoadContent(ContentManager content)
        {
            spriteSheet = content.Load<Texture2D>("projectiles");
            
        }

        public void Add(Vector2 loc, Vector2 speed, double life, bool ownerhero, ProjectileType type)
        {
            foreach (Projectile p in Projectiles)
            {
                if (!p.Active)
                {
                    p.Spawn(loc, speed, life, ownerhero, type);
                    break;
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            //var t = GameManager.Map.Layers.Where(l => l.Name == "Collision").First();
            //TileLayer tileLayer = t as TileLayer;

            for(int l=0;l<GameManager.Map.Layers.Count;l++)
            {
                if(GameManager.Map.Layers[l].Name=="Collision")
                {
                    TileLayer tileLayer = GameManager.Map.Layers[l] as TileLayer;

                    for (int p = 0; p < Projectiles.Count; p++)
                    {
                        if (Projectiles[p].Active)
                        {
                            Projectiles[p].Position += Projectiles[p].Speed;

                            Projectiles[p].rot = Helper.V2ToAngle(Projectiles[p].Speed);

                            //if (p.Position.X < 0f || p.Position.X > GameManager.Map.Width * GameManager.Map.TileWidth) p.Active = false;

                            GameManager.WaterController.CheckHit(Projectiles[p]);

                            Projectiles[p].Life -= gameTime.ElapsedGameTime.TotalMilliseconds;
                            if (Projectiles[p].Life <= 0)
                            {
                                Projectiles[p].alpha -= 0.1f;
                                if (Projectiles[p].alpha <= 0f) Projectiles[p].Active = false;
                            }

                            if (Projectiles[p].Life > 0 || Projectiles[p].alpha > 0)
                            {
                                // do collision checks
                                if (Projectiles[p].OwnedByHero)
                                {
                                    foreach (Enemy e in GameManager.EnemyController.Enemies)
                                    {
                                        if (!Projectiles[p].Active) break;

                                        if (e.CheckHit(Projectiles[p].Position, Projectiles[p].Speed))
                                        {
                                            Projectiles[p].Active = false;
                                        }
                                    }
                                }
                                else
                                {
                                    if (GameManager.Hero.CheckHit(Projectiles[p].Position, Projectiles[p].Speed)) Projectiles[p].Active = false;
                                }
                            }
                        }
                    }
                    
                }
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            foreach (Projectile p in Projectiles)
            {
                if(p.Active)
                    spriteBatch.Draw(spriteSheet, p.Position, new Rectangle((int)p.Type * (int)frameSize.X, 0, (int)frameSize.X, (int)frameSize.Y), Color.White * p.alpha, p.rot, frameSize / 2, 1f, SpriteEffects.None, 1);
            }
        }
    }
}

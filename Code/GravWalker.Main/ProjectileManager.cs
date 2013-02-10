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
        DudePistol,
        Grenade
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
                                if (Projectiles[p].Type == ProjectileType.Grenade) ExplodeGrenade(Projectiles[p], false);

                                Projectiles[p].alpha -= 0.1f;
                                if (Projectiles[p].alpha <= 0f) Projectiles[p].Active = false;
                            }

                            if (Projectiles[p].Life > 0 || Projectiles[p].alpha > 0)
                            {
                                if (Projectiles[p].Type == ProjectileType.Grenade)
                                {
                                    Projectiles[p].Speed.Y += 0.1f;
                                    GameManager.ParticleController.Add(Projectiles[p].Position + new Vector2(((float)randomNumber.NextDouble() * 5f) - 2.5f, ((float)randomNumber.NextDouble() * 5f) - 2.5f), Vector2.Zero, 100f, false, false, new Rectangle(8,0,8,8), 0f, Color.Gray);

                                    for (int i = 0; i < Projectiles.Count; i++)
                                        if (Projectiles[i].OwnedByHero && Projectiles[i].Active)
                                            if ((Projectiles[i].Position - Projectiles[p].Position).Length() <= 8) ExplodeGrenade(Projectiles[p], true);
                                }

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
                                    if (GameManager.Hero.CheckHit(Projectiles[p].Position, Projectiles[p].Speed, Projectiles[p].Type, false))
                                    {
                                        Projectiles[p].Active = false;
                                        if (Projectiles[p].Type == ProjectileType.Grenade) ExplodeGrenade(Projectiles[p], false);
                                    }
                                    
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
                if (p.Active)
                {
                    spriteBatch.Draw(spriteSheet, p.Position, new Rectangle((int)p.Type * (int)frameSize.X, 0, (int)frameSize.X, (int)frameSize.Y), Color.White * p.alpha, p.rot, frameSize / 2, 1f, SpriteEffects.None, 1);
                }
            }
        }

        void ExplodeGrenade(Projectile p, bool hurtsEnemies)
        {
            p.Active = false;
            p.alpha = 0;
            p.Life = 0;

            for (float r = 0; r < 200; r += 20f)
            {
                for (float circ = 0; circ < MathHelper.TwoPi; circ += 0.25f)
                {
                    Vector2 checkPos = p.Position + Helper.AngleToVector(circ, r);

                    Vector2 speed = (p.Position - checkPos);
                    speed.Normalize();

                    if(!hurtsEnemies)
                        GameManager.Hero.CheckHit(checkPos, speed * 2f, ProjectileType.Grenade, true);

                    if(hurtsEnemies)
                        foreach (Enemy e in GameManager.EnemyController.Enemies)
                            e.CheckHit(checkPos, speed);
                 }
            }

            GameManager.ParticleController.AddExplosion(p.Position);
            AudioController.PlaySFX("explode", 0.9f, -0.5f, 0f, p.Position);

            if (hurtsEnemies)
                GameManager.HUD.AddScore(ScorePartType.Grenade);
        }
    }
}

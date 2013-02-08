using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Input.Touch;
using Microsoft.Xna.Framework.Media;
using TiledLib;

namespace GravWalker
{
    public class Hero
    {
        static Random randomNumber = new Random();

        public Vector2 Position;
        public Vector2 CenterPosition;
        public Vector2 Target;
        public Vector2 Velocity = new Vector2(0,0);

        public float HP;
        public float Heat;

        Vector2 frameSize = new Vector2(64, 64);
        Vector2 frameOffset = new Vector2(32, 64);
        Texture2D spriteSheet;

        double animTime = 50;
        double currentFrameTime = 0;
        public int animFrame = 0;
        int numFrames = 10;
        bool isAnimating = false;


        double updateTime = 0;
        double targetUpdateTime = 0;

        float normalAngle = -MathHelper.PiOver2;
        public float spriteRot;

        Vector2 LastPosition;

        public List<Point> currentPath;
        public int forwardNode;
        public int backwardNode;
        int currentDirection = 1;

        int faceDirection = 1;

        double fireRateCooldown = 0;

        float mouseAngle;
        float gunAngle;
        Vector2 gunPos;

        float muzzleAlpha = 0f;

        List<int> ScenesTriggered = new List<int>();

        public Hero()
        {

        }

        public void Initialize() 
        {
            Vector2 chosenSpawn = new Vector2(9999999,0);
            // Try to find a spawn point
            var layer = GameManager.Map.Layers.Where(l => l.Name == "Spawn").First();
            if (layer!=null)
            {
                MapObjectLayer objectlayer = layer as MapObjectLayer;

                foreach (MapObject o in objectlayer.Objects)
                {
                    var pathLayer = GameManager.Map.Layers.Where(l => l.Name == "Paths").First();
                    MapObjectLayer pathObjectLayer = pathLayer as MapObjectLayer;
                    foreach (MapObject path in pathObjectLayer.Objects)
                    {
                        foreach (Point p in path.LinePoints)
                        {
                            if (o.Location.Contains(p))
                            {
                                currentPath = path.LinePoints;
                                forwardNode = path.LinePoints.IndexOf(p) +1;
                                backwardNode = path.LinePoints.IndexOf(p);
                                Position = Helper.PtoV(p);
                            }
                        }
                    }
                           
                    
                }
                Target = GetPathVector(forwardNode);
            }

            //spriteRot = Helper.AngleBetween(Position, Target);

            HP = 100f;
            Heat = 0f;
           
            spriteRot = Helper.TurnToFace(Position, Target, spriteRot, currentDirection, 1f);

            
        }

        

        public void LoadContent(ContentManager content)
        {
            spriteSheet = content.Load<Texture2D>("walker");
        }



        public void Update(GameTime gameTime, Vector2 mousePos)
        {
            updateTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            fireRateCooldown -= gameTime.ElapsedGameTime.TotalMilliseconds;

            if (muzzleAlpha > 0f) muzzleAlpha -= 0.1f;

            if (isAnimating)
            {
                currentFrameTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (currentFrameTime >= animTime)
                {
                    currentFrameTime = 0;
                    animFrame += (currentDirection!=faceDirection?-1:1);
                    if (animFrame == numFrames) animFrame = 0;
                    if (animFrame == -1) animFrame = numFrames-1;
                }
            }

            
            spriteRot = Helper.TurnToFace(Position, Target, spriteRot,currentDirection, 0.1f);

            mouseAngle = Helper.V2ToAngle((mousePos - Position)) - spriteRot;
            if (Helper.WrapAngle(mouseAngle+MathHelper.PiOver2)<0f)
                faceDirection = -1;
            else
                faceDirection = 1;

            CenterPosition = Position + (Helper.AngleToVector(Helper.WrapAngle(spriteRot - MathHelper.PiOver2), 32f));
            gunPos = Position + (Helper.AngleToVector(Helper.WrapAngle(((spriteRot + (0.36f * faceDirection)) - MathHelper.PiOver2)), 42f));
            gunAngle = Helper.V2ToAngle((mousePos - gunPos));// -spriteRot;

            isAnimating = false;



            CheckSceneTriggers(gameTime);
            
        }

        private void CheckSceneTriggers(GameTime gameTime)
        {
            if (GameManager.CurrentScene > 0) GameManager.SceneTime += gameTime.ElapsedGameTime.TotalMilliseconds;

            for (int l = 0; l < GameManager.Map.Layers.Count; l++)
            {
                if (GameManager.Map.Layers[l].Name == "Scenes")
                {
                    MapObjectLayer ol = GameManager.Map.Layers[l] as MapObjectLayer;

                    foreach (MapObject mo in ol.Objects)
                    {
                        if(mo.Location.Contains(Helper.VtoP(Position)))
                        {
                            int scene = int.Parse(mo.Name);

                            if (!ScenesTriggered.Contains(scene))
                            {
                                ScenesTriggered.Add(scene);
                                GameManager.CurrentScene = scene;
                                GameManager.SceneTime = 0;

                            }
                        }
                    }
                }
            }
        }



        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(spriteSheet, new Vector2(Position.X, Position.Y), new Rectangle((isAnimating?animFrame:5) * (int)frameSize.X, 0, (int)frameSize.X, (int)frameSize.Y), Color.White, spriteRot , frameOffset, 1f, faceDirection==-1?SpriteEffects.FlipHorizontally:SpriteEffects.None, 1);
            spriteBatch.Draw(spriteSheet, new Vector2(Position.X, Position.Y), new Rectangle(0, (int)frameSize.Y + 5, (int)frameSize.X, (int)frameSize.Y - 10), Color.White, spriteRot, new Vector2(32, 59), 1f, faceDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1);

            Vector2 barrelPos = gunPos + Helper.AngleToVector(Helper.WrapAngle((gunAngle+((float)randomNumber.NextDouble()*0.2f)-0.1f)), 15f);
            spriteBatch.Draw(spriteSheet, barrelPos, new Rectangle((int)frameSize.X*2, (int)frameSize.Y+5, (int)frameSize.X, (int)frameSize.Y-5), Color.White * muzzleAlpha, gunAngle, new Vector2(32, 27), 1f, SpriteEffects.None, 1);

            spriteBatch.Draw(spriteSheet, gunPos, new Rectangle((int)frameSize.X, (int)frameSize.Y+5, (int)frameSize.X, (int)frameSize.Y-5), Color.White, gunAngle, new Vector2(32, 27), 1f, SpriteEffects.None, 1);

        }

        public void MoveForward()
        {
            if (currentDirection == -1)
            {
                //spriteRot = spriteRot + MathHelper.Pi;
                Target = GetPathVector(forwardNode);
                currentDirection = 1;
            }

            Vector2 moveVect;

            if ((Position - Target).Length() < 10f)
            {
                if (forwardNode < currentPath.Count - 1)
                {
                    backwardNode = forwardNode;
                    forwardNode++;
                    Target = GetPathVector(forwardNode);

                    moveVect = Target - Position;
                    moveVect.Normalize();

                    if((GameManager.Camera.Position - (Position+ (moveVect *4f))).Length()<(GameManager.Camera.Width * 0.4f))
                        Position += moveVect * 4f;
                }
            }
            else
            {
                moveVect = Target - Position;
                moveVect.Normalize();
                if ((GameManager.Camera.Position - (Position + (moveVect * 4f))).Length() < (GameManager.Camera.Width * 0.4f))
                    Position += moveVect * 4f;
            }

            gunPos = Position + (Helper.AngleToVector(Helper.WrapAngle(((spriteRot + (0.36f * faceDirection)) - MathHelper.PiOver2)), 42f));

            isAnimating = true;
        }

        public void MoveBackward()
        {
            if (currentDirection == 1)
            {
                //spriteRot = spriteRot - MathHelper.Pi;
                Target = GetPathVector(backwardNode);
                currentDirection = -1;
            }

            Vector2 moveVect;

            if ((Position - Target).Length() < 10f)
            {
                if (backwardNode > 0)
                {
                    forwardNode = backwardNode;
                    backwardNode--;
                    Target = GetPathVector(backwardNode);

                    moveVect = Target - Position;
                    moveVect.Normalize();
                    if ((GameManager.Camera.Position - (Position + (moveVect * 4f))).Length() < (GameManager.Camera.Width * 0.4f))
                        Position += moveVect * 4f;
                }
            }
            else
            {
                moveVect = Target - Position;
                moveVect.Normalize();
                if ((GameManager.Camera.Position - (Position + (moveVect * 4f))).Length() < (GameManager.Camera.Width * 0.4f))
                    Position += moveVect * 4f;
            }

            gunPos = Position + (Helper.AngleToVector(Helper.WrapAngle(((spriteRot + (0.36f * faceDirection)) - MathHelper.PiOver2)), 42f));

            isAnimating = true;
        }

        private Vector2 GetPathVector(int targetNode)
        {
            return Helper.PtoV(currentPath[targetNode]);
        }




        internal void Fire(Vector2 mousePos)
        {
            if (fireRateCooldown <= 0 && Heat < 100f)
            {
                fireRateCooldown = 100;
                Vector2 barrelPos = gunPos + Helper.AngleToVector(Helper.WrapAngle(gunAngle), 10f);

                Vector2 vect = (mousePos + new Vector2(5f - ((float)randomNumber.NextDouble() * 10f), 5f - ((float)randomNumber.NextDouble() * 10f))) - gunPos;
                vect.Normalize();

                Vector2 speed = vect * 15f;
                Vector2 pos = barrelPos + (speed);//Position + (Helper.AngleToVector(Helper.WrapAngle(spriteRot - MathHelper.PiOver2), 48f)) + (speed * 2f);
                //pos = Vector2.Transform(pos, Matrix.CreateRotationZ(spriteRot));


                GameManager.ProjectileManager.Add(pos, speed, 500, true, ProjectileType.WalkerGun);
                muzzleAlpha = 1f;

                Heat += 4f;


                AudioController.PlaySFX("machinegun", 1f, -1f, -0.7f, Position);
            }
            else
            {
                if (Heat >= 100f)
                {
                    Vector2 barrelPos = gunPos + Helper.AngleToVector(Helper.WrapAngle(gunAngle), (float)randomNumber.NextDouble()*10f);
                    GameManager.ParticleController.AddSpark(barrelPos, Vector2.Zero);
                }
            }
        }

        internal void NotFiring()
        {
            if (fireRateCooldown <= 0 && Heat > 0f) Heat -= 1f;
        }

        public bool CheckHit(Vector2 pos, Vector2 speed)
        {
            //if (!Active) return false;

            bool isHit = false;

            if ((pos - CenterPosition).Length() <= 15)
            {
                
                isHit = true;

                HP -= 0.2f;
                GameManager.ParticleController.AddMetalDebris(pos, speed*0.2f);
            }

            return isHit;
        }

       
    }
}

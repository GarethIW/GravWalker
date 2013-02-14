using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiledLib;

namespace GravWalker
{
    public class Jeep : PathingEnemy
    {
        double rpgTime = 0;

        Vector2 gunPos;
        float gunAngle;

        double gunCooldown = 5000;

        bool braking = false;

        SoundEffectInstance jeepSound;

        public Jeep(EnemyType type, Vector2 position, Texture2D sheet, List<Point> path, bool loop, int pathnode, int scene)
            : base(type, position, sheet, path, loop, pathnode, scene)
        {
            numFrames = 2;
            animTime = 50;
            isAnimating = true;

            Speed = 0f;

            frameSize = new Vector2(64,64);
            frameOffset = new Vector2(32, 36);
            hitRadius = 20;

            centerOffestLength = 12;

            fireRate = 50;
            fireCountdown = 50;

            HP = 25;

            jeepSound = AudioController.effects["truck"].CreateInstance();
            jeepSound.Volume = 0f;
            jeepSound.IsLooped = true;
            jeepSound.Play();
        }

        public override void Update(GameTime gameTime)
        {
            gunCooldown -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (gunCooldown <= 0) gunCooldown = 3000;

            if ((GameManager.Camera.Position - Position).Length() < GameManager.Camera.Width / 2)
            {
                if ((GameManager.Hero.Position - Position).Length() < 400f)
                {
                   // if (EnemyController.randomNumber.Next(50) == 1) 
                        braking = true;
                        hasStopped = true;
                }
            }

            if (hasStopped && Speed<=0)
            {
                if (rpgTime > 0) rpgTime -= gameTime.ElapsedGameTime.TotalMilliseconds;

                isAnimating = false;

                //braking = false;
                
                //if (GameManager.Hero.Position.X < Position.X) currentDirection = -1; else currentDirection = 1;

                if (EnemyController.randomNumber.Next(GameManager.EnemyController.grenadeProbability) == 1 && (Position-GameManager.Hero.Position).Length()<800)
                {
                    Vector2 vect = (GameManager.Hero.CenterPosition + new Vector2(10f - ((float)EnemyController.randomNumber.NextDouble() * 20f), 10f - ((float)EnemyController.randomNumber.NextDouble() * 20f))) - gunPos;
                    vect.Normalize();

                    Vector2 speed = vect * 5f;
                    Vector2 pos = gunPos + (speed);

                    GameManager.ProjectileManager.Add(pos, speed, 1000, false, ProjectileType.Rocket);
                    AudioController.PlaySFX("mortar");
                    rpgTime = 1000;
                }
                if ((GameManager.Hero.Position - Position).Length() > 450f && !stoppedOnBarrier)
                {
                    if (EnemyController.randomNumber.Next(50) == 1)
                    {
                        isAnimating = true;
                        animFrame = 0;
                        braking = false;
                        hasStopped = false;
                        if (GameManager.Hero.Position.X < Position.X)
                        {
                            currentDirection = 1;
                            MoveBackward();
                        }
                        else
                        {
                            currentDirection = -1;
                            MoveForward();
                        }
                    }
                }
            }

            gunPos = Position + (Helper.AngleToVector(Helper.WrapAngle(((SpriteRot - ((MathHelper.PiOver2 - 0.9f) * currentDirection)) - MathHelper.PiOver2)), 32f));
            gunAngle = Helper.V2ToAngle(GameManager.Hero.CenterPosition - gunPos);  //Helper.V2ToAngle((GameManager.Hero.Position - gunPos));

            var layer = GameManager.Map.Layers.Where(l => l.Name == "EnemyBarriers").First();
            if (layer != null)
            {
                MapObjectLayer objectlayer = layer as MapObjectLayer;

                foreach (MapObject o in objectlayer.Objects)
                {
                    if (o.Location.Contains(Helper.VtoP(Position)))
                    {
                        if (EnemyController.randomNumber.Next(5) == 1)
                        {
                            hasStopped = true;
                            braking = true;
                            stoppedOnBarrier = true;
                        }
                    }
                }
            }

            if (braking)
            {
                if (Speed > 0f) Speed -= 0.02f;
            }
            else
                if (Speed < 3f) Speed += 0.02f;

            if (!hasStopped || braking == true)
            {
                if (currentDirection==-1)
                    MoveBackward();
                else
                    MoveForward();

                SpriteRot = Helper.TurnToFace(Position, Target, SpriteRot, currentDirection, 0.1f);
            }

            if (spawnAlpha < 1f) spawnAlpha += 0.1f;
            if (muzzleAlpha > 0f) muzzleAlpha -= 0.1f;

            fireCountdown -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (fireCountdown <= 0)
            {
                fireCountdown = fireRate + (EnemyController.randomNumber.NextDouble() * (fireRate / 10));
                DoFire();
            }

            if (isAnimating)
            {
                currentFrameTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (currentFrameTime >= animTime)
                {
                    currentFrameTime = 0;
                    animFrame++;
                    if (animFrame == numFrames) animFrame = 0;
                }
            }

            centerPosition = Position + (Helper.AngleToVector(Helper.WrapAngle(SpriteRot - MathHelper.PiOver2), centerOffestLength));

            //faceDirection = GameManager.Hero.Position.X < Position.X ? -1 : 1;

            Vector2 screenPos = Vector2.Transform(Position, GameManager.Camera.CameraMatrix);
            jeepSound.Pan = MathHelper.Clamp((screenPos.X - (GameManager.Camera.Width / 2)) / (GameManager.Camera.Width / 2), -1f, 1f);
            jeepSound.Volume = MathHelper.Clamp(((1f / 1200) * (1200 - (GameManager.Hero.Position - Position).Length())), 0f, 0.4f);

            if (HP <= 0) Die();
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);

            Vector2 barrelPos = gunPos + Helper.AngleToVector(Helper.WrapAngle((gunAngle + ((float)EnemyController.randomNumber.NextDouble() * 0.2f) - 0.1f)), 15f);
            spriteBatch.Draw(spriteSheet, barrelPos, new Rectangle((int)frameSize.X * 1, (int)frameSize.Y + 5, (int)frameSize.X, (int)frameSize.Y - 5), Color.White * muzzleAlpha, gunAngle, new Vector2(32, 27), 1f, SpriteEffects.None, 1);

            spriteBatch.Draw(spriteSheet, gunPos, new Rectangle(0, 64, 64, 64), Color.White, gunAngle, new Vector2(32, 32), 1f, SpriteEffects.None, 1);
        }

        public override void DoHit(Vector2 pos, Vector2 speed)
        {
            HP--;
            GameManager.ParticleController.AddMetalDebris(pos, speed * 0.1f);
            AudioController.PlaySFX("metalhit" + (EnemyController.randomNumber.Next(4) + 1), 0.5f, 0f, 0.3f, Position);
            if (EnemyController.randomNumber.Next(20) == 1)
                AudioController.PlaySFX("ricochet", 0.4f, 0f, 0.3f, Position);
            base.DoHit(pos, speed);
        }

        public override void DoFire()
        {
            if (gunCooldown > 1000) return;

            if ((GameManager.Hero.Position - Position).Length() < 600)
            {
                Vector2 vect = (GameManager.Hero.CenterPosition + new Vector2(10f - ((float)EnemyController.randomNumber.NextDouble() * 20f), 10f - ((float)EnemyController.randomNumber.NextDouble() * 20f))) - gunPos;
                vect.Normalize();

                Vector2 speed = vect * 10f;
                Vector2 pos = gunPos + (speed);

                GameManager.ProjectileManager.Add(pos, speed, 900, false, ProjectileType.DudePistol);
                AudioController.PlaySFX("smg", 0.5f, 0.3f, 0.6f, Position);

                muzzleAlpha = 1f;
            }

            base.DoFire();
        }

        public override void Die()
        {
            GameManager.ParticleController.AddGSW(Position + Helper.AngleToVector(Helper.WrapAngle(SpriteRot - MathHelper.PiOver2), 10f), Helper.AngleToVector(Helper.WrapAngle(SpriteRot - MathHelper.PiOver2), 1f));
            GameManager.ParticleController.AddGibs(centerPosition);
            GameManager.ParticleController.AddJeepGibs(centerPosition);
            GameManager.ParticleController.AddExplosion(centerPosition);
            AudioController.PlaySFX("explode", 0.9f, -0.5f, 0f, Position);

            jeepSound.Stop();

            base.Die();
        }
    }
}

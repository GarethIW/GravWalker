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
    public class Boat : Enemy
    {
        Vector2 gunPos;
        float gunAngle;
        double gunCooldown = 5000;

        Vector2 spawnPosition;

        SoundEffectInstance chopperSound;

        bool inBarrier = false;

        public Boat(EnemyType type, Vector2 position, Texture2D sheet, int scene)
            : base(type, position, sheet, scene)
        {
            spawnPosition = position;
            numFrames = 1;
            animTime = 50;
            isAnimating = true;
            animFrame = 0;

            Speed = 0.003f;

            frameSize = new Vector2(64,64);
            frameOffset = new Vector2(32, 20);
            hitRadius = 25;

            centerOffestLength = 0;

            fireRate = 50;
            fireCountdown = 50;

            HP = 20;

            Target = GameManager.Hero.Position + Helper.AngleToVector(GameManager.Hero.spriteRot, 400f);

            chopperSound = AudioController.effects["boat"].CreateInstance();
            chopperSound.Volume = 0f;
            chopperSound.IsLooped = true;
            chopperSound.Play();
        }

        public override void Update(GameTime gameTime)
        {
            if (!Active) return;

            base.Update(gameTime);

            gunCooldown -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (gunCooldown <= 0) gunCooldown = 3000;

            
            Target.Y = spawnPosition.Y;

            if (Target.X < Position.X) faceDirection = -1;
            else faceDirection = 1;

            bool found = false;

            var layer = GameManager.Map.Layers.Where(l => l.Name == "EnemyBarriers").First();
            if (layer != null)
            {
                MapObjectLayer objectlayer = layer as MapObjectLayer;

                foreach (MapObject o in objectlayer.Objects)
                {
                    if (o.Location.Contains(Helper.VtoP(Position)) && !inBarrier)
                    {
                        Target.X = Position.X + ((o.Location.Width * 5) * -faceDirection);
                        //inBarrier = true;
                        found = true;
                    }
                }
            }

            //if (!found) inBarrier = false;

            //if(!inBarrier)
            //{
            if ((Position - Target).Length() < 200f && Speed <= 0.1f)
            {
                Target = GameManager.Hero.Position + Helper.AngleToVector(((GameManager.Hero.spriteRot - MathHelper.PiOver2) - (MathHelper.Pi * 0.4f)) + ((float)EnemyController.randomNumber.NextDouble() * (MathHelper.Pi * 0.8f)), (float)EnemyController.randomNumber.Next(400));
                //inBarrier = false;
            }
            //}

            if ((Target - Position).Length() > 200f)
            {
                if (Target.X < Position.X) SpriteRot += 0.01f;
                if (Target.X > Position.X) SpriteRot -= 0.01f;

                Speed += 0.01f;

                GameManager.WaterController.BoatSplash(Position + (Helper.AngleToVector(Helper.WrapAngle(((SpriteRot - ((MathHelper.PiOver2 + 0.3f) * faceDirection)) - MathHelper.PiOver2)), 20f)));

                //SpriteRot += (0.01f);
                SpriteRot = MathHelper.Clamp(SpriteRot, -0.2f, 0.2f);
            }
            else
            {
                Speed -= 0.1f;

                SpriteRot = MathHelper.Lerp(SpriteRot, 0f, 0.1f);
            }

            Speed = MathHelper.Clamp(Speed, 0f, 3f);

            if (Target.X < Position.X) faceDirection = -1;
            else faceDirection = 1;

            if (spawnAlpha >= 1f)
                //Position = Vector2.Lerp(Position, Target, Speed);
                Position.X += Speed * faceDirection;

            gunPos = Position + (Helper.AngleToVector(Helper.WrapAngle(((SpriteRot - ((MathHelper.PiOver2 - 0.3f) * faceDirection)) - MathHelper.PiOver2)), 20f));
            gunAngle = Helper.V2ToAngle((GameManager.Hero.Position - gunPos));

            Vector2 screenPos = Vector2.Transform(Position, GameManager.Camera.CameraMatrix);
            chopperSound.Pan = MathHelper.Clamp((screenPos.X - (GameManager.Camera.Width / 2)) / (GameManager.Camera.Width / 2), -1f, 1f);
            chopperSound.Volume = MathHelper.Clamp(((1f / 1200) * (1200 - (GameManager.Hero.Position - Position).Length())), 0f, 1f);

            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            if (!Active) return;

            spriteBatch.Draw(spriteSheet, new Vector2(Position.X, Position.Y), new Rectangle(animFrame * (int)frameSize.X, 0, (int)frameSize.X, (int)frameSize.Y), Color.White * spawnAlpha, SpriteRot, frameOffset, 1f, Target.X < Position.X ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1);


            Vector2 barrelPos = gunPos + Helper.AngleToVector(Helper.WrapAngle((gunAngle + ((float)EnemyController.randomNumber.NextDouble() * 0.2f) - 0.1f)), 15f);
            spriteBatch.Draw(spriteSheet, barrelPos, new Rectangle((int)frameSize.X * 1, (int)frameSize.Y + 5, (int)frameSize.X, (int)frameSize.Y - 5), Color.White * muzzleAlpha, gunAngle, new Vector2(32, 27), 1f, SpriteEffects.None, 1);

            spriteBatch.Draw(spriteSheet, gunPos, new Rectangle(0,64,64,64), Color.White, gunAngle, new Vector2(32, 32), 1f, SpriteEffects.None, 1);


        }

        public override void DoHit(Vector2 pos, Vector2 speed)
        {
            HP--;
            GameManager.ParticleController.AddMetalDebris(pos, speed*0.1f);
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
            GameManager.ParticleController.AddBoatGibs(centerPosition);
            GameManager.ParticleController.AddExplosion(centerPosition);
            AudioController.PlaySFX("explode", 0.9f, -0.5f, 0f, Position);

            chopperSound.Stop();

            base.Die();
        }
    }
}

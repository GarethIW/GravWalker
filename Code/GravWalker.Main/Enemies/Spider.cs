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
    public class Spider : Enemy
    {
        Vector2 gunPos;
        float gunAngle;

        Rectangle spawnLocation;

        bool inBarrier = false;

        public Spider(EnemyType type, Vector2 position, Rectangle loc, Texture2D sheet, int scene)
            : base(type, position, sheet, scene)
        {
            spawnLocation = loc;

            numFrames = 7;
            animTime = 50;
            isAnimating = true;
            animFrame = 0;

            Speed = 2f;

            frameSize = new Vector2(64,64);
            frameOffset = new Vector2(32, 32);
            hitRadius = 25;

            centerOffestLength = 0;

            fireRate = 2000;
            fireCountdown = 0;

            HP = 15;

            Target = NewTargetLocation();
        }

        public override void Update(GameTime gameTime)
        {
            if (!Active) return;

            base.Update(gameTime);


            if ((Position - Target).Length() < 10f)
            {
                Target = NewTargetLocation();
            }

            Vector2 normDirection = Target - Position;
            normDirection.Normalize();

            if (spawnAlpha >= 1f)
                Position += normDirection * Speed;

            SpriteRot = Helper.TurnToFace(Position, Target, SpriteRot, 1f, 0.1f);

            gunPos = Position;// +(Helper.AngleToVector(Helper.WrapAngle(((SpriteRot - ((MathHelper.PiOver2 - 0.3f) * faceDirection)) - MathHelper.PiOver2)), 20f));
            gunAngle = Helper.V2ToAngle((GameManager.Hero.Position - gunPos));

        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            if (!Active) return;

            spriteBatch.Draw(spriteSheet, new Vector2(Position.X, Position.Y), new Rectangle(animFrame * (int)frameSize.X, 0, (int)frameSize.X, (int)frameSize.Y), Color.White * spawnAlpha, SpriteRot, frameOffset, 1f, SpriteEffects.None, 1);


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
            if ((GameManager.Hero.Position - Position).Length() < 600)
            {
                Vector2 vect = (GameManager.Hero.CenterPosition + new Vector2(10f - ((float)EnemyController.randomNumber.NextDouble() * 20f), 10f - ((float)EnemyController.randomNumber.NextDouble() * 20f))) - gunPos;
                vect.Normalize();

                Vector2 speed = vect * 5f;
                Vector2 pos = gunPos + (speed);

                GameManager.ProjectileManager.Add(pos, speed, 1000, false, ProjectileType.Rocket);
                AudioController.PlaySFX("mortar", 0.5f, 0.3f, 0.6f, Position);

                muzzleAlpha = 1f;
            }

            base.DoFire();
        }

        public override void Die()
        {
            GameManager.ParticleController.AddGSW(Position + Helper.AngleToVector(Helper.WrapAngle(SpriteRot - MathHelper.PiOver2), 10f), Helper.AngleToVector(Helper.WrapAngle(SpriteRot - MathHelper.PiOver2), 1f));
            GameManager.ParticleController.AddGibs(centerPosition);
            GameManager.ParticleController.AddSpiderGibs(centerPosition);
            GameManager.ParticleController.AddExplosion(centerPosition);
            AudioController.PlaySFX("explode", 0.9f, -0.5f, 0f, Position);

            base.Die();
        }

        Vector2 NewTargetLocation()
        {
            return new Vector2(spawnLocation.X + EnemyController.randomNumber.Next(spawnLocation.Width), spawnLocation.Y + EnemyController.randomNumber.Next(spawnLocation.Height));
        }
    }
}

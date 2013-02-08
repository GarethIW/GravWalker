using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GravWalker
{
    public class Chopper : Enemy
    {
        Vector2 gunPos;
        float gunAngle;
        double gunCooldown = 5000;

        public Chopper(EnemyType type, Vector2 position, Texture2D sheet, int scene)
            : base(type, position, sheet, scene)
        {
            numFrames = 2;
            animTime = 50;
            isAnimating = true;
            animFrame = 0;

            Speed = 0.005f;

            frameSize = new Vector2(64,64);
            frameOffset = new Vector2(32, 32);
            hitRadius = 25;

            centerOffestLength = 0;

            fireRate = 100;
            fireCountdown = 100;

            HP = 25;

            Target = GameManager.Hero.Position + Helper.AngleToVector(GameManager.Hero.spriteRot, 400f);

            
        }

        public override void Update(GameTime gameTime)
        {
            if (!Active) return;

            gunCooldown -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (gunCooldown <= 0) gunCooldown = 5000;

            if(EnemyController.randomNumber.Next(100)==1)
                Target = GameManager.Hero.Position + Helper.AngleToVector(((GameManager.Hero.spriteRot-MathHelper.PiOver2) - (MathHelper.Pi * 0.4f)) + ((float)EnemyController.randomNumber.NextDouble() * (MathHelper.Pi *0.8f)), 400f);

            if ((Target - Position).Length() > 200f)
            {
                if (Target.X < Position.X) SpriteRot -= 0.01f;
                if (Target.X > Position.X) SpriteRot += 0.01f;
                SpriteRot = MathHelper.Clamp(SpriteRot, -0.2f, 0.2f);
            }
            else
            {
                SpriteRot = MathHelper.Lerp(SpriteRot, 0f, 0.1f);
            }

            if(spawnAlpha>=1f)
                Position = Vector2.Lerp(Position, Target, Speed);

            gunPos = Position + (Helper.AngleToVector(Helper.WrapAngle(((SpriteRot + ((MathHelper.PiOver2 + 0.3f) * faceDirection)) - MathHelper.PiOver2)), 20f));
            gunAngle = Helper.V2ToAngle((GameManager.Hero.Position - gunPos));

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {

            if (!Active) return;

            spriteBatch.Draw(spriteSheet, gunPos, new Rectangle(0,64,64,64), Color.White, gunAngle, new Vector2(32, 32), 1f, SpriteEffects.None, 1);

            base.Draw(spriteBatch);
        }

        public override void DoHit(Vector2 pos, Vector2 speed)
        {
            HP--;
            GameManager.ParticleController.AddMetalDebris(pos, speed*0.1f);
            base.DoHit(pos, speed);
        }

        public override void DoFire()
        {
            if (gunCooldown > 1000) return;

            Vector2 vect = (GameManager.Hero.CenterPosition + new Vector2(10f - ((float)EnemyController.randomNumber.NextDouble() * 20f), 10f - ((float)EnemyController.randomNumber.NextDouble() * 20f))) - gunPos;
            vect.Normalize();

            Vector2 speed = vect * 10f;
            Vector2 pos = gunPos + (speed);

            GameManager.ProjectileManager.Add(pos, speed, 900, false, ProjectileType.DudePistol);
            AudioController.PlaySFX("smg", 0.5f, 0.3f, 0.6f, Position);

            base.DoFire();
        }

        public override void Die()
        {
            GameManager.ParticleController.AddGSW(Position + Helper.AngleToVector(Helper.WrapAngle(SpriteRot - MathHelper.PiOver2), 10f), Helper.AngleToVector(Helper.WrapAngle(SpriteRot - MathHelper.PiOver2), 1f));
            GameManager.ParticleController.AddGibs(centerPosition);
            GameManager.ParticleController.AddChopperGibs(centerPosition);
            GameManager.ParticleController.AddExplosion(centerPosition);
            AudioController.PlaySFX("explode", 0.9f, -0.5f, 0f, Position);
            base.Die();
        }
    }
}

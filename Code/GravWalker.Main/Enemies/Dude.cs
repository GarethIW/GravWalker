using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GravWalker
{
    public class Dude : PathingEnemy
    {
        

        public Dude(EnemyType type, Vector2 position, Texture2D sheet, List<Point> path, int pathnode, int scene)
            : base(type, position, sheet, path, pathnode, scene)
        {
            numFrames = 4;
            animTime = 100;
            isAnimating = true;

            Speed = 2f;

            frameSize = new Vector2(16,16);
            frameOffset = new Vector2(8, 16);
            hitRadius = 10;

            centerOffestLength = 8;

            fireCountdown = 2000;

            HP = 1;
        }

        public override void Update(GameTime gameTime)
        {
            if ((GameManager.Camera.Position - Position).Length() < GameManager.Camera.Width / 2)
            {
                if ((GameManager.Hero.Position - Position).Length() < 400f)
                {
                    if (EnemyController.randomNumber.Next(50) == 1) hasStopped = true;
                }
            }

            if (hasStopped)
            {
                isAnimating = false;
                animFrame = 4;
                if (GameManager.Hero.Position.X < Position.X) currentDirection = -1; else currentDirection = 1;
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }

        public override void DoHit(Vector2 pos, Vector2 speed)
        {
            HP--;
            GameManager.ParticleController.AddGSW(pos, speed*0.1f);
            base.DoHit(pos, speed);
        }

        public override void DoFire()
        {
            if (hasStopped)
            {
                Vector2 vect = (GameManager.Hero.CenterPosition + new Vector2(10f - ((float)EnemyController.randomNumber.NextDouble() * 20f), 10f - ((float)EnemyController.randomNumber.NextDouble() * 20f))) - centerPosition;
                vect.Normalize();

                Vector2 speed = vect * 10f;
                Vector2 pos = centerPosition + (speed);

                GameManager.ProjectileManager.Add(pos, speed, 900, false, ProjectileType.DudePistol);
            }

            base.DoFire();
        }

        public override void Die()
        {
            GameManager.ParticleController.AddGSW(Position + Helper.AngleToVector(Helper.WrapAngle(SpriteRot - MathHelper.PiOver2), 10f), Helper.AngleToVector(Helper.WrapAngle(SpriteRot - MathHelper.PiOver2), 1f));
            GameManager.ParticleController.AddGibs(centerPosition);
            base.Die();
        }
    }
}

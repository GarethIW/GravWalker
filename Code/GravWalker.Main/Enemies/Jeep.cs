using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GravWalker
{
    public class Jeep : PathingEnemy
    {
        double rpgTime = 0;

        public Jeep(EnemyType type, Vector2 position, Texture2D sheet, List<Point> path, bool loop, int pathnode, int scene)
            : base(type, position, sheet, path, loop, pathnode, scene)
        {
            numFrames = 4;
            animTime = 100;
            isAnimating = true;

            Speed = 2f;

            frameSize = new Vector2(64,64);
            frameOffset = new Vector2(32, 32);
            hitRadius = 10;

            centerOffestLength = 12;

            fireCountdown = 3000;
            fireRate = 3000;

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
                if (rpgTime > 0) rpgTime -= gameTime.ElapsedGameTime.TotalMilliseconds;

                isAnimating = false;
                if (rpgTime > 0)
                    animFrame = 7;
                else
                    animFrame = 4;
                if (GameManager.Hero.Position.X < Position.X) currentDirection = -1; else currentDirection = 1;

                if (EnemyController.randomNumber.Next(GameManager.EnemyController.grenadeProbability) == 1 && (Position-GameManager.Hero.Position).Length()<600 && GameManager.EnemyController.totalGameTime>2000)
                {
                    Vector2 grenTarget = GameManager.Hero.Position + new Vector2(0, -250f);
                    if (grenTarget.Y > Position.Y) grenTarget.Y = GameManager.Hero.Position.Y;
                    grenTarget.Y += (EnemyController.randomNumber.Next(50) - 25);
                    Vector2 grenSpeed = (grenTarget - Position)/60f;
                    GameManager.ProjectileManager.Add(Position, grenSpeed, 2000, false, ProjectileType.Grenade);
                    AudioController.PlaySFX("mortar");
                    rpgTime = 1000;
                }
            }

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Vector2 barrelPos = centerPosition + Helper.AngleToVector(Helper.WrapAngle(((SpriteRot + (MathHelper.Pi * (GameManager.Hero.Position.X<Position.X ? 1 : 0))) + ((float)EnemyController.randomNumber.NextDouble() * 0.2f) - 0.1f)), 6f);
            spriteBatch.Draw(spriteSheet, barrelPos, new Rectangle(0, 21, 16, 3), Color.White * muzzleAlpha, SpriteRot, new Vector2(8, 1), 1f, GameManager.Hero.Position.X < Position.X ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1);

            base.Draw(spriteBatch);
        }

        public override void DoHit(Vector2 pos, Vector2 speed)
        {
            HP--;
            GameManager.ParticleController.AddGSW(pos, speed*0.1f);
            AudioController.PlaySFX("hit", 0.4f, 0.3f, 0.6f, Position);
            base.DoHit(pos, speed);
        }

        public override void DoFire()
        {
            if (hasStopped)
            {
                if ((GameManager.Hero.Position - Position).Length() < 600)
                {
                    Vector2 vect = (GameManager.Hero.CenterPosition + new Vector2(10f - ((float)EnemyController.randomNumber.NextDouble() * 20f), 10f - ((float)EnemyController.randomNumber.NextDouble() * 20f))) - centerPosition;
                    vect.Normalize();

                    Vector2 speed = vect * 10f;
                    Vector2 pos = centerPosition + (speed);

                    GameManager.ProjectileManager.Add(pos, speed, 900, false, ProjectileType.DudePistol);

                    if (EnemyController.randomNumber.Next(2) == 1)
                        AudioController.PlaySFX("pistol", 0.4f, 0.3f, 0.6f, Position);

                    muzzleAlpha = 1f;
                }
            }

            base.DoFire();
        }

        public override void Die()
        {
            GameManager.ParticleController.AddGSW(Position + Helper.AngleToVector(Helper.WrapAngle(SpriteRot - MathHelper.PiOver2), 10f), Helper.AngleToVector(Helper.WrapAngle(SpriteRot - MathHelper.PiOver2), 1f));
            GameManager.ParticleController.AddGibs(centerPosition);

            if (EnemyController.randomNumber.Next(5) == 1)
                AudioController.PlaySFX("death" + (EnemyController.randomNumber.Next(4) + 1), 1f, -0.1f, 0.1f, Position);

            base.Die();
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GravWalker
{
    public class Enemy
    {
        public bool Active;

        public Vector2 Position;
        public Vector2 Target;
        public EnemyType Type;

        public float Speed;

        internal double fireRate = 1000;
        internal double fireCountdown = 0;

        internal Vector2 frameSize = new Vector2(64, 64);
        internal Vector2 frameOffset = new Vector2(32, 64);
        internal Texture2D spriteSheet;

        internal Vector2 centerPosition;
        internal float centerOffestLength;

        internal double animTime = 50;
        internal double currentFrameTime = 0;
        internal int animFrame = 0;
        internal int numFrames = 1;
        internal bool isAnimating;

        internal float hitRadius;

        internal int faceDirection;

        public float SpriteRot;

        public int HP;

        internal float spawnAlpha;

        public int Scene = 0;

        internal float muzzleAlpha = 0f;

        public Enemy(EnemyType type, Vector2 position, Texture2D sheet, int scene)
        {
            Type = type;
            Position = position;
            Target = Position;
            spriteSheet = sheet;
            Scene = scene;

            Active = true;

            fireCountdown = 0;

            spawnAlpha = 0f;
        }

        public virtual void Update(GameTime gameTime)
        {
            if (spawnAlpha < 1f) spawnAlpha += 0.1f;
            if (muzzleAlpha > 0f) muzzleAlpha -= 0.1f;

            fireCountdown -= gameTime.ElapsedGameTime.TotalMilliseconds;
            if (fireCountdown <= 0)
            {
                fireCountdown = fireRate + (EnemyController.randomNumber.NextDouble() * (fireRate/10));
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

            faceDirection = GameManager.Hero.Position.X < Position.X ? -1 : 1;

            if (HP <= 0) Die();
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(spriteSheet, new Vector2(Position.X, Position.Y), new Rectangle(animFrame * (int)frameSize.X, 0, (int)frameSize.X, (int)frameSize.Y), Color.White * spawnAlpha, SpriteRot, frameOffset, 1f, GameManager.Hero.Position.X<Position.X ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1);

        }

        public virtual void Die()
        {
            GameManager.HUD.AddScore(Type);
            Active = false;
        }

        public virtual bool CheckHit(Vector2 pos, Vector2 speed)
        {
            if (!Active) return false;

            bool isHit = false;

            if ((pos - centerPosition).Length() <= hitRadius)
            {
                DoHit(pos, speed);
                isHit = true;
            }

            return isHit;
        }

        public virtual void DoHit(Vector2 pos, Vector2 speed)
        { }

        public virtual void DoFire()
        {
           
        }
    }
}

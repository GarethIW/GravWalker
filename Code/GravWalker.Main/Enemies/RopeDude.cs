using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GravWalker
{
    public class RopeDude : Enemy
    {
        float ropeLengthMax;
        public float ropeLength;

        Vector2 anchorPosition;

        public RopeDude(EnemyType type, Vector2 position, Texture2D sheet, int scene)
            : base(type, position, sheet, scene)
        {
            numFrames = 1;
            animTime = 100;
            isAnimating = false;
            animFrame = 5;

            Speed = 0.02f;

            frameSize = new Vector2(16,16);
            frameOffset = new Vector2(8, 0);
            hitRadius = 10;

            centerOffestLength = -8;

            fireCountdown = 2000;

            HP = 2;

            ropeLengthMax = 100f + ((float)EnemyController.randomNumber.NextDouble() * 200f);
            ropeLength = 0;

            Target = Position + new Vector2(0, ropeLengthMax);

            anchorPosition = Position;
        }

        public override void Update(GameTime gameTime)
        {
            if (!Active) return;

            if(spawnAlpha>=1f)
                Position = Vector2.Lerp(Position, Target, Speed);

            ropeLength = (Position - anchorPosition).Length();

            

            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            Vector2 ropeVect = Position - anchorPosition;
            ropeVect.Normalize();
            for (float i = 0; i < ropeLength; i += 2f)
            {
                Vector2 pos = anchorPosition + (ropeVect * i);
                spriteBatch.Draw(GameManager.ParticleController._texParticles, pos, new Rectangle(12, 4, 1, 1), Color.White);
                //GameManager.ParticleController.Add(pos, Vector2.Zero, 1f, false, false, new Rectangle(8, 0, 8, 8), 0f, Color.White);
            }

            if (!Active) return;

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
            Vector2 vect = (GameManager.Hero.CenterPosition + new Vector2(10f - ((float)EnemyController.randomNumber.NextDouble() * 20f), 10f - ((float)EnemyController.randomNumber.NextDouble() * 20f))) - centerPosition;
            vect.Normalize();

            Vector2 speed = vect * 10f;
            Vector2 pos = centerPosition + (speed);

            GameManager.ProjectileManager.Add(pos, speed, 900, false, ProjectileType.DudePistol);
            

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

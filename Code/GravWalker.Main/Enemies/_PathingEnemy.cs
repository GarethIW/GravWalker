using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiledLib;

namespace GravWalker
{
    public class PathingEnemy : Enemy
    {
        public List<Point> Path;
        public bool pathLoops;
        internal int forwardNode;
        internal int backwardNode;
        internal int currentDirection = -1;

        internal bool hasStopped = false;

        public PathingEnemy(EnemyType type, Vector2 position, Texture2D sheet, List<Point> path, bool loop, int pathnode, int scene)
            : base(type, position, sheet, scene)
        {
            Path = path;
            pathLoops = loop;
            if (currentDirection == -1)
            {
                backwardNode = pathnode - 1;
                forwardNode = pathnode;
                Target = GetPathVector(backwardNode);
            }
            if (currentDirection == 1)
            {
                backwardNode = pathnode;
                forwardNode = pathnode + 1;
                Target = GetPathVector(forwardNode);
            }
            SpriteRot = Helper.TurnToFace(Position, Target, SpriteRot, currentDirection, 1f);
        }

        public override void Update(GameTime gameTime)
        {
            if (!hasStopped)
            {
                if (GameManager.Hero.Position.X < Position.X)
                    MoveBackward();
                else
                    MoveForward();

                SpriteRot = Helper.TurnToFace(Position, Target, SpriteRot, currentDirection, 0.1f);
            }

            var layer = GameManager.Map.Layers.Where(l => l.Name == "EnemyBarriers").First();
            if (layer != null)
            {
                MapObjectLayer objectlayer = layer as MapObjectLayer;

                foreach (MapObject o in objectlayer.Objects)
                {
                    if (o.Location.Contains(Helper.VtoP(Position)) && (GameManager.Hero.Position - Position).Length() < 600)
                    {
                        if (EnemyController.randomNumber.Next(5) == 1) hasStopped = true;
                    }
                }
            }

            base.Update(gameTime);
        }

        public void MoveForward()
        {
            if (currentDirection == -1)
            {
               // SpriteRot = SpriteRot + MathHelper.Pi;
                
                Target = GetPathVector(forwardNode);
                currentDirection = 1;
                SpriteRot = Helper.TurnToFace(Position, Target, SpriteRot, currentDirection, 1f);
            }

            Vector2 moveVect;

            if ((Position - Target).Length() < 10f)
            {
                if (forwardNode < Path.Count - 1)
                {
                    backwardNode = forwardNode;
                    forwardNode++;
                    Target = GetPathVector(forwardNode);

                    moveVect = Target - Position;
                    moveVect.Normalize();
                    Position += moveVect * Speed;
                }
                else
                {
                    hasStopped = true;
                    fireCountdown = 0;
                }
            }
            else
            {
                moveVect = Target - Position;
                moveVect.Normalize();
                Position += moveVect * Speed;
            }

        }

        public void MoveBackward()
        {
            if (currentDirection == 1)
            {
                //SpriteRot = SpriteRot - MathHelper.Pi;
                Target = GetPathVector(backwardNode);
                currentDirection = -1;
                SpriteRot = Helper.TurnToFace(Position, Target, SpriteRot, currentDirection, 1f);
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
                    Position += moveVect * Speed;
                }
                else
                {
                    hasStopped = true;
                    fireCountdown = 0;
                }
            }
            else
            {
                moveVect = Target - Position;
                moveVect.Normalize();
                Position += moveVect * Speed;
            }

        }

        private Vector2 GetPathVector(int targetNode)
        {
            return Helper.PtoV(Path[targetNode]);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(spriteSheet, new Vector2(Position.X, Position.Y), new Rectangle(animFrame * (int)frameSize.X, 0, (int)frameSize.X, (int)frameSize.Y), Color.White * spawnAlpha, SpriteRot, frameOffset, 1f, currentDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 1);
        }
    }
}

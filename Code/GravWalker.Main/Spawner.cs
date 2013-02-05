using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GravWalker
{
    public class Spawner
    {
        public Vector2 Position;
        public float Distance;
        public double Rate;
        public int Number;
        public EnemyType Type;
        public bool OtherPath;
        public bool IsPathSpawn;
        public Vector2 SpawnOffset;
        public int Scene;
        public double SceneDelay;

        List<Point> Path;
        int PathNode;

        double currentTime;
        public int currentNumber;

        double currentDelay;

        public Spawner(EnemyType type, Vector2 position, List<Point> path, int pathnode, float distance, double rate, int number, bool pathspawn, bool otherpath, Vector2 offset, int scene, double scenedelay)
        {
            Type = type;
            Position = position;
            Path = path;
            PathNode = pathnode;
            Distance = distance;
            Rate = rate;
            Number = number;
            OtherPath = otherpath;
            IsPathSpawn = pathspawn;
            SpawnOffset = offset;

            Scene = scene;
            SceneDelay = scenedelay;

            currentNumber = Number;
            currentTime = Rate;
        }

        public void Update(GameTime gameTime)
        {
            if (Scene == 0 || (Scene == GameManager.CurrentScene && GameManager.SceneTime >= SceneDelay))
            {
                if (currentNumber > 0)
                {
                    if (!OtherPath || (Path != GameManager.Hero.currentPath) || Scene >0)
                    {
                        if ((!IsPathSpawn && (Position - GameManager.Hero.Position).Length() <= Distance) ||
                            (IsPathSpawn && (Math.Abs(PathNode - GameManager.Hero.forwardNode) < Distance || Math.Abs(GameManager.Hero.backwardNode - PathNode) < Distance)) ||
                            Scene > 0)
                        {
                            currentTime += gameTime.ElapsedGameTime.TotalMilliseconds;
                            if (currentTime >= Rate - EnemyController.randomNumber.Next(100))
                            {
                                currentTime = 0;
                                GameManager.EnemyController.Spawn(Type, Position + SpawnOffset, Path, PathNode, Scene);
                                currentNumber--;
                            }
                        }
                    }
                }
            }
        }
    }
}

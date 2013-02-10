using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TiledLib;

namespace GravWalker
{
    public enum GravDirection
    {
        Up,
        Down,
        Left,
        Right
    }

    public class GravPad
    {
        public Rectangle Location;
        public int Id;
        public GravDirection Direction;
        public int Opposite;

        public float UpAngle = 0f;

        public List<Point> Path;
        public bool pathLoops;
        public int PathNode;

        public bool hasHealed = false;

        public GravPad(int id, Rectangle rect, GravDirection dir, int opp, List<Point> path, bool loop, int pathNode)
        {
            Id = id;
            Location = rect;
            Direction = dir;
            Opposite = opp;
            UpAngle = GravPadController.DirToUp(dir);
            Path = path;
            pathLoops = loop;
            PathNode = pathNode;
        }
    }

    public class GravPadController
    {
        public Dictionary<int, GravPad> GravPads = new Dictionary<int, GravPad>();

        double particleTime = 0;

        public GravPadController(Map gameMap)
        {
            var layer = GameManager.Map.Layers.Where(l => l.Name == "GravPads").First();
            if (layer != null)
            {
                MapObjectLayer objectlayer = layer as MapObjectLayer;

                foreach (MapObject o in objectlayer.Objects)
                {
                    GravDirection dir = GravDirection.Up;
                    List<Point> path = new List<Point>();
                    bool pathLoops = false;
                    int pathnode = 0;
                    int opp = 0;

                    int id = int.Parse(o.Name);
                    if (o.Properties.Contains("Direction")) dir = (GravDirection)Enum.Parse(typeof(GravDirection), o.Properties["Direction"], true); 
                    if (o.Properties.Contains("Opposite")) opp = int.Parse(o.Properties["Opposite"]);

                    var pathLayer = GameManager.Map.Layers.Where(l => l.Name == "Paths").First();
                    MapObjectLayer pathObjectLayer = pathLayer as MapObjectLayer;
                    foreach (MapObject mappath in pathObjectLayer.Objects)
                    {
                        foreach (Point p in mappath.LinePoints)
                        {
                            if (o.Location.Contains(p))
                            {
                                path = mappath.LinePoints;
                                pathnode = mappath.LinePoints.IndexOf(p);

                                pathLoops = bool.Parse(mappath.Properties["Looping"]);
                            }
                        }
                    }

                    GravPads.Add(id, new GravPad(id, o.Location, dir, opp, path, pathLoops, pathnode));
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            particleTime += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (particleTime >= 100) particleTime = 0;

            foreach (GravPad g in GravPads.Values)
            {
                if (particleTime == 0)
                {
                    Vector2 loc = new Vector2((float)GameManager.ParticleController.Rand.Next(g.Location.Width), (float)GameManager.ParticleController.Rand.Next(g.Location.Height));
                    loc += Helper.PtoV(new Point(g.Location.Left, g.Location.Top));

                    GameManager.ParticleController.Add(loc, Helper.AngleToVector(g.UpAngle, 2f), 1500, false, false, new Rectangle(0, 64, 40, 40), 0f, Color.White * 0.7f);
                }
            }
        }

        public static float DirToUp(GravDirection dir)
        {
            switch (dir)
	        {
		        case GravDirection.Left:
                    return 0f;
                case GravDirection.Right:
                    return Helper.WrapAngle(MathHelper.Pi);
                case GravDirection.Up:
                     return -MathHelper.PiOver2;
                case GravDirection.Down:
                    return MathHelper.PiOver2;
                default:
                    return 0f;
	        }

        }
        
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using TiledLib;


namespace GravWalker
{
   
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class SpawnerController
    {
        public List<Spawner> Spawners = new List<Spawner>();

        public List<EnemyType> RequiredTypes = new List<EnemyType>();

        public SpawnerController(Map gameMap) 
        {
            var layer = GameManager.Map.Layers.Where(l => l.Name == "EnemySpawns").First();
            if (layer != null)
            {
                MapObjectLayer objectlayer = layer as MapObjectLayer;

                foreach (MapObject o in objectlayer.Objects)
                {
                    EnemyType type = EnemyType.Dude;
                    float distance = 500;
                    double rate = 1000;
                    int number = 10;
                    Vector2 position = Vector2.Zero;
                    List<Point> path = new List<Point>();
                    int pathnode = 0;
                    bool isPathSpawn = false;
                    bool otherpath = false;
                    Vector2 offset = Vector2.Zero;
                    int scene = 0;
                    double scenedelay = 0;

                    try
                    {
                        type = (EnemyType)Enum.Parse(typeof(EnemyType), o.Type, true);
                    }
                    catch (Exception ex) { }

                    if (o.Properties.Contains("Distance")) distance = float.Parse(o.Properties["Distance"]);
                    if (o.Properties.Contains("Rate")) rate = double.Parse(o.Properties["Rate"]);
                    if (o.Properties.Contains("Number")) number = int.Parse(o.Properties["Number"]);
                    if (o.Properties.Contains("OtherPath")) otherpath = bool.Parse(o.Properties["OtherPath"]);
                    if (o.Properties.Contains("IsPathSpawn")) isPathSpawn = bool.Parse(o.Properties["IsPathSpawn"]);
                    if (o.Properties.Contains("Offset")) offset = new Vector2(float.Parse(o.Properties["Offset"].Split(',')[0]), float.Parse(o.Properties["Offset"].Split(',')[1]));
                    if (o.Properties.Contains("SceneDelay")) scenedelay = double.Parse(o.Properties["SceneDelay"]);
                    if (o.Properties.Contains("Scene")) scene = int.Parse(o.Properties["Scene"]);

                    bool found=false;
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
                                position = Helper.PtoV(p);
                                found = true;
                            }
                        }
                    }

                    if(!found)
                    {
                        // Not a pathed enemy, use rectangle position
                        position = Helper.PtoV(o.Location.Center);
                    }

                    Spawners.Add(new Spawner(type, position, path, pathnode, distance, rate, number, isPathSpawn, otherpath, offset, scene, scenedelay));

                    if (!RequiredTypes.Contains(type)) RequiredTypes.Add(type);
                }
            }

            GameManager.SpawnerController = this;
        }

        

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void Update(GameTime gameTime)
        {
            int numToSpawn = 0;

            foreach (Spawner s in Spawners)
            {
                s.Update(gameTime);

                if (s.Scene == GameManager.CurrentScene)
                    numToSpawn += s.currentNumber;
            }

            if (numToSpawn == 0)
            {
                bool found=false;
                foreach (Enemy e in GameManager.EnemyController.Enemies)
                {
                    if (e.Scene == GameManager.CurrentScene && e.Active) found = true;
                }
                
                if(!found) GameManager.CurrentScene = 0;
            }
        }

     
    }
}

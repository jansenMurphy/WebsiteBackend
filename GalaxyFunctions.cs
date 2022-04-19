
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace WebsiteBackend
{
    
    public class GalaxyFunctions{

        public static void GeneratePlanets(string galaxyName, IDBConnection dBConnection, int playerCount){
            List<Planet> planets = new List<Planet>();
            if(!dBConnection.MakeDBIfNoneExists()){
                //THERE WAS AN ERROR
            }else{
                Random rand = new Random();
                {
                    PlanetGenerator pg = new PlanetGenerator();
                    List<(int,int)> planetLocations = new List<(int, int)>();
                    for(int i =0; i < 40 + playerCount*2;i++){
                        Planet p = pg.MakePlanet();
                        (int,int) xy;
                        bool redo = false;
                        int sanity=0;
                        do{
                            xy = (rand.Next(3840), rand.Next(2160));
                            sanity++;
                            for(int j=0;j<planetLocations.Count;j++){
                                if(Math.Abs(planetLocations[j].Item1 - xy.Item1) + Math.Abs(planetLocations[j].Item2 - xy.Item2) > 30+i*3-sanity){
                                    redo = true;
                                    break;
                                }
                            }
                        }while(redo);
                        p.x = xy.Item1;
                        p.y = xy.Item2;
                        if(i< 10 || i > 40 + playerCount) p.isImportant = true;
                        planets.Add(p);
                    }
                }
                dBConnection.MakePlanetsState(galaxyName,planets.ToArray());
                AddPlanetConnections(galaxyName, dBConnection);
            }
        }

        static void AddPlanetConnections(string galaxyName, IDBConnection dBConnection){
            Planet[] planets;
            dBConnection.GetPlanetsState(galaxyName, out planets);

            List<(int,int)> toAdd = new List<(int, int)>();

            foreach(Planet p in planets){
                var closeFive = p.FindClosestFivePlanetIDs(planets);
                foreach(int cf in closeFive){
                    if(cf < p.planetID){
                        if(!toAdd.Contains((cf,p.planetID))) toAdd.Add((cf,p.planetID));
                    }else{
                        if(!toAdd.Contains((p.planetID,cf))) toAdd.Add((p.planetID,cf));
                    }
                }
            }
            dBConnection.MakeConnectionsState(galaxyName, toAdd.ToArray());
        }
    }
}
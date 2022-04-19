using System;
using System.Collections.Generic;
using System.Linq;

namespace WebsiteBackend
{
    public class Planet{
        public string name, description;
        public int x, y;
        public bool isImportant;
        public int planetID;//ASSIGNED BY MySQL! DO NOT MANUALLY ASSIGN
        public int currentOwner;
        public int fleetOwner;

        public List<int> FindClosestFivePlanetIDs(Planet[] allPlanets){
            List<(int,int)?> closestFive = new List<(int,int)?>();//Tuple is index, distance
            for(int k=0;k<allPlanets.Length;k++){
                int distance = Math.Abs(allPlanets[k].x-x)+Math.Abs(allPlanets[k].y-y);
                bool doAdd = false;
                if(distance == 0) continue;
                int i=4;
                for(i=4;i>=0;i--){
                    if(closestFive[i]== null){
                        doAdd = true;
                        continue;
                    }
                    if(closestFive[i].Value.Item2 < distance){
                        break;
                    }else{
                        doAdd = true;
                    }
                }
                if (doAdd){
                    closestFive.Insert(i,(k,distance));
                }
            }
            List<int> retval = new List<int>();
            for(int i=0;i<5;i++){
                retval.Add(closestFive[i].Value.Item1);
            }
            return retval;
        }
    }
}
using System;

namespace WebsiteBackend{
    public record Order{
        public enum orderType: sbyte{M,S,C,D}
        //Hold is useless and is not included
        //M is Move. Fleet at Loc1 goes to Loc2
        //S is Support. Fleet at Loc1 supports fleet at loc3 attacking loc2
        //C is is create. Loc1 is marked for fleet creation
        //D is Destroy. Mark fleet for destruction if important system is lost.
        public orderType order;
        public int location1{get; init;}
        public int location2{get; init;}
        public int location3{get; init;}
        public int turn{get; init;}
        public int playerID{get; init;}
    }
}
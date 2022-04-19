
using System.Collections.Generic;

namespace WebsiteBackend{
    public class Galaxy{
        public string name;
        public int turn;
        public Planet[] planets;
        public (int,int)[] connections;
    }
}
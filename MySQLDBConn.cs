using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Options;
using MySql.Data.MySqlClient;
using System.Linq;

namespace WebsiteBackend
{
    public class MySQLDBConn : IDBConnection
    {
        private IJwtAuthenticationManager jwtAuth;

        private readonly DBLogin dBLogin;
        private readonly string dbConnString;


        public MySQLDBConn(IOptions<DBLogin> dbLogin, IOptions<IJwtAuthenticationManager> jwtAuth){
            this.dBLogin = dbLogin.Value ?? throw new ArgumentException(nameof(DBLogin));
            this.jwtAuth = jwtAuth.Value ?? throw new ArgumentException(nameof(IJwtAuthenticationManager));
            Console.WriteLine(this.dBLogin.password + this.dBLogin.username);
            dbConnString = "server=localhost;userid="+dBLogin.username+";password="+dBLogin.password+"database=spacediplomacy";
        }

        public bool AddPlayer(string galaxy, string playerName, string plaintextPassword, string factionName, out int playerID)
        {
            try{
                bool retval;
                using(var con = new MySqlConnection(dbConnString)){
                    using(MySqlCommand command = new MySqlCommand()){
                        command.Connection = con;
                        var salt = CryptoFunctions.CreateSalt();
                        string password = CryptoFunctions.CreatePassword(plaintextPassword,salt.ToString());
                        command.CommandText = $"INSERT INTO players(username, password, salt, factionName, connectedGalaxyName) VALUES({playerName}, {password}, {salt}, {factionName}, {galaxy})";
                        command.ExecuteNonQuery();
                        command.CommandText = $"SELECT id FROM players WHERE username={playerName}";
                        retval = int.TryParse(command.ExecuteScalar().ToString(),out playerID);
                        con.Close();
                    }
                }
                return retval;
            }catch{
                playerID = -1;
                return false;
            }
        }

        public bool EndGalaxy(string galaxy)
        {
            try{
                using(var con = new MySqlConnection(dbConnString)){
                    con.Open();
                    using(MySqlCommand command = new MySqlCommand()){
                        command.Connection = con;
                        command.CommandText = $"DELETE FROM planets WHERE connectedGalaxyName={galaxy}";
                        command.ExecuteNonQuery();
                        command.CommandText = $"DELETE FROM players WHERE connectedGalaxyName={galaxy}";
                        command.ExecuteNonQuery();
                        command.CommandText = $"UPDATE galaxies SET turn=-1, playerCount=0 WHERE name={galaxy}";
                        command.ExecuteNonQuery();
                        con.Close();
                    }
                }
                return true;
            }catch{
                return false;
            }
        }

        public bool FindPlayerCount(string galaxy, out int playerCount, out int maxPlayers)
        {
            try{
                using(var con = new MySqlConnection(dbConnString)){
                    con.Open();
                    using(MySqlCommand command = new MySqlCommand()){
                        command.Connection = con;
                        command.CommandText = $"SELECT playerCount,maxPlayers FROM galaxies WHERE name={galaxy}";
                        MySqlDataReader result = command.ExecuteReader();
                        result.Read();
                        int.TryParse(result.GetString(0),out playerCount);
                        result.Read();
                        int.TryParse(result.GetString(1),out maxPlayers);
                        result.Close();
                        con.Close();
                    }
                }
                return true;
            }catch{
                playerCount = -1;
                maxPlayers = -1;
                return false;
            }
        }

        public bool GetAllOrders(string galaxy, int turn, out Order[] orders)
        {
            try{
                var ordersList = new List<Order>();
                using(var con = new MySqlConnection(dbConnString)){
                    con.Open();
                    using(MySqlCommand command = new MySqlCommand()){
                        command.Connection = con;
                        //Use finding all players in a galaxy to find all orders connected to each player
                        command.CommandText = $"SELECT location1,location2,location3,actionType,playerID FROM orders WHERE turn = {turn} AND orders.playerID IN(SELECT playerID FROM players WHERE player.connectedGalaxy={galaxy})";
                        MySqlDataReader reader = command.ExecuteReader();
                        try{
                            while(reader.Read()){
                                ordersList.Add(new Order(){location1=reader.GetInt32(0),location2=reader.GetInt32(1),location3=reader.GetInt32(2),order=(Order.orderType)reader.GetSByte(3),playerID=reader.GetInt32(4),turn=turn,});
                            }
                        }finally{
                            reader.Close();
                            con.Close();
                        }
                    }
                }
                orders = ordersList.ToArray();
                return true;
            }catch{
                orders = null;
                return false;
            }
        }

        public bool GetOrders(int playerID, int turn, out Order[] orders)
        {
            try{
                var tempOrders = new List<Order>();
                using(var con = new MySqlConnection(dbConnString)){
                    con.Open();
                    using(MySqlCommand command = new MySqlCommand()){
                        command.Connection = con;
                        command.CommandText = $"SELECT location1,location2,location3,actionType FROM orders WHERE turn={turn} ANDorders.playerID={playerID})";
                        MySqlDataReader reader = command.ExecuteReader();
                        try{
                            while(reader.Read()){
                                tempOrders.Add(new Order(){location1=reader.GetInt32(0),location2=reader.GetInt32(1),location3=reader.GetInt32(2),order=(Order.orderType)reader.GetSByte(3),playerID=playerID,turn=turn,});
                            }
                        }finally{
                            reader.Close();
                            con.Close();
                        }
                    }
                    con.Close();
                }
                orders = tempOrders.ToArray();
                return true;
            }catch{
                orders = null;
                return false;
            }
        }

        public bool GetPlanetsState(string galaxy, out Planet[] planets)
        {
            try{
                List<Planet> retList = new List<Planet>();
                using(var con = new MySqlConnection(dbConnString)){
                    con.Open();
                    using(MySqlCommand command = new MySqlCommand()){
                        command.Connection = con;
                        command.CommandText = $"SELECT planetID, x, y, currentOwner, isImportant, fleetOwner, name,description FROM planets WHERE connectedGalaxyName={galaxy})";
                        MySqlDataReader reader = command.ExecuteReader();
                        try{
                            while(reader.Read()){
                                retList.Add(new Planet(){planetID=reader.GetInt32(0),x=reader.GetInt32(1),y=reader.GetInt32(2),currentOwner=reader.GetInt32(3),isImportant=reader.GetBoolean(4),fleetOwner=reader.GetInt32(5),name=reader.GetString(6),description=reader.GetString(7)});
                            }
                        }finally{
                            reader.Close();
                            con.Close();
                        }
                    }
                    con.Close();
                }
                planets = retList.ToArray();
                return true;
            }catch{
                planets = null;
                return false;
            }
        }

        public bool UpdateOrders(string galaxy, int playerName, int turn, Order[] orders)
        {
            try{
                using(var con = new MySqlConnection(dbConnString)){
                    con.Open();
                    using(MySqlCommand command = new MySqlCommand()){
                        command.Connection = con;
                        command.CommandText = $"DELETE FROM orders WHERE orders.playerID={playerName}";
                        command.ExecuteNonQuery();
                        StringBuilder sb = new StringBuilder("INSERT INTO orders(playerID, location1, location2, location3, actionType, turn) VALUES");
                        for(int i=0;i<orders.Length; i++){
                            if(i!=0) sb.Append(",");
                            sb.Append($"({orders[i].playerID}, {orders[i].location1}, {orders[i].location2}, {orders[i].location3}, {(sbyte)orders[i].order}, {orders[i].turn})");
                        }
                        command.CommandText = sb.ToString();
                        command.ExecuteNonQuery();
                    }
                    con.Close();
                }
                return true;
            }catch{
                return false;
            }
        }
    

        public bool FindPlayerCount(string galaxy, out int count)
        {
            try{
                using(var con = new MySqlConnection(dbConnString)){
                    con.Open();
                    using(MySqlCommand command = new MySqlCommand()){
                        command.Connection = con;
                        command.CommandText = $"SELECT playerCount FROM galaxies WHERE name='{galaxy}'";
                        if(!int.TryParse(command.ExecuteScalar().ToString(),out count)) throw new Exception();
                    }
                    con.Close();
                }
                return true;
            }catch{
                count = -1;
                return false;
            }
        }

        public bool MakeDBIfNoneExists()
        {
            try{
                using(var con = new MySqlConnection("server=localhost;userid="+dBLogin.username+";password="+dBLogin.password+"database=master")){
                    con.Open();
                    using (MySqlCommand command = new MySqlCommand("CREATE DATABASE IF NOT EXISTS SpaceDiplomacy", con)){
                        command.CommandText = "CREATE TABLE IF NOT EXISTS galaxies(turn INTEGER DEFAULT -1, PRIMARY KEY name VARCHAR(30), playerCount INTEGER, maxPlayers INTEGER), turnLength TINYINT, lastTurnTick DATETIME";
                        command.ExecuteNonQuery();
                        command.CommandText = $"CREATE TABLE IF NOT EXISTS players(id INTEGER PRIMARY KEY AUTO_INCREMENT, username UNIQUE VARCHAR[30], password UNIQUE VARCHAR[128], salt INTEGER, factionName VARCHAR[30], connectedGalaxyName VARCHAR(30) NOT NULL, FOREIGN KEY(connectedGalaxyName) REFERENCES galaxies(name) ON DELETE CASCADE)";//TODO Add jwt? Don't save username password as plaintext
                        command.ExecuteNonQuery();
                        command.CommandText = $"CREATE TABLE IF NOT EXISTS planets(id INTEGER PRIMARY KEY AUTO_INCREMENT, x INTEGER NOT NULL, y INTEGER NOT NULL, currentOwner INTEGER, FOREIGN KEY(currentOwner) REFERENCES players(id) ON DELETE CASCADE, isImportant BOOLEAN DEFAULT FALSE, fleetOwner INTEGER, FOREIGN KEY(fleetOwner) REFERENCES players(id) ON DELETE NULL, connectedGalaxyName VARCHAR(30) NOT NULL, FOREIGN KEY(connectedGalaxyName) REFERENCES galaxies(name) ON DELETE CASCADE,name TEXT, description TEXT)";//TODO Add jwt? Don't save username password as plaintext
                        command.ExecuteNonQuery();
                        command.CommandText = $"CREATE TABLE IF NOT EXISTS orders(id INTEGER PRIMARY KEY AUTO_INCREMENT, playerID INTEGER NOT NULL, FOREIGN KEY(playerID) REFERENCES players(id) ON DELETE CASCADE, location1 INTEGER, location2 INTEGER, location3 INTEGER, FOREIGN KEY(location1) REFERENCES planets(id) ON DELETE CASCADE, FOREIGN KEY(location2) REFERENCES planets(id) ON DELETE CASCADE, FOREIGN KEY(location3) REFERENCES planets(id) ON DELETE CASCADE, actionType TINYINT DEFAULT 0, turn INTEGER NOT NULL))";//TODO Add jwt? Don't save username password as plaintext
                        command.ExecuteNonQuery();
                        command.CommandText = $"CREATE TABLE IF NOT EXISTS planetConnections(galaxyID VARCHAR(30), FOREIGN KEY (galaxyID) REFERENCES galaxies(name) ON DELETE CASCADE, planetID INTEGER NOT NULL, FOREIGN KEY (planetID) REFERENCES planets(id) ON DELETE CASCADE, connectedPlanetID INTEGER NOT NULL, FOREIGN KEY (connectedPlanetID) REFERENCES planets(id) ON DELETE CASCADE, PRIMARY KEY(galaxyID, planetID, connectedPlanetID)))";//TODO Add jwt? Don't save username password as plaintext
                        command.ExecuteNonQuery();
                        command.CommandText = "INSERT INTO galaxies(name,playerCount,maxPlayers) VALUES('Andromeda',0,0),('Butterfly',0,0),('Circinus',0,0),('Eye_of_Sauron',0,0),('Fireworks',0,0),('Hockey_Stick',0,0),('Hoag's_Galaxy',0,0),('Magellan',0,0),('Sombrero',0,0),('Medusa',0,0),('Sculptor',0,0),('Needle',0,0),('Pinwheel',0,0),('Sunflower',0,0),('Tadpole',0,0),('Triangulum',0,0),('Whirlpool',0,0),('Antennae',0,0),('Black_Eye',0,0),('Cigar',0,0),('Cosmos',0,0),('Malin',0,0)";
                        command.ExecuteNonQuery();

                    }
                    con.Close();
                }
                return true;
            }catch{
                return false;
            }
        }

        public bool FindUnusedGalaxy(out string galaxy)
        {
            try{
            using(var con = new MySqlConnection(dbConnString)){
                con.Open();
                using(MySqlCommand command = new MySqlCommand()){
                    command.Connection = con;
                    command.CommandText = "SELECT name FROM galaxies WHERE turn=-1 LIMIT 1";
                    galaxy = command.ExecuteScalar().ToString();
                    return true;
                }
            }
            }catch{
                galaxy = null;
                return false;
            }
        }

        public bool ChangePlanetsState(string galaxy, Planet[] changedPlanets)
        {
            try{
            using(var con = new MySqlConnection(dbConnString)){
                con.Open();
                using(MySqlCommand command = new MySqlCommand()){
                    command.Connection = con;
                    for(int i=0;i<changedPlanets.Length;i++){
                        command.CommandText = $"UPDATE planets SET currentOwner='{changedPlanets[i].currentOwner}', FROM planets WHERE id='{changedPlanets[i].planetID}'";
                        command.ExecuteNonQuery();
                    }
                    return true;
                }
            }
            }catch{
                galaxy = null;
                return false;
            }
        }

        public bool MakePlanetsState(string galaxy, Planet[] planets)
        {
            if(!FindPlayerCount(galaxy, out int ct ) || ct !=0){
                Console.WriteLine("Error, galaxy already in use; cannot make galaxy state");
                return false;
            }

            try{
            using(var con = new MySqlConnection(dbConnString)){
                con.Open();
                using(MySqlCommand command = new MySqlCommand()){
                    command.Connection = con;
                    System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder("INSERT INTO planets(x,y,isImportant,fleetOwner,connectedGalaxyName,name,description) VALUES");
                    List<(int,int)> planetConnections = new List<(int, int)>();
                    for(int i=0;i<planets.Length;i++){
                        stringBuilder.Append($"({planets[i].x},{planets[i].y},{planets[i].currentOwner},{planets[i].isImportant},{planets[i].fleetOwner},'{galaxy}','{planets[i].name}','{planets[i].description}')");
                    }
                    command.CommandText = stringBuilder.ToString();
                    command.ExecuteNonQuery();
                    return true;
                }
            }
            }catch{
                galaxy = null;
                return false;
            }
        }
        public bool MakeConnectionsState(string galaxy, (int,int)[] connections)
        {
            if(!FindPlayerCount(galaxy, out int ct ) || ct !=0){
                Console.WriteLine("Error, galaxy already in use; cannot make galaxy state");
                return false;
            }

            try{
            using(var con = new MySqlConnection(dbConnString)){
                con.Open();
                using(MySqlCommand command = new MySqlCommand()){
                    command.Connection = con;
                    System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder("INSERT INTO planetConnections(galaxyID, planetID,connectedPlanetID) VALUES");
                    for(int i=0;i<connections.Length;i++){
                        stringBuilder.Append($"('{galaxy}',{connections[i].Item1},{connections[i].Item2})");
                    }
                    command.CommandText = stringBuilder.ToString();
                    command.ExecuteNonQuery();
                    return true;
                }
            }
            }catch{
                galaxy = null;
                return false;
            }
        }

        public bool CheckUsernamePassword(string username, string plaintextPassword, out int playerID, out bool usernameExists, out bool correctPassword, out string galaxyName){
            correctPassword = false;
            try{
                using(var con = new MySqlConnection(dbConnString)){
                    con.Open();
                    using(MySqlCommand command = new MySqlCommand()){
                        command.Connection = con;
                        command.CommandText = $"SELECT id, password, salt, connectedGalaxyName FROM players WHERE username='{username}'";
                        MySqlDataReader reader = command.ExecuteReader();
                        try{
                            usernameExists = true;
                            reader.Read();
                            int potentialID = reader.GetInt32("id");
                            string password = reader.GetString("password");
                            int salt = reader.GetInt32("salt");
                            galaxyName = reader.GetString("connectedGalaxyName");
                            if(CryptoFunctions.ComparePassword(CryptoFunctions.CreatePassword(plaintextPassword,salt.ToString()),password)){
                                playerID = potentialID;
                                reader.Close();
                                con.Close();
                                correctPassword = true;
                                return true;
                            }
                        }catch{
                            usernameExists = false;
                            Console.WriteLine("INVALID");
                            playerID = -1;
                            reader.Close();
                            con.Close();
                            galaxyName="";
                            return false;
                        }
                    }
                }
            }catch{
                playerID=0;
                usernameExists = false;
                galaxyName="";
                return false;
            }
            playerID=0;
            return false;
        }

        public bool GetCurrentTurn(string galaxyName, out int turn)
        {
            try{
                using(var con = new MySqlConnection(dbConnString)){
                    con.Open();
                    using(MySqlCommand command = new MySqlCommand()){
                        command.Connection = con;
                        command.CommandText = $"SELECT turn FROM galaxies WHERE name='{galaxyName}'";
                        if(!int.TryParse(command.ExecuteScalar().ToString(), out turn)) throw new Exception();
                    }
                    con.Close();
                }
                return true;
            }catch{
                turn = -5;
                return false;
            }
        }

        public bool GetConnections(string galaxy, out (int, int)[] connections)
        {
            try{
                List<(int,int)> retList = new List<(int,int)>();
                using(var con = new MySqlConnection(dbConnString)){
                    con.Open();
                    using(MySqlCommand command = new MySqlCommand()){
                        command.Connection = con;
                        command.CommandText = $"SELECT planetID, connectedPlanet FROM planetConnections WHERE connectedGalaxyName={galaxy})";
                        MySqlDataReader reader = command.ExecuteReader();
                        try{
                            while(reader.Read()){
                                retList.Add((reader.GetInt32("planetID"),reader.GetInt32("connectedPlanet")));
                            }
                        }finally{
                            reader.Close();
                            con.Close();
                        }
                    }
                    con.Close();
                }
                connections = retList.ToArray();
                return true;
            }catch{
                connections = null;
                return false;
            }
        }

        public bool IterateGalaxy(string galaxyName, out bool gameEnded, out int winner){
            gameEnded = false;
            winner = 0;
            Order[] orders;
            Planet[] planets;
            (int,int)[] connections;

            Dictionary<int,Planet> planetDict = new Dictionary<int, Planet>();
            //planetID, list of planet fleet origins, owners, and supporter count            

            if(!GetConnections(galaxyName, out connections))
                return false;
            int turn;
            if(!GetCurrentTurn(galaxyName, out turn))
                return false;
            if(!GetAllOrders(galaxyName,turn, out orders))
                return false;
            if(!GetPlanetsState(galaxyName,out planets))
                return false;
            foreach(Planet planet in planets){
                planetDict.Add(planet.planetID,planet);
            }


            {
                Dictionary<int,List<(int, int, int)>> planetAssaults = new Dictionary<int, List<(int, int, int)>>();

                List<Order> activeOrdersRemaining = orders.Where(x => x.order == Order.orderType.M).ToList();
                activeOrdersRemaining.AddRange(orders.Where(x => x.order == Order.orderType.S));
                

                while(activeOrdersRemaining.Count > 0){
                    Order currentOrder = activeOrdersRemaining[0];
                    activeOrdersRemaining.RemoveAt(0);
                    if(currentOrder.order == Order.orderType.M){
                        Planet p1,p2;
                        if(!(planetDict.TryGetValue(currentOrder.location1,out p1)))
                            continue;
                        if(!(planetDict.TryGetValue(currentOrder.location2,out p2)))
                            continue;

                        //Verify the planets are connected
                        if(p1.currentOwner != currentOrder.playerID || !(p1.planetID<p2.planetID)?connections.Contains((p1.planetID,p2.planetID)):connections.Contains((p2.planetID,p1.planetID)))
                            continue;


                        List<(int,int,int)> currentAssaultedPlanet;
                        if(planetAssaults.TryGetValue(currentOrder.location2, out currentAssaultedPlanet)){
                            currentAssaultedPlanet.Add((p1.planetID,p1.fleetOwner,1));
                        }else{
                            planetAssaults.Add(currentOrder.location2, new List<(int, int, int)>{(p1.planetID,p1.fleetOwner,0)});
                        }
                    }else{
                        Planet p1,p2,p3;
                        if(!(planetDict.TryGetValue(currentOrder.location1,out p1)))
                            continue;
                        if(!(planetDict.TryGetValue(currentOrder.location2,out p2)))
                            continue;
                        if(!(planetDict.TryGetValue(currentOrder.location2,out p3)))
                            continue;

                        //Verify planets are properly connected
                        if(p1.currentOwner != currentOrder.playerID || !(p1.planetID<p2.planetID)?connections.Contains((p1.planetID,p2.planetID)):connections.Contains((p2.planetID,p1.planetID)) || !(p3.planetID<p2.planetID)?connections.Contains((p3.planetID,p2.planetID)):connections.Contains((p2.planetID,p3.planetID)))
                            continue;
                        
                        List<(int,int,int)> currentAssaultedPlanet;
                        if(planetAssaults.TryGetValue(currentOrder.location2, out currentAssaultedPlanet)){
                            int tupleLocation;
                            if((tupleLocation = currentAssaultedPlanet.FindIndex(0, x => x.Item1 == currentOrder.location3)) == -1)
                                continue;
                            currentAssaultedPlanet[tupleLocation] = (currentAssaultedPlanet[tupleLocation].Item1,p2.planetID,currentAssaultedPlanet[tupleLocation].Item3+1);
                        }
                        //Else, no assault to support and nothing needs to be done
                    }
                }

            }

            //Increment turn
            try{
                using(var con = new MySqlConnection(dbConnString)){
                    con.Open();
                    using(MySqlCommand command = new MySqlCommand()){
                        command.Connection = con;
                        command.CommandText = $"UPDATE galaxies SET turn={turn+1} WHERE name='{galaxyName}'";
                        command.ExecuteNonQuery();
                        con.Close();
                    }
                }
            }catch{
                return false;
            }

            //Determine if there is a winner
            {
                Dictionary<int,int> playerScore = new Dictionary<int, int>();//<Player ID, important system count>
                foreach(Planet p in planets){
                    if(!p.isImportant) continue;
                    if(playerScore.ContainsKey(p.currentOwner)){
                        playerScore[p.currentOwner]++;
                        if(playerScore[p.currentOwner] > planets.Length + 23){
                            gameEnded = true;
                            winner = p.currentOwner;
                        }
                    }else
                        playerScore.Add(p.currentOwner,1);
                }
            }
            return true;
        }
    }
}


using System.Collections.Generic;

namespace WebsiteBackend
{
    public interface IDBConnection{
        bool AddPlayer(string galaxy, string playerName, string plaintextPassword, string factionName, out int playerID);
        bool EndGalaxy(string galaxy);
        bool FindPlayerCount(string galaxy, out int playerCount, out int maxPlayers);
        bool GetAllOrders(string galaxy, int turn, out Order[] orders);
        bool GetOrders(int playerID, int turn, out Order[] orders);
        bool UpdateOrders(string galaxy, int playerName, int turn, Order[] orders);
        bool FindPlayerCount(string galaxy, out int count);
        bool MakeDBIfNoneExists();
        bool FindUnusedGalaxy(out string galaxy);
        bool ChangePlanetsState(string galaxy, Planet[] changedPlanets);
        bool CheckUsernamePassword(string username, string plaintextPassword, out int playerID, out bool usernameExists, out bool passwordCorrect, out string galaxyName);
        bool MakePlanetsState(string galaxy, Planet[] changedPlanets);
        bool MakeConnectionsState(string galaxy, (int,int)[] connections);
        bool GetConnections(string galaxy, out (int,int)[] connections);
        bool GetPlanetsState(string galaxyName, out Planet[] planets);
        bool GetCurrentTurn(string galaxyName, out int turn);
    }
}
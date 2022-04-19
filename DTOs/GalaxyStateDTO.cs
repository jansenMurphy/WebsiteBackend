namespace WebsiteBackend.DTOs{
    public class GalaxyStateDTO{
        public Planet[] planets;
        public Order[] playerOrders,lastTurnOrders;

        public GalaxyStateDTO(Planet[] planets, Order[] playerOrders, Order[] lastTurnOrders){
            this.planets = planets;
            this.playerOrders = playerOrders;
            this.lastTurnOrders = lastTurnOrders;
        }
    }
}
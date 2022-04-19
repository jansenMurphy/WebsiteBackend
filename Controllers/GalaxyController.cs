using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace WebsiteBackend{

    [Authorize]
    [Route("galaxies")]
    [ApiController]
    public class GalaxyController : ControllerBase{

        private IJwtAuthenticationManager jwtAM;
        private IDBConnection dbLogin;

        public GalaxyController(IOptions<JWTAuthenticationManager> jwtAM, IOptions<IDBConnection> dbLogin)
        {
            if(jwtAM == null || dbLogin == null){
                Console.WriteLine("PANIC");
                return;
            }
            this.dbLogin = dbLogin.Value;
            this.jwtAM = jwtAM.Value;
        }

        [HttpGet]
        public ActionResult<DTOs.GetGalaxyStateDTO> GetWorldState(){
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if(identity != null){
                Planet[] planets;
                int id;
                string galaxyName = identity.FindFirst( o => o.Type == ClaimTypes.Role)?.Value;
                string playerName = identity.FindFirst( o => o.Type == ClaimTypes.Name)?.Value;
                if(int.TryParse(identity.FindFirst( o => o.Type == ClaimTypes.SerialNumber)?.Value, out id)){      
                    int turn;
                    if(dbLogin.GetCurrentTurn(galaxyName, out turn)){      
                        if(dbLogin.GetPlanetsState(galaxyName, out planets)){
                            Order[] currentOrders,lastTurnOrders;
                            if(dbLogin.GetOrders(id,turn,out currentOrders)){
                                if(turn <=1){
                                    if(dbLogin.GetAllOrders(galaxyName,turn-1,out lastTurnOrders))
                                        return StatusCode(200,new DTOs.GalaxyStateDTO(planets,currentOrders,lastTurnOrders));
                                }
                            }
                        }
                    }
                }
                return StatusCode(500);
                

            }else{
                return StatusCode(401);
            }
            //return NotFound(); //If no orders exist
        }

        [HttpPost]
        public ActionResult<DTOs.PostUpdateOrdersDTO> UpdateOrders(DTOs.PostUpdateOrdersDTO createOrders){
            var identity = HttpContext.User.Identity as ClaimsIdentity;
            if(identity == null)
                return StatusCode(400,"User Authentication Error. Try logging out and back in");
            Planet[] planets;
            int id;
            string galaxyName = identity.FindFirst( o => o.Type == ClaimTypes.Role)?.Value;
            string playerName = identity.FindFirst( o => o.Type == ClaimTypes.Name)?.Value;
            (int,int)[] connections;
            int turn;


            if(int.TryParse(identity.FindFirst( o => o.Type == ClaimTypes.SerialNumber)?.Value, out id))
                return StatusCode(500,"Database Error. Try again and contact owner if probelm persists");

            if(!dbLogin.GetCurrentTurn(galaxyName, out turn))
                return StatusCode(500,"Database Error. Try again and contact owner if probelm persists");

            if(!dbLogin.GetPlanetsState(galaxyName, out planets))
                return StatusCode(500,"Database Error. Try again and contact owner if probelm persists");

            if(!dbLogin.GetConnections(galaxyName,out connections))
                return StatusCode(500,"Database Error. Try again and contact owner if probelm persists");

            //verify orders are valid
            foreach(Order order in createOrders.orders){
                Planet pl1,pl2,pl3;
                //Get planets listed in orders
                try{
                    pl1 = planets.Where(planet => planet.planetID.Equals(order.location1)).Single();
                }catch{
                    return StatusCode(400, "Invalid orders");
                }
                try{
                    pl2 = planets.Where(planet => planet.planetID.Equals(order.location2)).Single();
                }catch{
                    pl2=null;
                }
                try{
                    pl3 = planets.Where(planet => planet.planetID.Equals(order.location3)).Single();
                }catch{
                    pl3 = null;
                }


                //Find if planets are logically placed
                switch(order.order){
                    case (Order.orderType.C):
                        if(pl1.currentOwner != id)
                            return StatusCode(400, "Invalid orders");
                        break;
                    case (Order.orderType.D):
                        if(pl1.fleetOwner != id)
                            return StatusCode(400, "Invalid orders");
                        break;
                    case (Order.orderType.M):
                        if(pl1.currentOwner != id || !(pl1.planetID<pl2.planetID)?connections.Contains((pl1.planetID,pl2.planetID)):connections.Contains((pl2.planetID,pl1.planetID)))
                            return StatusCode(400, "Invalid orders");
                        break;
                    case (Order.orderType.S):
                        if(pl1.currentOwner != id || !(pl1.planetID<pl2.planetID)?connections.Contains((pl1.planetID,pl2.planetID)):connections.Contains((pl2.planetID,pl1.planetID))
                        || !(pl3.planetID<pl2.planetID)?connections.Contains((pl3.planetID,pl2.planetID)):connections.Contains((pl2.planetID,pl3.planetID)))
                            return StatusCode(400, "Invalid orders");
                        break;
                }
            }


            //Apply orders
            if(!dbLogin.UpdateOrders(galaxyName,id,turn,createOrders.orders))
                return StatusCode(500,"Database Error. Try again and contact owner if problem persists");
            return StatusCode(204, "Orders updated");
        }
    }
}
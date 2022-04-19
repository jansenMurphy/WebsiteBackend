using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace WebsiteBackend{


    [Route("api/logins")]
    [ApiController]
    public class LoginController : ControllerBase{
        private readonly IJwtAuthenticationManager jwtAuth;
        private readonly IDBConnection dbConnection;

        public LoginController(IOptions<IDBConnection> dbConnection, IOptions<IJwtAuthenticationManager> jwtAuth){
            this.dbConnection = dbConnection.Value ?? throw new ArgumentException(nameof(IDBConnection));
            this.jwtAuth = jwtAuth.Value ?? throw new ArgumentException(nameof(IJwtAuthenticationManager));
        }

        [AllowAnonymous]
        [HttpPost("create")]
        public IActionResult CreateGalaxy([FromBody] DTOs.CreateGalaxyAttempt cgaDTO){
            bool playerAlreadyExists, passwordCorrect;
            int playerID;
            string galaxyName;
            dbConnection.CheckUsernamePassword(cgaDTO.username,cgaDTO.password, out playerID, out playerAlreadyExists, out passwordCorrect, out galaxyName);
            if(playerAlreadyExists){
                return Forbid("Username already in use; cannot create new galaxy with this username");
            }

            string unusedGalaxy;
            if(!dbConnection.FindUnusedGalaxy(out unusedGalaxy)){
                Console.WriteLine("ERROR");
                return StatusCode(500,"Can't find unused galaxy");
            }

            var pg = new PlanetGenerator();
            Planet[] planets = new Planet[1];
            planets[0] = pg.MakePlanet();
            dbConnection.MakePlanetsState(unusedGalaxy, planets);
            string tokenAuth = jwtAuth.Authenticate(cgaDTO.username, cgaDTO.password, unusedGalaxy, playerID);
            
            return StatusCode(200,tokenAuth);
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] DTOs.LoginPlayerAttemptDTO lpaDTO){
            if(lpaDTO.username == null)
                return StatusCode(400, "Username cannot be null");
            bool playerAlreadyExists, passwordCorrect;
            int playerID;
            string galaxyName;
            dbConnection.CheckUsernamePassword(lpaDTO.username,lpaDTO.password, out playerID, out playerAlreadyExists, out passwordCorrect, out galaxyName);
            if(playerAlreadyExists){
                if(passwordCorrect){
                    string tokenAuth = jwtAuth.Authenticate(lpaDTO.username, lpaDTO.password, galaxyName, playerID);
                    return StatusCode(200,tokenAuth);
                }else{
                    return BadRequest("Incorrect password for this username");
                }
            }else{
                int currentPlayers, maxPlayers, id;
                dbConnection.FindPlayerCount(lpaDTO.galaxy,out currentPlayers, out maxPlayers);
                if(currentPlayers < maxPlayers){
                    dbConnection.AddPlayer(lpaDTO.galaxy,lpaDTO.username,lpaDTO.password, "", out id);//TODO Faction name
                    string tokenAuth = jwtAuth.Authenticate(lpaDTO.username, lpaDTO.password,lpaDTO.galaxy,id);
                    return StatusCode(200,tokenAuth);
                }else{
                    return BadRequest("Galaxy is either full or not yet created");
                }
            }
        }
    }
}
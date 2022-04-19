using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace WebsiteBackend{
    public class JWTToken
    {
        public string Password {get;set;}
    }
}
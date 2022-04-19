using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace WebsiteBackend{
    public class JWTAuthenticationManager : IJwtAuthenticationManager{

        private string key;

        public JWTAuthenticationManager(IOptions<JWTToken> token)
        {
            this.key = token.Value.Password ?? throw new ArgumentException(nameof(JWTToken));
        }
        
        public string Authenticate(string username, string password, string galaxyName, int id){
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenKey = System.Text.Encoding.UTF8.GetBytes(key);
            var tokenDescriptor = new SecurityTokenDescriptor{
                Subject = new ClaimsIdentity(new Claim[]{
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, galaxyName),
                    new Claim(ClaimTypes.SerialNumber,id.ToString())
                }),
                Expires = System.DateTime.UtcNow.AddDays(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenKey), SecurityAlgorithms.HmacSha512Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            //Write token to handler
            return tokenHandler.WriteToken(token);
        }
    }
}
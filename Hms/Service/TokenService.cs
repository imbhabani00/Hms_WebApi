using Hms.Constants;
using Hms.Service.Response.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Hms.Service
{
    public interface ITokenService
    {
        string GenerateAccessToken(UserResponse user, DateTime expires);
        string GenerateRefreshToken(UserResponse user);
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }

    public class TokenService : ITokenService
    {
        #region Proprties
        private readonly IConfiguration _configuration;
        private readonly string _issuer;
        private readonly byte[] _secretKey;
        #endregion

        #region Constructor
        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
            _issuer = _configuration.GetValue<string>("JWTSettings:Issuer");
            _secretKey = Encoding.UTF8.GetBytes(
                _configuration.GetValue<string>("JWTSettings:JWTSecretKey")
            );
        }
        #endregion

        #region GenerateAccessToken
        public string GenerateAccessToken(UserResponse user, DateTime expires)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FirstName ?? string.Empty),
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Role, user.RoleName ?? string.Empty),
                new Claim(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
                new Claim(ClaimTypes.Surname, user.LastName ?? string.Empty),
                new Claim(CustomClaimTypesConstants.TenantId, user.TenantId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _issuer,
                Audience = null,
                Expires = expires,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(_secretKey),
                    SecurityAlgorithms.HmacSha256
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
        #endregion

        #region GenerateRefreshToken
        public string GenerateRefreshToken(UserResponse user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(CustomClaimTypesConstants.TenantId, user.TenantId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var refreshExpiryDays =
                _configuration.GetValue<int>("JWTSettings:RefreshTokenExpiryInDays");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Issuer = _issuer,
                Expires = DateTime.UtcNow.AddDays(refreshExpiryDays),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(_secretKey),
                    SecurityAlgorithms.HmacSha256
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
        #endregion


        #region
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _issuer,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(_secretKey),
                ValidateLifetime = false 
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(
                token,
                tokenValidationParameters,
                out SecurityToken securityToken
            );

            if (securityToken is not JwtSecurityToken jwtToken ||
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                    StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return principal;
        }
    }
    #endregion
}
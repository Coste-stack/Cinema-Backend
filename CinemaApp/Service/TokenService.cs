using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using CinemaApp.Model;

namespace CinemaApp.Service;

public interface ITokenService
{
    AuthTokenDTO GenerateToken(User user, int? minutes = null);
}

public class TokenService(IConfiguration config) : ITokenService
{
    private readonly IConfiguration _config = config;

    public AuthTokenDTO GenerateToken(User user, int? minutes = null)
    {
        int effectiveMinutes;
        if (minutes != null && minutes.Value > 0) {
            effectiveMinutes = minutes.Value;
        } else {
            var jwtSection = _config.GetSection("Jwt");
            var expiryMinutesString = jwtSection["ExpiryMinutes"] ?? "60";
            if (!int.TryParse(expiryMinutesString, out var expiryMinutes)) expiryMinutes = 60;
            effectiveMinutes = expiryMinutes;
        }

        var token = _GenerateToken(user, effectiveMinutes);
        return new AuthTokenDTO { 
            Token = token, 
            ExpiresAt = DateTime.UtcNow.AddMinutes(effectiveMinutes) 
        };
    }

    private string _GenerateToken(User user, int expiryMinutes)
    {
        var jwtSection = _config.GetSection("Jwt");
        var secret = jwtSection["Secret"] ?? throw new Exception("JWT secret not configured");
        var issuer = jwtSection["Issuer"] ?? "CinemaApp";
        var audience = jwtSection["Audience"] ?? "CinemaAppClients";

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("userType", user.UserType.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

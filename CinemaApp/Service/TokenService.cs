using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using CinemaApp.Model;

namespace CinemaApp.Service;

public interface ITokenService
{
    string GenerateToken(User user);
}

public class TokenService(IConfiguration config) : ITokenService
{
    private readonly IConfiguration _config = config;

    public string GenerateToken(User user)
    {
        var jwtSection = _config.GetSection("Jwt");
        var secret = jwtSection["Secret"] ?? throw new Exception("JWT secret not configured");
        var issuer = jwtSection["Issuer"] ?? "CinemaApp";
        var audience = jwtSection["Audience"] ?? "CinemaAppClients";
        var expiryMinutesString = jwtSection["ExpiryMinutes"] ?? "60";
        if (!int.TryParse(expiryMinutesString, out var expiryMinutes)) expiryMinutes = 60;

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

using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using CinemaApp.Filters;

namespace CinemaApp.Controller;

[ApiController]
[Route("[controller]")]
public class AuthController(IUserService userService, IPasswordHasher<User> passwordHasher, ITokenService tokenService) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;

    [HttpPost("login")]
    [ServiceFilter(typeof(TurnstileFilter))]
    public async Task<ActionResult<AuthResponseDTO>> Login([FromBody] LoginRequestDTO request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        // Validate request
        var user = _userService.Get(request.Email);
        if (user == null || string.IsNullOrEmpty(user.PasswordHash)) 
        {
            return Unauthorized(new { message = "Invalid credentials" });  
        }

        // Validate password
        var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Success || result == PasswordVerificationResult.SuccessRehashNeeded)
        {
            AuthTokenDTO authToken = _tokenService.GenerateToken(user);
            RefreshToken refreshToken = _tokenService.GenerateRefreshToken();

            // Persist refresh token for the user safely (modify tokens only)
            _userService.AddRefreshToken(user.Id, refreshToken);

            var response = new AuthResponseDTO { Token = authToken, RefreshToken = refreshToken };
            return Ok(response);
        }

        return Unauthorized(new { message = "Invalid credentials" });
    }

    [HttpPost("register")]
    [ServiceFilter(typeof(TurnstileFilter))]
    public async Task<ActionResult<AuthResponseDTO>> Register([FromBody] UserCreateDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = _userService.Add(dto);

        AuthTokenDTO authToken = _tokenService.GenerateToken(user);
        RefreshToken refreshToken = _tokenService.GenerateRefreshToken();

        // Persist refresh token for the new user
        _userService.AddRefreshToken(user.Id, refreshToken);

        var response = new AuthResponseDTO { Token = authToken, RefreshToken = refreshToken };
        return CreatedAtAction("GetById", "User", new { id = user.Id }, response);
    }

    [HttpPost("refresh-token")]
    public ActionResult<AuthResponseDTO> RefreshToken([FromBody] RefreshToken refreshToken)
    {
        // Lookup user by refresh token
        var user = _userService.Get(refreshToken);
        if (user == null)
            return Unauthorized("Invalid refresh token.");

        // Get the token record
        var tokenRecord = user.RefreshTokens.FirstOrDefault(rt => rt.Token == refreshToken.Token);
        if (tokenRecord == null || tokenRecord.ExpiresAt < DateTime.UtcNow)
            return Unauthorized("Refresh token expired or invalid.");

        // Revoke old token to prevent reuse
        _userService.InvalidateRefreshToken(user.Id, tokenRecord.Token);

        // Generate new access token
        AuthTokenDTO newAccessToken = _tokenService.GenerateToken(user);

        // Generate new refresh token
        RefreshToken newRefreshToken = _tokenService.GenerateRefreshToken();
        _userService.AddRefreshToken(user.Id, newRefreshToken);

        // Return tokens
        var response = new AuthResponseDTO
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken
        };

        return Ok(response);
    }
}

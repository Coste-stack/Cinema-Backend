using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;

namespace CinemaApp.Controller;

[ApiController]
[Route("[controller]")]
public class AuthController(IUserService userService, IPasswordHasher<User> passwordHasher, ITokenService tokenService, IConfiguration config) : ControllerBase
{
    private readonly IUserService _userService = userService;
    private readonly IPasswordHasher<User> _passwordHasher = passwordHasher;
    private readonly ITokenService _tokenService = tokenService;
    private readonly IConfiguration _config = config;

    

    [HttpPost("login")]
    public ActionResult<AuthResponseDTO> Login([FromBody] LoginRequestDTO request)
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
            AuthTokenDTO tokenDTO = _tokenService.GenerateToken(user);
            var response = new AuthResponseDTO { Token = tokenDTO.Token, ExpiresAt = tokenDTO.ExpiresAt };
            return Ok(response);
        }

        return Unauthorized(new { message = "Invalid credentials" });
    }

    [HttpPost("register")]
    public ActionResult<AuthResponseDTO> Register([FromBody] UserCreateDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = _userService.Add(dto);

        AuthTokenDTO tokenDTO = _tokenService.GenerateToken(user);
        var response = new AuthResponseDTO { Token = tokenDTO.Token, ExpiresAt = tokenDTO.ExpiresAt };
        return CreatedAtAction("GetById", "User", new { id = user.Id }, response);
    }
}

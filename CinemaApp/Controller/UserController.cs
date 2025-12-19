using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using CinemaApp.Filters;
using CinemaApp.Model;
using CinemaApp.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Controller;

[ApiController]
[Route("[controller]")]
public class UserController(IUserService service) : ControllerBase
{
    private readonly IUserService _service = service;

    [HttpGet]
    public ActionResult<List<UserResponseDTO>> GetAll()
    {
        var users = _service.GetAll();
        return users.ToResponse();
    }


    [HttpGet("{id:int}")]
    public ActionResult<UserResponseDTO> GetById(int id)
    {
        var user = _service.Get(id);
        if (user == null) throw new NotFoundException("User not found");
        return user.ToResponse();
    }
    
    [HttpGet("by-email")]
    public ActionResult<UserResponseDTO> GetByEmail(string email)
    {
        var user = _service.Get(email);
        if (user == null) throw new NotFoundException("User not found");
        return user.ToResponse();
    }

    [HttpGet("current-email")]
    [Authorize]
    public ActionResult<string> GetUserEmail()
    {
        var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(sub, out var userId)) return Forbid();

        var user = _service.Get(userId);
        if (user == null) throw new NotFoundException("User not found");
        if (user.Email == null) throw new NotFoundException("Email not found");

        return Ok(user.Email);
    }

    [HttpPut("{id:int}")]
    public ActionResult Update(int id, [FromBody] UserCreateDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _service.Update(id, dto);
        return NoContent();  
    }

    [HttpPut("password")]
    [Authorize]
    [ServiceFilter(typeof(TurnstileFilter))]
    public ActionResult UpdatePassword([FromBody] string password)
    {
        var sub = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
            ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(sub, out var userId)) return Forbid();

        _service.UpdatePassword(userId, password);
        return NoContent();  
    }
}
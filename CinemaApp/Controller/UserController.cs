using CinemaApp.Model;
using CinemaApp.Service;
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
    
    [HttpGet("email")]
    public ActionResult<UserResponseDTO> GetByEmail(string email)
    {
        var user = _service.Get(email);
        if (user == null) throw new NotFoundException("User not found");
        return user.ToResponse();
    }

    [HttpPost("register")]
    public ActionResult Register([FromBody] UserCreateDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = _service.Add(dto);
        var response = user.ToResponse();
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, response);
    }

    [HttpPut("{id:int}")]
    public ActionResult Update(int id, [FromBody] UserCreateDTO dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        _service.Update(id, dto);
        return NoContent();  
    }
}
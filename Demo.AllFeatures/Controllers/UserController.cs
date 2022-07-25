using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Demo.AllFeatures.Data.Dtos;
using Demo.AllFeatures.Services;

namespace Demo.AllFeatures.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost()]
    public async Task<IActionResult> Create([FromBody] CreateUser body)
    {
        try
        {
            var createdUser = await _userService.CreateUserAsync(body);

            return Created(nameof(Create), createdUser);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        try
        {
            var user = await _userService.FindUserViaIdAsync(id);

            if (user is null) return NotFound();

            return Ok(user);
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}


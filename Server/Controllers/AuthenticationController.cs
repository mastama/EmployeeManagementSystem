using BaseLibrary.DTOs;
using Microsoft.AspNetCore.Mvc;
using ServiceLibrary.Repositories.Contracts;

namespace EmployeeManagementService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IUserAccount _userAccount;

    public AuthenticationController(IUserAccount userAccount)
    {
        _userAccount = userAccount;
    }
    
    [HttpGet]
    public IActionResult Test()
    {
        return Ok("Controller is working.");
    }

    [HttpPost("register")]
    public async Task<IActionResult> CreateAsync(Register user)
    {
        if (user == null) return BadRequest("Model is empty");
        var result = await _userAccount.CreateAsync(user);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> SignInAsync(Login user)
    {
        if (user == null)
        {
            return BadRequest("Model is empty");
        }
        var result = await _userAccount.SignInAsync(user);
        return Ok(result);
    }
}
using EventMS.Auth.Application.Contracts;
using EventMS.Auth.Application.DTO;
using EventMS.Auth.Contracts;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventMS.Auth.Presentation.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
public class AuthController(IAuthService authService, IValidator<UserLoginRequest>  validator) : ControllerBase
{

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserLoginRequest request, CancellationToken ct)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
        {
            return BadRequest(validationResult.Errors.Select(e => new
            {
                e.PropertyName,
                e.ErrorMessage
            }));
        }

        UsersRole role = (UsersRole)request.Role;

        await authService.RegisterAsync(request.Login, request.Password, role, ct);

        return Ok();
    }


    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginRequest model, CancellationToken ct)
    {
        var tokenString = await authService.LoginAsync(model.Login, model.Password, ct);

        return Ok(new { Token = tokenString });
    }
}
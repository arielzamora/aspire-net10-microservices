using Core.MediatOR.Contracts;
using MasterNet.Application.Accounts.GetCurrentUser;
using MasterNet.WebApi.Models;
using MasterNet.Application.Accounts.Register;
using MasterNet.Application.Interfaces;
using MasterNet.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using static MasterNet.Application.Accounts.GetCurrentUser.GetCurrentUserQuery;
using static MasterNet.Application.Accounts.Register.RegisterCommand;
using static MasterNet.Application.Accounts.Login.LoginCommand;

namespace MasterNet.WebApi.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController : ControllerBase
{

    private readonly IMediatOR _sender;
    private readonly IUserAccessor _user;

    public AccountController(IMediatOR sender, IUserAccessor user)
    {
        _sender = sender;
        _user = user;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new LoginCommandRequest(request.Email, request.Password);
        var result = await _sender.Send(command, cancellationToken);
        //return result.IsSuccess ? Ok(result.Value) : Unauthorized();
        return this.FromResult(result);
    }


    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> Register(
        [FromForm] RegisterRequest request,
        CancellationToken cancellationToken
    )
    {
        var command = new RegisterCommandRequest(request);
        var result = await _sender.Send(command, cancellationToken);
        return this.FromResult(result);
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType((int)HttpStatusCode.OK)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var email = _user.GetEmail();
        var request = new GetCurrentUserRequest { Email = email };
        var query = new GetCurrentUserQueryRequest(request);
        var result = await _sender.Send(query, cancellationToken);
        return this.FromResult(result);
    }
}
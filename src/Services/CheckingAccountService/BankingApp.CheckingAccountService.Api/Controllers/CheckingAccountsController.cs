using BankingApp.CheckingAccountService.Api.Extensions;
using BankingApp.CheckingAccountService.Application.Commands;
using BankingApp.CheckingAccountService.Application.DTOs;
using BankingApp.CheckingAccountService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.CheckingAccountService.Api.Controllers;

[ApiController]
[Route("api/checking-accounts")]
public class CheckingAccountsController : ControllerBase
{
    private readonly IMediator _mediator;

    public CheckingAccountsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CreateAccountResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(
        [FromBody] CreateAccountRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey = null)
    {
        var command = new CreateAccountCommand(request.Cpf, request.Name, request.Password);
        var result = await _mediator.Send(command);

        return CreatedAtAction(nameof(GetBalance), new { }, result);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request.Identifier, request.Password);
        var result = await _mediator.Send(command);

        return Ok(result);
    }

    [HttpDelete("deactivate")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Deactivate([FromBody] DeactivateAccountRequest request)
    {
        var accountId = User.GetAccountId();
        var command = new DeactivateAccountCommand(accountId, request.Password);

        await _mediator.Send(command);

        return NoContent();
    }

    [HttpPost("movements")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateMovement(
        [FromBody] CreateMovementRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey = null)
    {
        var accountId = User.GetAccountId();
        var command = new CreateMovementCommand(
            accountId,
            request.AccountNumber,
            request.Amount,
            request.MovementType
        );

        await _mediator.Send(command);

        return NoContent();
    }

    [HttpGet("balance")]
    [Authorize]
    [ProducesResponseType(typeof(BalanceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBalance()
    {
        var accountId = User.GetAccountId();
        var query = new GetBalanceQuery(accountId);

        var result = await _mediator.Send(query);

        return Ok(result);
    }

    [HttpGet("by-number/{accountNumber}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AccountDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByAccountNumber(long accountNumber)
    {
        var query = new GetAccountByNumberQuery(accountNumber);
        var result = await _mediator.Send(query);

        return Ok(result);
    }
}
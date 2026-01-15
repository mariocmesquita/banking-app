using BankingApp.TransferService.Api.Extensions;
using BankingApp.TransferService.Application.Commands;
using BankingApp.TransferService.Application.DTOs;
using BankingApp.TransferService.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BankingApp.TransferService.Api.Controllers;

[ApiController]
[Route("api/transfers")]
[Authorize]
public class TransfersController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransfersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateTransfer(
        [FromBody] CreateTransferRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey = null)
    {
        var accountId = User.GetAccountId();
        var jwtToken = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        var command = new CreateTransferCommand(
            accountId,
            request.DestinationAccountNumber,
            request.Amount,
            jwtToken
        );

        await _mediator.Send(command);

        return NoContent();
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TransferDetailsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTransfer(Guid id)
    {
        var query = new GetTransferQuery(id);
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound(new { errorCode = "TRANSFER_NOT_FOUND", message = "Transferência não encontrada" });

        return Ok(result);
    }
}
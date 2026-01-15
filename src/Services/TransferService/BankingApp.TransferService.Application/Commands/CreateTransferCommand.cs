using BankingApp.TransferService.Application.DTOs;
using MediatR;

namespace BankingApp.TransferService.Application.Commands;

public record CreateTransferCommand(
    Guid OriginAccountId,
    long DestinationAccountNumber,
    decimal Amount,
    string JwtToken
) : IRequest<CreateTransferResponse>;
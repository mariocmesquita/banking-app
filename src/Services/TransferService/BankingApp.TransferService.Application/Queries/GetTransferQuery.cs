using BankingApp.TransferService.Application.DTOs;
using MediatR;

namespace BankingApp.TransferService.Application.Queries;

public record GetTransferQuery(Guid TransferId) : IRequest<TransferDetailsResponse?>;
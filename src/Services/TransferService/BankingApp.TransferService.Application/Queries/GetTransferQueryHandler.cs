using BankingApp.TransferService.Application.DTOs;
using BankingApp.TransferService.Domain.Interfaces;
using MediatR;

namespace BankingApp.TransferService.Application.Queries;

public class GetTransferQueryHandler : IRequestHandler<GetTransferQuery, TransferDetailsResponse?>
{
    private readonly ITransferRepository _transferRepository;

    public GetTransferQueryHandler(ITransferRepository transferRepository)
    {
        _transferRepository = transferRepository;
    }

    public async Task<TransferDetailsResponse?> Handle(
        GetTransferQuery request,
        CancellationToken cancellationToken)
    {
        var transfer = await _transferRepository.GetByIdAsync(request.TransferId);

        if (transfer == null)
            return null;

        return new TransferDetailsResponse(
            transfer.Id,
            transfer.OriginAccountId,
            transfer.DestinationAccountId,
            transfer.MovementDate,
            transfer.Amount,
            transfer.Status.ToString()
        );
    }
}
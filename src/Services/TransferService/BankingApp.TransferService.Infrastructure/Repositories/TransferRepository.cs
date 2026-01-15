using System.Reflection;
using BankingApp.TransferService.Domain.Entities;
using BankingApp.TransferService.Domain.Enums;
using BankingApp.TransferService.Domain.Interfaces;
using BankingApp.TransferService.Infrastructure.Database;
using Dapper;

namespace BankingApp.TransferService.Infrastructure.Repositories;

public class TransferRepository : ITransferRepository
{
    private readonly DapperContext _context;

    public TransferRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<Transfer?> GetByIdAsync(Guid id)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            SELECT
                id_transfer,
                id_checking_account_origin,
                id_checking_account_destination,
                movement_date,
                amount,
                status
            FROM transfer
            WHERE id_transfer = @Id
        ";

        var row = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id.ToString() });

        return row == null ? null : MapToDomain(row);
    }

    public async Task AddAsync(Transfer transfer)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            INSERT INTO transfer (
                id_transfer,
                id_checking_account_origin,
                id_checking_account_destination,
                movement_date,
                amount,
                status
            ) VALUES (
                @Id,
                @OriginAccountId,
                @DestinationAccountId,
                @MovementDate,
                @Amount,
                @Status
            )
        ";

        await connection.ExecuteAsync(sql, new
        {
            Id = transfer.Id.ToString(),
            OriginAccountId = transfer.OriginAccountId.ToString(),
            DestinationAccountId = transfer.DestinationAccountId.ToString(),
            MovementDate = transfer.MovementDate.ToString("O"),
            transfer.Amount,
            Status = (int)transfer.Status
        });
    }

    private static Transfer MapToDomain(dynamic row)
    {
        var transfer = (Transfer)Activator.CreateInstance(
            typeof(Transfer),
            BindingFlags.NonPublic | BindingFlags.Instance,
            null, null, null)!;

        typeof(Transfer).GetProperty(nameof(Transfer.Id))!
            .SetValue(transfer, Guid.Parse((string)row.id_transfer));

        typeof(Transfer).GetProperty(nameof(Transfer.OriginAccountId))!
            .SetValue(transfer, Guid.Parse((string)row.id_checking_account_origin));

        typeof(Transfer).GetProperty(nameof(Transfer.DestinationAccountId))!
            .SetValue(transfer, Guid.Parse((string)row.id_checking_account_destination));

        typeof(Transfer).GetProperty(nameof(Transfer.MovementDate))!
            .SetValue(transfer, DateTime.Parse((string)row.movement_date));

        typeof(Transfer).GetProperty(nameof(Transfer.Amount))!
            .SetValue(transfer, (decimal)(double)row.amount);

        typeof(Transfer).GetProperty(nameof(Transfer.Status))!
            .SetValue(transfer, (TransferStatus)(int)row.status);

        return transfer;
    }
}
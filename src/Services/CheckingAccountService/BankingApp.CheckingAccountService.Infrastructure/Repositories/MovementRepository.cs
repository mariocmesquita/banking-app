using System.Reflection;
using BankingApp.CheckingAccountService.Domain.Entities;
using BankingApp.CheckingAccountService.Domain.Enums;
using BankingApp.CheckingAccountService.Domain.Interfaces;
using BankingApp.CheckingAccountService.Infrastructure.Database;
using Dapper;

namespace BankingApp.CheckingAccountService.Infrastructure.Repositories;

public class MovementRepository : IMovementRepository
{
    private readonly DapperContext _context;

    public MovementRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Movement movement)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            INSERT INTO movement (id_movement, id_checking_account, movement_date, movement_type, amount)
            VALUES (@Id, @CheckingAccountId, @MovementDate, @MovementType, @Amount)
        ";

        await connection.ExecuteAsync(sql, new
        {
            Id = movement.Id.ToString(),
            CheckingAccountId = movement.CheckingAccountId.ToString(),
            MovementDate = movement.MovementDate.ToString("O"), // ISO 8601
            MovementType = ((char)movement.MovementType).ToString(),
            movement.Amount
        });
    }

    public async Task<decimal> GetBalanceAsync(Guid accountId)
    {
        using var connection = _context.CreateConnection();

        // Calcula saldo: soma de créditos - soma de débitos
        var sql = @"
            SELECT
                COALESCE(SUM(CASE WHEN movement_type = 'C' THEN amount ELSE 0 END), 0) -
                COALESCE(SUM(CASE WHEN movement_type = 'D' THEN amount ELSE 0 END), 0) as balance
            FROM movement
            WHERE id_checking_account = @AccountId
        ";

        var balance = await connection.QuerySingleAsync<decimal>(sql, new { AccountId = accountId.ToString() });

        return balance;
    }

    private static Movement MapToDomain(dynamic row)
    {
        var movement = (Movement)Activator.CreateInstance(
            typeof(Movement),
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            null,
            null)!;

        typeof(Movement).GetProperty(nameof(Movement.Id))!
            .SetValue(movement, Guid.Parse((string)row.id_movement));

        typeof(Movement).GetProperty(nameof(Movement.CheckingAccountId))!
            .SetValue(movement, Guid.Parse((string)row.id_checking_account));

        typeof(Movement).GetProperty(nameof(Movement.MovementDate))!
            .SetValue(movement, DateTime.Parse((string)row.movement_date));

        var movementTypeChar = ((string)row.movement_type)[0];
        var movementType = movementTypeChar == 'C' ? MovementType.Credit : MovementType.Debit;
        typeof(Movement).GetProperty(nameof(Movement.MovementType))!
            .SetValue(movement, movementType);

        typeof(Movement).GetProperty(nameof(Movement.Amount))!
            .SetValue(movement, Convert.ToDecimal(row.amount));

        return movement;
    }
}
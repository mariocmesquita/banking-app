using BankingApp.FeeService.Domain.Entities;
using BankingApp.FeeService.Domain.Interfaces;
using BankingApp.FeeService.Infrastructure.Database;
using Dapper;

namespace BankingApp.FeeService.Infrastructure.Repositories;

public class FeeRepository : IFeeRepository
{
    private readonly DapperContext _context;

    public FeeRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Fee fee)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            INSERT INTO fee (
                id_fee,
                id_checking_account,
                movement_date,
                amount
            ) VALUES (
                @Id,
                @CheckingAccountId,
                @MovementDate,
                @Amount
            )
        ";

        await connection.ExecuteAsync(sql, new
        {
            Id = fee.Id.ToString(),
            CheckingAccountId = fee.CheckingAccountId.ToString(),
            MovementDate = fee.MovementDate.ToString("O"),
            fee.Amount
        });
    }
}
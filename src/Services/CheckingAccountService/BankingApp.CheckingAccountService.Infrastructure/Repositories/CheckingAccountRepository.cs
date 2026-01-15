using System.Reflection;
using BankingApp.CheckingAccountService.Domain.Entities;
using BankingApp.CheckingAccountService.Domain.Interfaces;
using BankingApp.CheckingAccountService.Domain.ValueObjects;
using BankingApp.CheckingAccountService.Infrastructure.Database;
using Dapper;

namespace BankingApp.CheckingAccountService.Infrastructure.Repositories;

public class CheckingAccountRepository : ICheckingAccountRepository
{
    private readonly DapperContext _context;

    public CheckingAccountRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<CheckingAccount?> GetByIdAsync(Guid id)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            SELECT id_checking_account, account_number, cpf, name, active, password, salt
            FROM checking_account
            WHERE id_checking_account = @Id
        ";

        var row = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Id = id.ToString() });

        return row != null ? MapToDomain(row) : null;
    }

    public async Task<CheckingAccount?> GetByAccountNumberAsync(long accountNumber)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            SELECT id_checking_account, account_number, cpf, name, active, password, salt
            FROM checking_account
            WHERE account_number = @AccountNumber
        ";

        var row = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { AccountNumber = accountNumber });

        return row != null ? MapToDomain(row) : null;
    }

    public async Task<CheckingAccount?> GetByCpfAsync(string cpf)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            SELECT id_checking_account, account_number, cpf, name, active, password, salt
            FROM checking_account
            WHERE cpf = @Cpf
        ";

        var row = await connection.QueryFirstOrDefaultAsync<dynamic>(sql, new { Cpf = cpf });

        return row != null ? MapToDomain(row) : null;
    }

    public async Task AddAsync(CheckingAccount account)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            INSERT INTO checking_account (id_checking_account, account_number, cpf, name, active, password, salt)
            VALUES (@Id, @AccountNumber, @Cpf, @Name, @Active, @Password, @Salt)
        ";

        await connection.ExecuteAsync(sql, new
        {
            Id = account.Id.ToString(),
            AccountNumber = account.AccountNumber.Value,
            Cpf = account.Cpf.Value,
            account.Name,
            Active = account.Active ? 1 : 0,
            Password = account.PasswordHash,
            account.Salt
        });
    }

    public async Task UpdateAsync(CheckingAccount account)
    {
        using var connection = _context.CreateConnection();

        var sql = @"
            UPDATE checking_account
            SET name = @Name,
                active = @Active,
                password = @Password,
                salt = @Salt
            WHERE id_checking_account = @Id
        ";

        await connection.ExecuteAsync(sql, new
        {
            Id = account.Id.ToString(),
            account.Name,
            Active = account.Active ? 1 : 0,
            Password = account.PasswordHash,
            account.Salt
        });
    }

    private static CheckingAccount MapToDomain(dynamic row)
    {
        var account = (CheckingAccount)Activator.CreateInstance(
            typeof(CheckingAccount),
            BindingFlags.NonPublic | BindingFlags.Instance,
            null,
            null,
            null)!;

        typeof(CheckingAccount).GetProperty(nameof(CheckingAccount.Id))!
            .SetValue(account, Guid.Parse((string)row.id_checking_account));

        typeof(CheckingAccount).GetProperty(nameof(CheckingAccount.AccountNumber))!
            .SetValue(account, new AccountNumber((long)row.account_number));

        typeof(CheckingAccount).GetProperty(nameof(CheckingAccount.Cpf))!
            .SetValue(account, new Cpf((string)row.cpf));

        typeof(CheckingAccount).GetProperty(nameof(CheckingAccount.Name))!
            .SetValue(account, (string)row.name);

        typeof(CheckingAccount).GetProperty(nameof(CheckingAccount.Active))!
            .SetValue(account, (long)row.active == 1);

        typeof(CheckingAccount).GetProperty(nameof(CheckingAccount.PasswordHash))!
            .SetValue(account, (string)row.password);

        typeof(CheckingAccount).GetProperty(nameof(CheckingAccount.Salt))!
            .SetValue(account, (string)row.salt);

        return account;
    }
}
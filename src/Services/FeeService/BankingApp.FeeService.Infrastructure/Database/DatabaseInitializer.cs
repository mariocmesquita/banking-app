using Dapper;

namespace BankingApp.FeeService.Infrastructure.Database;

public class DatabaseInitializer
{
    private readonly DapperContext _context;

    public DatabaseInitializer(DapperContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync()
    {
        using var connection = _context.CreateConnection();

        var createFeeTable = @"
            CREATE TABLE IF NOT EXISTS fee (
                id_fee TEXT(37) PRIMARY KEY,
                id_checking_account TEXT(37) NOT NULL,
                movement_date TEXT(25) NOT NULL,
                amount REAL NOT NULL,
                FOREIGN KEY(id_fee) REFERENCES fee(id_fee)
            );
        ";

        var createIdempotencyTable = @"
            CREATE TABLE IF NOT EXISTS idempotency (
                idempotency_key TEXT(100) PRIMARY KEY,
                processed_at TEXT(25) NOT NULL
            );
        ";

        var createIndexes = @"
            CREATE INDEX IF NOT EXISTS IX_Fee_Account ON fee(id_checking_account);
            CREATE INDEX IF NOT EXISTS IX_Fee_Date ON fee(movement_date);
        ";

        await connection.ExecuteAsync(createFeeTable);
        await connection.ExecuteAsync(createIdempotencyTable);
        await connection.ExecuteAsync(createIndexes);
    }
}
using Dapper;

namespace BankingApp.TransferService.Infrastructure.Database;

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

        var createTransferTable = @"
            CREATE TABLE IF NOT EXISTS transfer (
                id_transfer TEXT(37) PRIMARY KEY,
                id_checking_account_origin TEXT(37) NOT NULL,
                id_checking_account_destination TEXT(37) NOT NULL,
                movement_date TEXT(25) NOT NULL,
                amount REAL NOT NULL,
                status INTEGER NOT NULL DEFAULT 0,
                FOREIGN KEY(id_transfer) REFERENCES transfer(id_transfer)
            );
        ";

        var createIdempotencyTable = @"
            CREATE TABLE IF NOT EXISTS idempotency (
                idempotency_key TEXT(37) PRIMARY KEY,
                request TEXT(1000),
                result TEXT(1000)
            );
        ";

        var createIndexes = @"
            CREATE INDEX IF NOT EXISTS IX_Transfer_Origin ON transfer(id_checking_account_origin);
            CREATE INDEX IF NOT EXISTS IX_Transfer_Destination ON transfer(id_checking_account_destination);
            CREATE INDEX IF NOT EXISTS IX_Transfer_Date ON transfer(movement_date);
        ";

        await connection.ExecuteAsync(createTransferTable);
        await connection.ExecuteAsync(createIdempotencyTable);
        await connection.ExecuteAsync(createIndexes);
    }
}
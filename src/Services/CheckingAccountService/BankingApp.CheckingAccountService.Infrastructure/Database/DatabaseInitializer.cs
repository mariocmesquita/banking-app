using Dapper;

namespace BankingApp.CheckingAccountService.Infrastructure.Database;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(DapperContext context)
    {
        using var connection = context.CreateConnection();
        connection.Open();

        // Cria tabela checking_account seguindo EXATAMENTE a estrutura do .sql
        var createCheckingAccountTable = @"
            CREATE TABLE IF NOT EXISTS checking_account (
                id_checking_account TEXT(37) PRIMARY KEY,
                account_number INTEGER(10) NOT NULL UNIQUE,
                cpf TEXT(11) NOT NULL UNIQUE,
                name TEXT(100) NOT NULL,
                active INTEGER(1) NOT NULL default 1,
                password TEXT(100) NOT NULL,
                salt TEXT(100) NOT NULL,
                CHECK (active in (0,1))
            );
        ";

        // Cria tabela movement
        var createMovementTable = @"
            CREATE TABLE IF NOT EXISTS movement (
                id_movement TEXT(37) PRIMARY KEY,
                id_checking_account TEXT(37) NOT NULL,
                movement_date TEXT(25) NOT NULL,
                movement_type TEXT(1) NOT NULL,
                amount REAL NOT NULL,
                CHECK (movement_type in ('C','D')),
                FOREIGN KEY(id_checking_account) REFERENCES checking_account(id_checking_account)
            );
        ";

        // Cria tabela idempotency
        var createIdempotencyTable = @"
            CREATE TABLE IF NOT EXISTS idempotency (
                idempotency_key TEXT(37) PRIMARY KEY,
                request TEXT(1000),
                result TEXT(1000)
            );
        ";

        // Cria índices
        var createIndexes = @"
            CREATE INDEX IF NOT EXISTS IX_CheckingAccount_AccountNumber ON checking_account(account_number);
            CREATE INDEX IF NOT EXISTS IX_CheckingAccount_Cpf ON checking_account(cpf);
            CREATE INDEX IF NOT EXISTS IX_Movement_AccountId ON movement(id_checking_account);
            CREATE INDEX IF NOT EXISTS IX_Movement_Date ON movement(movement_date);
            CREATE INDEX IF NOT EXISTS IX_Movement_AccountId_Date ON movement(id_checking_account, movement_date);
        ";

        await connection.ExecuteAsync(createCheckingAccountTable);
        await connection.ExecuteAsync(createMovementTable);
        await connection.ExecuteAsync(createIdempotencyTable);
        await connection.ExecuteAsync(createIndexes);
    }
}
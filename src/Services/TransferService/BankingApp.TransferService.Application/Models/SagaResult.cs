namespace BankingApp.TransferService.Application.Models;

public class SagaResult
{
    private SagaResult() { }

    public bool IsSuccess { get; private set; }
    public bool WasRolledBack { get; private set; }
    public string? ErrorMessage { get; private set; }

    public static SagaResult Success()
    {
        return new SagaResult
        {
            IsSuccess = true,
            WasRolledBack = false,
            ErrorMessage = null
        };
    }

    public static SagaResult FailedWithRollback(string errorMessage)
    {
        return new SagaResult
        {
            IsSuccess = false,
            WasRolledBack = true,
            ErrorMessage = errorMessage
        };
    }

    public static SagaResult FailedWithoutRollback(string errorMessage)
    {
        return new SagaResult
        {
            IsSuccess = false,
            WasRolledBack = false,
            ErrorMessage = errorMessage
        };
    }

    public static SagaResult FailedAtDebit(string errorMessage)
    {
        return new SagaResult
        {
            IsSuccess = false,
            WasRolledBack = false,
            ErrorMessage = errorMessage
        };
    }
}

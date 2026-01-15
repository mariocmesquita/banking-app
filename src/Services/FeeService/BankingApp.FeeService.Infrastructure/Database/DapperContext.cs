using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

namespace BankingApp.FeeService.Infrastructure.Database;

public class DapperContext
{
    private readonly IConfiguration _configuration;

    public DapperContext(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IDbConnection CreateConnection()
    {
        var connectionString = _configuration.GetConnectionString("DefaultConnection");

        var dataSourcePrefix = "Data Source=";
        if (connectionString.StartsWith(dataSourcePrefix))
        {
            var fileName = connectionString.Substring(dataSourcePrefix.Length);
            var projectDirectory = Path.Combine(AppContext.BaseDirectory, "..", "..", "..");
            var absolutePath = Path.GetFullPath(Path.Combine(projectDirectory, fileName));
            connectionString = $"{dataSourcePrefix}{absolutePath}";
        }

        return new SqliteConnection(connectionString);
    }
}
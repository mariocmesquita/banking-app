using BankingApp.FeeService.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BankingApp.FeeService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton<FeeCalculationService>();
        return services;
    }
}
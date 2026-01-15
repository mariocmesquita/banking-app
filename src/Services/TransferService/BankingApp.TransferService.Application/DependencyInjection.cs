using BankingApp.TransferService.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace BankingApp.TransferService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly); });
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);
        services.AddScoped<TransferSagaOrchestrator>();

        return services;
    }
}
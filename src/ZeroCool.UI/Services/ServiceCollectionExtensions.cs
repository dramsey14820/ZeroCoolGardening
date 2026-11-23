using Microsoft.Extensions.DependencyInjection;
using ZeroCool.Data;

namespace ZeroCool.UI.Services;

public static class ServiceCollectionExtensions
{
    /// <summary>Registers repositories and the editing session. The host must register <c>IFileStore</c> (singleton).</summary>
    public static IServiceCollection AddZeroCoolUI(this IServiceCollection services)
    {
        services.AddScoped<PlanRepository>();
        services.AddScoped<SettingsRepository>();
        services.AddScoped<PlantCatalogRepository>();
        services.AddScoped<GardenSession>();
        return services;
    }
}


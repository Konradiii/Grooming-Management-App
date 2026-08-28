namespace Grooming_Management_App.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Rejestruje jedną implementację pod wieloma interfejsami.
    /// Wszystkie interfejsy wskazują na TĘ SAMĄ instancję w obrębie żądania.
    /// </summary>
    public static IServiceCollection AddScopedWithInterfaces<TImplementation>(
        this IServiceCollection services,
        params Type[] interfaces)
        where TImplementation : class
    {
        services.AddScoped<TImplementation>();

        foreach (var iface in interfaces)
        {
            services.AddScoped(iface, sp => sp.GetRequiredService<TImplementation>());
        }

        return services;
    }
}
using Microsoft.Extensions.DependencyInjection;
using Minnor.Core.Context;
using Minnor.Core.Models;

namespace Minnor.Core.Extensions;

public static class MinnorServiceCollectionExtensions
{
    public static IServiceCollection AddMinnor(this IServiceCollection services, Action<MinnorOptions> configure)
    {
        var options = new MinnorOptions();
        configure(options);

        if (ValidateConfigure(options))
            throw new ArgumentNullException(nameof(options), "Configuration action cannot be null.");

        services.AddSingleton(options);

        services.AddScoped<MinnorContext>(sp =>
        {
            var opts = sp.GetRequiredService<MinnorOptions>();
            return new MinnorContext(opts.ConnectionString!);
        });

        return services;
    }

    private static bool ValidateConfigure(MinnorOptions options)
    {
        return string.IsNullOrEmpty(options.ConnectionString);
    }
}

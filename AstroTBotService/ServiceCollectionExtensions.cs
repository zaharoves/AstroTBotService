using AstroCalculation;
using AstroCalculation.Interfaces;
using AstroCalculation.Configurations;

namespace AstroTBotService
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAstroCalculation(
            this IServiceCollection services,
            Action<AstroCalculationConfig>? configure = null)
        {
            if (configure != null)
                services.Configure(configure);

            services.AddOptions<AstroCalculationConfig>();

            services.AddSingleton<ISwissEphemerisService, SwissEphemerisService>();
            services.AddSingleton<ICommonHelper, CommonHelper>();
            services.AddScoped<IAstroCalculationService, AstroCalculationService>();

            return services;
        }
    }
}

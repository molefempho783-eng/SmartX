using Microsoft.Extensions.Logging;
using SmartX.Maui.Services;
using SmartX.Maui.Pages;

namespace SmartX.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        // Register the main application and default fonts (Microsoft Docs, 2026).
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register ApiService as singleton (Microsoft Docs, 2026).
        builder.Services.AddSingleton<ApiService>();

        // Register pages for DI resolution (Microsoft Docs, 2026).
        builder.Services.AddTransient<LandingPage>();
        builder.Services.AddTransient<SensorRegistrationPage>();
        builder.Services.AddTransient<TelemetryDashboardPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

/* Reference List
CommunityToolkit, 2024. MVVM Toolkit source generators. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators/observableproperty> [Accessed 13 September 2026].

Microsoft Docs, 2026. Dependency injection in .NET MAUI. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/dependency-injection> [Accessed 13 September 2026].

Microsoft Docs, 2026. .NET MAUI controls. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/> [Accessed 13 September 2026].
*/
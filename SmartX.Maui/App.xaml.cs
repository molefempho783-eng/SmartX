using Microsoft.Extensions.DependencyInjection;

namespace SmartX.Maui;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		// Every page in this app uses a hardcoded light palette, so the app is
		// pinned to the light theme. Without this, a device running dark mode
		// resolves the default AppThemeBinding styles to white Entry text on a
		// light background, making the registration form unreadable
		// (Microsoft Docs, 2026).
		UserAppTheme = AppTheme.Light;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		return new Window(new AppShell());
	}
}
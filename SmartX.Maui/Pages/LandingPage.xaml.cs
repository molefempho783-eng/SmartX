// Landing page code-behind with Shell navigation (Microsoft Docs, 2026).

namespace SmartX.Maui.Pages;

public partial class LandingPage : ContentPage
{
    public LandingPage()
    {
        InitializeComponent();
    }

    // Navigate to sensor registration page (IIE, 2026).
    private async void OnSensorIngestionTapped(object sender, TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("//SensorRegistrationPage");
    }

    // Button variant of the same navigation (Clicked passes EventArgs, the
    // TapGestureRecognizer passes TappedEventArgs, so they need separate
    // handlers) (Microsoft Docs, 2026).
    private async void OnRegisterNavClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//SensorRegistrationPage");
    }

    // Navigate to the live telemetry dashboard (IIE, 2026).
    private async void OnDashboardTapped(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//TelemetryDashboardPage");
    }
}

/* Reference List
IIE, 2026. PROG7312 POE. The Independent Institute of Education (Pty) Ltd.

Microsoft Docs, 2026. .NET MAUI Shell navigation. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/shell/navigation> [Accessed 12 September 2026].
*/
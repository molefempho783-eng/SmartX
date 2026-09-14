using CommunityToolkit.Mvvm.ComponentModel;

namespace SmartX.Maui.Models;

/// Represents a single telemetry log line, parsed into its component fields
/// so the dashboard can show the device and sensor separately (IIE, 2026).

public partial class LogEntry : ObservableObject
{
    // The original, unparsed line as returned by the API (IIE, 2026).
    [ObservableProperty]
    private string text = string.Empty;

    // Parsed fields shown as separate columns on the dashboard (IIE, 2026).
    [ObservableProperty]
    private string time = string.Empty;

    [ObservableProperty]
    private string deviceId = string.Empty;

    [ObservableProperty]
    private string sensorType = string.Empty;

    // Value plus unit, e.g. "55.5 %" (IIE, 2026).
    [ObservableProperty]
    private string reading = string.Empty;

    // Determines colour-coding (IIE, 2026).
    [ObservableProperty]
    private bool isAnomalous;

    // Severity level: Normal, Info, Critical (IIE, 2026).
    // The three colour properties below are derived from Severity, so changing
    // Severity must also raise PropertyChanged for them (CommunityToolkit, 2024).
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BackgroundColor))]
    [NotifyPropertyChangedFor(nameof(IndicatorColor))]
    [NotifyPropertyChangedFor(nameof(TextColor))]
    private string severity = "Normal";

    // Colour properties bound to XAML (Microsoft Docs, 2026).
    public Color BackgroundColor => Severity switch
    {
        "Critical" => Color.FromArgb("#2e1a1a"),
        "Info" => Color.FromArgb("#1a1e2e"),
        _ => Color.FromArgb("#1a2e1a")
    };

    public Color IndicatorColor => Severity switch
    {
        "Critical" => Color.FromArgb("#ef4444"),
        "Info" => Color.FromArgb("#3b82f6"),
        _ => Color.FromArgb("#10b981")
    };

    public Color TextColor => Severity switch
    {
        "Critical" => Color.FromArgb("#fca5a5"),
        "Info" => Color.FromArgb("#93c5fd"),
        _ => Color.FromArgb("#a5d6a7")
    };
}

/* Reference List
CommunityToolkit, 2024. MVVM Toolkit source generators. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators/observableproperty> [Accessed 12 September 2026].

IIE, 2026. PROG7312 Module Manual. The Independent Institute of Education (Pty) Ltd.

Microsoft Docs, 2026. Colors in .NET MAUI. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/maui/user-interface/graphics/colors> [Accessed 12 September 2026].
*/

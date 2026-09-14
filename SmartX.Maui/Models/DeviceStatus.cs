using CommunityToolkit.Mvvm.ComponentModel;

namespace SmartX.Maui.Models;

/// Rolled-up state for one physical device, derived from the telemetry log
/// so the operator can see the fleet at a glance rather than reading lines
/// one by one (IIE, 2026).

public partial class DeviceStatus : ObservableObject
{
    [ObservableProperty]
    private string deviceId = string.Empty;

    // Sensor type of the most recent reading (IIE, 2026).
    [ObservableProperty]
    private string lastSensor = string.Empty;

    // Most recent value plus unit, e.g. "55.5 %" (IIE, 2026).
    [ObservableProperty]
    private string lastReading = string.Empty;

    [ObservableProperty]
    private string lastSeen = string.Empty;

    // How many log lines this device contributed in the current window
    // (IIE, 2026).
    [ObservableProperty]
    private int readingCount;

    // True when any reading from this device breached a threshold (IIE, 2026).
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(StatusColor))]
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(StatusBackground))]
    private bool hasAlert;

    public Color StatusColor => HasAlert
        ? Color.FromArgb("#991b1b")
        : Color.FromArgb("#2e7d32");

    public Color StatusBackground => HasAlert
        ? Color.FromArgb("#F8CECC")
        : Color.FromArgb("#D5E8D4");

    public string StatusText => HasAlert ? "ALERT" : "OK";
}

/* Reference List
CommunityToolkit, 2024. MVVM Toolkit source generators. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators/observableproperty> [Accessed 13 September 2026].

IIE, 2026. PROG7312 POE. The Independent Institute of Education (Pty) Ltd.

Microsoft Docs, 2026. Bindable layouts in .NET MAUI. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/maui/user-interface/layouts/bindablelayout> [Accessed 13 September 2026].
*/

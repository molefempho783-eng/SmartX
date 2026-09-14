

using CommunityToolkit.Mvvm.ComponentModel;

namespace SmartX.Maui.Models;

/// Represents a registered sensor with its deployment metadata (IIE, 2026).

public partial class SensorRegistration : ObservableObject
{
    // ObservableProperty generates INotifyPropertyChanged automatically
    // (CommunityToolkit, 2024).
    [ObservableProperty]
    private string macAddress = string.Empty;

    [ObservableProperty]
    private string location = string.Empty;

    [ObservableProperty]
    private string category = string.Empty;

    [ObservableProperty]
    private DateTime registeredAt = DateTime.UtcNow;

    [ObservableProperty]
    private List<string> attachments = new();
}

/* Reference List
CommunityToolkit, 2024. MVVM Toolkit source generators. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/communitytoolkit/mvvm/generators/observableproperty> [Accessed 12 September 2026].

IIE, 2026. PROG7312 POE. The Independent Institute of Education (Pty) Ltd.

Microsoft Docs, 2026. Data binding in .NET MAUI. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/data-binding/> [Accessed 12 September 2026].
*/
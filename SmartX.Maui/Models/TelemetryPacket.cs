namespace SmartX.Maui.Models;

/// Client-side mirror of the API's generic telemetry wrapper (IIE, 2026).

public class TelemetryPacket<T>
{
    public string DeviceId { get; set; } = string.Empty;
    public string SensorType { get; set; } = string.Empty;

    // Generic property: T is resolved at compile time (Microsoft Docs, 2026).
    public T Value { get; set; } = default!;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Unit { get; set; } = string.Empty;

    public override string ToString()
        => $"[{Timestamp:HH:mm:ss}] {DeviceId} {SensorType}={Value} {Unit}";
}

/* Reference List
Albahari, J. and Albahari, B., 2022. C# 10 in a nutshell: The definitive reference. Sebastopol: O'Reilly Media.

IIE, 2026. PROG7312 POE. The Independent Institute of Education (Pty) Ltd.

Microsoft Docs, 2026. Generic classes and methods. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/generics> [Accessed 13 September 2026].
*/

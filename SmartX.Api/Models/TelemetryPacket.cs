using System;
using System.Globalization;


namespace SmartX.Api.Models;

public class TelemetryPacket<T>
{
    public string DeviceId { get; set; } = string.Empty;
    public string SensorType { get; set; } = string.Empty;

    // Generic property: T is resolved at compile time (Microsoft Docs, 2026).
    public T Value { get; set; } = default!;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string Unit { get; set; } = string.Empty;

    // Generic method output uses compile-time type binding (Albahari and Albahari, 2022).
    public override string ToString()
        => string.Format(CultureInfo.InvariantCulture,
                         "[{0:HH:mm:ss}] {1} {2}={3} {4}",
                         Timestamp, DeviceId, SensorType, Value, Unit).TrimEnd();
}

/* Reference List
Albahari, J. and Albahari, B., 2022. C# 10 in a nutshell: The definitive reference. Sebastopol: O'Reilly Media.

Microsoft Docs, 2026. Generic classes and methods. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/generics> [Accessed 12 September 2026].

Microsoft Docs, 2026. Boxing and unboxing. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/types/boxing-and-unboxing> [Accessed 12 September 2026].
*/
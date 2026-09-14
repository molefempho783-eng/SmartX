using System;


//Represents a registered sensor with its deployment metadata (IIE, 2026).

namespace SmartX.Api.Models;



public class SensorRegistration
{
    // MAC address as unique identifier (IEEE, 2016).
    public string MacAddress { get; set; } = string.Empty;

    // Human-readable deployment location (IIE, 2026).
    public string Location { get; set; } = string.Empty;

    // Category: Environmental, Power Consumption, Actuator (IIE, 2026).
    public string Category { get; set; } = string.Empty;

    // UTC timestamp for audit trail (Microsoft Docs, 2026).
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    // List<T> for sequential file attachments (Albahari and Albahari, 2022).
    public List<string> Attachments { get; set; } = new();
}

/* Reference List
Albahari, J. and Albahari, B., 2022. C# 10 in a nutshell: The definitive reference. Sebastopol: O'Reilly Media.

IEEE, 2016. IEEE standard for local and metropolitan area networks: Overview and architecture. IEEE Std 802-2014. Piscataway: IEEE.

IIE, 2026. PROG7312 POE. The Independent Institute of Education (Pty) Ltd.

Microsoft Docs, 2026. Auto-implemented properties. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/auto-implemented-properties> [Accessed 12 September 2026].
*/
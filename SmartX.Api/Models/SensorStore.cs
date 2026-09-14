using SmartX.Api.Models;
using System.Collections.Concurrent;

// In-memory store for registered sensors and telemetry log (IIE, 2026).

namespace SmartX.Api.Services;

public class SensorStore
{
    // ConcurrentDictionary prevents race conditions in multi-threaded API calls
    // (Microsoft Docs, 2026).
    private readonly ConcurrentDictionary<string, SensorRegistration> _sensors = new();

    // List<T> for ordered telemetry log storage (Albahari and Albahari, 2022).
    private readonly List<string> _telemetryLog = new();

    // Lock object for thread-safe writes to the log (Microsoft Docs, 2026).
    private readonly object _lock = new();

    public void Register(SensorRegistration reg)
    {
        // O(1) insertion/update via key assignment (Sedgewick and Wayne, 2011).
        _sensors[reg.MacAddress] = reg;

        lock (_lock)
        {
            _telemetryLog.Add($"[{DateTime.UtcNow:HH:mm:ss}] Registered {reg.MacAddress} at {reg.Location}");
        }
    }

    public IEnumerable<SensorRegistration> GetAll() => _sensors.Values;

    // O(1) lookup using TryGetValue (Microsoft Docs, 2026).
    public SensorRegistration? Get(string mac) =>
        _sensors.TryGetValue(mac, out var s) ? s : null;

    // Generic method accepts any TelemetryPacket<T> (Albahari and Albahari, 2022).
    public void RecordTelemetry<T>(TelemetryPacket<T> packet)
    {
        lock (_lock)
        {
            _telemetryLog.Add(packet.ToString());
        }
    }

    public object GetDashboardSummary()
    {
        lock (_lock)
        {
            return new
            {
                TotalSensors = _sensors.Count,
                TotalReadings = _telemetryLog.Count,

                // LINQ TakeLast for recent entries (Microsoft Docs, 2026).
                RecentLog = _telemetryLog.TakeLast(20).ToList()
            };
        }
    }

    public void AddAttachment(string mac, string fileName)
    {
        if (_sensors.TryGetValue(mac, out var sensor))
        {
            sensor.Attachments.Add(fileName);
        }
    }
}

/* Reference List
Albahari, J. and Albahari, B., 2022. C# 10 in a nutshell: The definitive reference. Sebastopol: O'Reilly Media.

IIE, 2026. PROG7312 POE. The Independent Institute of Education (Pty) Ltd.

Microsoft Docs, 2026. ConcurrentDictionary<TKey,TValue> class. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/api/system.collections.concurrent.concurrentdictionary-2> [Accessed 12 September 2026].

Microsoft Docs, 2026. Language Integrated Query (LINQ). [online] Available at: <https://learn.microsoft.com/en-us/dotnet/csharp/linq/> [Accessed 12 September 2026].

Sedgewick, R. and Wayne, K., 2011. Algorithms. 4th ed. Boston: Addison-Wesley.
*/
// Centralised API client for the MAUI frontend (Microsoft Docs, 2026).

using System.Net.Http.Json;
using SmartX.Maui.Models;

namespace SmartX.Maui.Services;

public class ApiService
{
    private readonly HttpClient _http;

    // Single source of truth for the backend port. Must match the
    // applicationUrl in SmartX.Api/Properties/launchSettings.json and the
    // host port published by docker-compose.yml (IIE, 2026).
    public const int ApiPort = 5000;

    public ApiService()
    {
        // Platform-aware base URL (Microsoft Docs, 2026):
        // - Windows desktop:    http://localhost:5000
        // - Android emulator:   http://10.0.2.2:5000 (host loopback alias)
        // - iOS simulator:      http://localhost:5000
        // - Mac Catalyst:       http://localhost:5000
        var host = DeviceInfo.Platform == DevicePlatform.Android
            ? "10.0.2.2"
            : "localhost";

        _http = new HttpClient
        {
            BaseAddress = new Uri($"http://{host}:{ApiPort}"),
            Timeout = TimeSpan.FromSeconds(30)
        };
    }

    public string BaseAddress => _http.BaseAddress?.ToString() ?? "(not set)";

    // Set on every dashboard poll so the UI can show connection state without
    // issuing a second request (Microsoft Docs, 2026).
    public bool IsApiOnline { get; private set; }

    // GET /api/sensors (IIE, 2026).
    public async Task<List<SensorRegistration>> GetSensorsAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<SensorRegistration>>("api/sensors")
                   ?? new List<SensorRegistration>();
        }
        catch
        {
            return new List<SensorRegistration>();
        }
    }

    // POST /api/sensors/register (IIE, 2026).
    public async Task<bool> RegisterSensorAsync(SensorRegistration reg)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/sensors/register", reg);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // POST /api/sensors/{mac}/attachment - multipart form upload
    // (Microsoft Docs, 2026).
    public async Task<bool> UploadAttachmentAsync(string mac, FileResult file)
    {
        try
        {
            using var content = new MultipartFormDataContent();
            using var stream = await file.OpenReadAsync();
            var fileContent = new StreamContent(stream);
            fileContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(
                    string.IsNullOrWhiteSpace(file.ContentType)
                        ? "application/octet-stream"
                        : file.ContentType);
            content.Add(fileContent, "file", file.FileName);

            var response = await _http.PostAsync(
                $"api/sensors/{Uri.EscapeDataString(mac)}/attachment", content);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    // Generic helper: one method posts TelemetryPacket<float>, <int> or <bool>
    // without boxing, because T is resolved at compile time
    // (Albahari and Albahari, 2022).
    private async Task<bool> PostPacketAsync<T>(string route, TelemetryPacket<T> packet)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(route, packet);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public Task<bool> SendMoistureAsync(string deviceId, float value) =>
        PostPacketAsync("api/telemetry/moisture", new TelemetryPacket<float>
        {
            DeviceId = deviceId,
            SensorType = "Moisture",
            Value = value,
            Unit = "%"
        });

    public Task<bool> SendWattageAsync(string deviceId, int value) =>
        PostPacketAsync("api/telemetry/power", new TelemetryPacket<int>
        {
            DeviceId = deviceId,
            SensorType = "Wattage",
            Value = value,
            Unit = "W"
        });

    public Task<bool> SendValveAsync(string deviceId, bool isOpen) =>
        PostPacketAsync("api/telemetry/valve", new TelemetryPacket<bool>
        {
            DeviceId = deviceId,
            SensorType = "Valve",
            Value = isOpen,
            Unit = ""
        });

    // POST /api/telemetry/batch (IIE, 2026).
    public async Task<string> StoreBatchAsync(int batchIndex, float[] moisture, int[] wattage)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/telemetry/batch", new
            {
                batchIndex,
                moistureValues = moisture,
                wattageValues = wattage
            });
            return response.IsSuccessStatusCode
                ? await response.Content.ReadAsStringAsync()
                : $"Batch store failed ({(int)response.StatusCode}).";
        }
        catch (Exception ex)
        {
            return $"Batch store failed: {ex.Message}";
        }
    }

    // GET /api/telemetry/batches/summary (IIE, 2026).
    public async Task<string> GetBatchSummaryAsync()
    {
        try
        {
            return await _http.GetStringAsync("api/telemetry/batches/summary");
        }
        catch (Exception ex)
        {
            return $"Batch summary failed: {ex.Message}";
        }
    }

    // POST /api/deployment/validate - sends a nested Facility > Zone > Sub-Zone
    // tree for recursive validation (IIE, 2026).
    public async Task<string> ValidateDeploymentAsync(object tree)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("api/deployment/validate", tree);
            return await response.Content.ReadAsStringAsync();
        }
        catch (Exception ex)
        {
            return $"Validation failed: {ex.Message}";
        }
    }

    // GET /api/demo/operator-overloading (IIE, 2026).
    public async Task<string> GetOperatorDemoAsync()
    {
        try
        {
            return await _http.GetStringAsync("api/demo/operator-overloading");
        }
        catch (Exception ex)
        {
            return $"Demo failed: {ex.Message}";
        }
    }

    // GET /api/dashboard/summary (IIE, 2026).
    public async Task<DashboardSummary> GetDashboardSummaryAsync()
    {
        try
        {
            var summary = await _http.GetFromJsonAsync<DashboardSummary>("api/dashboard/summary")
                          ?? new DashboardSummary();
            IsApiOnline = true;
            return summary;
        }
        catch
        {
            IsApiOnline = false;
            return new DashboardSummary();
        }
    }

    // Lightweight connectivity probe used by the dashboard status pill
    // (Microsoft Docs, 2026).
    public async Task<bool> IsApiOnlineAsync()
    {
        try
        {
            var response = await _http.GetAsync("api/dashboard/summary");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }
}

/* Reference List
Albahari, J. and Albahari, B., 2022. C# 10 in a nutshell: The definitive reference. Sebastopol: O'Reilly Media.

IIE, 2026. PROG7312 POE. The Independent Institute of Education (Pty) Ltd.

Microsoft Docs, 2026. Call a web API from a .NET MAUI app. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/maui/data-cloud/local-web-services> [Accessed 13 September 2026].

Microsoft Docs, 2026. DeviceInfo class. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/device/information> [Accessed 13 September 2026].
*/

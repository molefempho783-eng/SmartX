// Rubric requirement: dynamic dashboard engagement integration (IIE, 2026).

using System.Collections.ObjectModel;
using System.Globalization;
using SmartX.Maui.Models;
using SmartX.Maui.Services;

namespace SmartX.Maui.Pages;

public partial class TelemetryDashboardPage : ContentPage
{
    private readonly ApiService _api;
    private IDispatcherTimer? _refreshTimer;

    // Guards against overlapping polls when a request is slower than the timer
    // interval (Microsoft Docs, 2026).
    private bool _isRefreshing;

    // Whether the 3-second auto-refresh is running. Pausing lets an operator
    // study a burst without the list shifting underneath them (IIE, 2026).
    private bool _isLive = true;

    // Active severity filter: All, Critical, Normal or Info (IIE, 2026).
    private string _filter = "All";

    // Full unfiltered window of parsed lines. List<T> is used because the set is
    // rebuilt wholesale on every poll and only ever enumerated in order
    // (Albahari and Albahari, 2022).
    private readonly List<LogEntry> _allEntries = new();

    // ObservableCollection updates the UI automatically on change
    // (Microsoft Docs, 2026).
    public ObservableCollection<LogEntry> LogEntries { get; } = new();

    // Per-device roll-up shown above the log (IIE, 2026).
    public ObservableCollection<DeviceStatus> Devices { get; } = new();

    public TelemetryDashboardPage(ApiService api)
    {
        InitializeComponent();
        _api = api;

        LogCollectionView.ItemsSource = LogEntries;
        BindableLayout.SetItemsSource(DeviceList, Devices);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Initial refresh (IIE, 2026).
        _ = RefreshAsync();

        // Auto-refresh every 3 seconds - dynamic engagement feature (IIE, 2026).
        // The timer is created once and restarted, so navigating away and back
        // never stacks duplicate timers (Microsoft Docs, 2026).
        if (_refreshTimer is null)
        {
            _refreshTimer = Dispatcher.CreateTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(3);
            _refreshTimer.Tick += async (s, e) => await RefreshAsync();
        }

        if (_isLive) _refreshTimer.Start();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Stop the timer to prevent background work (Microsoft Docs, 2026).
        _refreshTimer?.Stop();
    }

    // Poll the API and rebuild every view on the page (IIE, 2026).
    private async Task RefreshAsync()
    {
        if (_isRefreshing) return;
        _isRefreshing = true;

        try
        {
            var summary = await _api.GetDashboardSummaryAsync();
            SetStatus(_api.IsApiOnline);

            TotalSensorsLabel.Text = summary.TotalSensors.ToString();
            TotalReadingsLabel.Text = summary.TotalReadings.ToString();

            // Parse the raw window into structured entries, newest first so the
            // latest activity needs no scrolling (IIE, 2026).
            _allEntries.Clear();
            foreach (var line in summary.RecentLog)
                _allEntries.Add(ParseLogLine(line));

            _allEntries.Reverse();

            var anomalies = _allEntries.Where(e => e.IsAnomalous).ToList();
            AnomalyCountLabel.Text = anomalies.Count.ToString();

            BuildDeviceRollup();
            ApplyFilter();
            UpdateAlertBanner(anomalies);

            LastUpdatedLabel.Text = _isLive
                ? $"Live - updated {DateTime.Now:HH:mm:ss}, refreshing every 3s"
                : $"Paused - last updated {DateTime.Now:HH:mm:ss}";

            await PulseAnomalyCardAsync(anomalies.Count);
        }
        finally
        {
            _isRefreshing = false;
        }
    }

    // Collapse the log window into one row per device using a Dictionary for
    // O(1) lookup while scanning, then transfer into the bound
    // ObservableCollection (Sedgewick and Wayne, 2011; Microsoft Docs, 2026).
    private void BuildDeviceRollup()
    {
        var rollup = new Dictionary<string, DeviceStatus>();

        // _allEntries is newest-first, so the first row seen for a device is its
        // most recent reading (IIE, 2026).
        foreach (var entry in _allEntries)
        {
            if (string.IsNullOrWhiteSpace(entry.DeviceId)) continue;
            if (entry.SensorType == "Registration") continue;

            if (!rollup.TryGetValue(entry.DeviceId, out var status))
            {
                status = new DeviceStatus
                {
                    DeviceId = entry.DeviceId,
                    LastSensor = entry.SensorType,
                    LastReading = entry.Reading,
                    LastSeen = entry.Time
                };
                rollup[entry.DeviceId] = status;
            }

            status.ReadingCount++;
            if (entry.IsAnomalous) status.HasAlert = true;
        }

        Devices.Clear();

        // Devices in alert state float to the top (IIE, 2026).
        foreach (var status in rollup.Values
                                     .OrderByDescending(d => d.HasAlert)
                                     .ThenBy(d => d.DeviceId))
        {
            Devices.Add(status);
        }

        ActiveDevicesLabel.Text = Devices.Count.ToString();
    }

    // Rebuild the bound collection from the active severity filter (IIE, 2026).
    private void ApplyFilter()
    {
        LogEntries.Clear();

        var visible = _filter == "All"
            ? _allEntries
            : _allEntries.Where(e => e.Severity == _filter).ToList();

        foreach (var entry in visible)
            LogEntries.Add(entry);

        LogCountLabel.Text = _filter == "All"
            ? $"{_allEntries.Count} entries"
            : $"{visible.Count} of {_allEntries.Count} entries";
    }

    // Surface the most recent breach above the fold so it cannot be missed
    // (IIE, 2026).
    private void UpdateAlertBanner(List<LogEntry> anomalies)
    {
        if (anomalies.Count == 0)
        {
            AlertBanner.IsVisible = false;
            return;
        }

        var latest = anomalies[0];
        var extra = anomalies.Count > 1
            ? $" (+{anomalies.Count - 1} more)"
            : string.Empty;

        AlertDetailLabel.Text =
            $"{latest.DeviceId} reported {latest.SensorType} of {latest.Reading} at {latest.Time}{extra}";
        AlertBanner.IsVisible = true;
    }

    // MAUI animation API - pulse effect drawing the eye to a new breach
    // (Microsoft Docs, 2026).
    private async Task PulseAnomalyCardAsync(int anomalyCount)
    {
        if (anomalyCount > 0)
        {
            AnomalyCard.BackgroundColor = Color.FromArgb("#F8CECC");
            AnomalyCountLabel.TextColor = Color.FromArgb("#991b1b");

            await AnomalyCard.ScaleToAsync(1.05, 300, Easing.CubicOut);
            await AnomalyCard.ScaleToAsync(1.0, 300, Easing.CubicIn);
        }
        else
        {
            AnomalyCard.BackgroundColor = Colors.White;
            AnomalyCountLabel.TextColor = Color.FromArgb("#1F4E79");
        }
    }

    // Connection pill gives immediate feedback when the gateway loses the API,
    // rather than silently showing stale counters (IIE, 2026).
    private void SetStatus(bool online)
    {
        StatusLabel.Text = online ? "API Connected" : "API Offline";
        StatusLabel.TextColor = online
            ? Color.FromArgb("#2e7d32")
            : Color.FromArgb("#991b1b");
        StatusPill.BackgroundColor = online
            ? Color.FromArgb("#D5E8D4")
            : Color.FromArgb("#F8CECC");
    }

    // Severity filter buttons (IIE, 2026).
    private void OnFilterClicked(object? sender, EventArgs e)
    {
        if (sender is not Button button) return;

        _filter = button.CommandParameter?.ToString() ?? "All";
        HighlightActiveFilter();
        ApplyFilter();
    }

    private void HighlightActiveFilter()
    {
        var buttons = new (Button Button, string Value)[]
        {
            (FilterAllButton, "All"),
            (FilterCriticalButton, "Critical"),
            (FilterNormalButton, "Normal"),
            (FilterInfoButton, "Info")
        };

        foreach (var (button, value) in buttons)
        {
            var selected = value == _filter;
            button.BackgroundColor = selected
                ? Color.FromArgb("#1F4E79")
                : Color.FromArgb("#f0f0f0");
            button.TextColor = selected ? Colors.White : Color.FromArgb("#333333");
        }
    }

    // Pause or resume the live feed (IIE, 2026).
    private void OnToggleLiveClicked(object? sender, EventArgs e)
    {
        _isLive = !_isLive;

        if (_isLive)
        {
            _refreshTimer?.Start();
            LiveToggleButton.Text = "Pause live feed";
            LiveToggleButton.BackgroundColor = Color.FromArgb("#f0f0f0");
            LiveToggleButton.TextColor = Color.FromArgb("#333333");
        }
        else
        {
            _refreshTimer?.Stop();
            LiveToggleButton.Text = "Resume live feed";
            LiveToggleButton.BackgroundColor = Color.FromArgb("#1F4E79");
            LiveToggleButton.TextColor = Colors.White;
            LastUpdatedLabel.Text = $"Paused - last updated {DateTime.Now:HH:mm:ss}";
        }
    }

    // Parse a log line into its component fields and classify it (IIE, 2026).
    // The API writes telemetry as "[HH:mm:ss] DeviceId SensorType=Value Unit"
    // and registrations as "[HH:mm:ss] Registered MAC at Location".
    private LogEntry ParseLogLine(string line)
    {
        var entry = new LogEntry { Text = line };

        var payload = line;
        var close = line.IndexOf(']');
        if (line.StartsWith("[") && close > 0)
        {
            entry.Time = line.Substring(1, close - 1);
            payload = line[(close + 1)..].Trim();
        }

        // Registration lines carry a MAC and a location rather than a reading
        // (IIE, 2026).
        if (payload.StartsWith("Registered", StringComparison.OrdinalIgnoreCase))
        {
            var rest = payload["Registered".Length..].Trim();
            var atIndex = rest.IndexOf(" at ", StringComparison.OrdinalIgnoreCase);

            entry.DeviceId = atIndex > 0 ? rest[..atIndex] : rest;
            entry.SensorType = "Registration";
            entry.Reading = atIndex > 0 ? rest[(atIndex + 4)..] : string.Empty;
            entry.Severity = "Info";
            return entry;
        }

        // Telemetry lines: "DeviceId SensorType=Value Unit" (IIE, 2026).
        var space = payload.IndexOf(' ');
        if (space > 0)
        {
            entry.DeviceId = payload[..space];

            var remainder = payload[(space + 1)..].Trim();
            var equals = remainder.IndexOf('=');

            if (equals > 0)
            {
                entry.SensorType = remainder[..equals].Trim();
                entry.Reading = remainder[(equals + 1)..].Trim();
            }
            else
            {
                entry.Reading = remainder;
            }
        }
        else
        {
            entry.DeviceId = payload;
        }

        entry.Severity = Classify(entry, out var anomalous);
        entry.IsAnomalous = anomalous;
        return entry;
    }

    // Anomaly detection heuristic applied to the parsed fields (IIE, 2026).
    private string Classify(LogEntry entry, out bool anomalous)
    {
        anomalous = false;

        if (entry.SensorType.Equals("Moisture", StringComparison.OrdinalIgnoreCase)
            && TryExtractValue(entry.Reading, out var moisture))
        {
            // Dry-out or waterlogging (IIE, 2026).
            if (moisture < 20f || moisture > 90f)
            {
                anomalous = true;
                return "Critical";
            }
            return "Normal";
        }

        if (entry.SensorType.Equals("Wattage", StringComparison.OrdinalIgnoreCase)
            && TryExtractValue(entry.Reading, out var watt))
        {
            // Overload on a smart meter (IIE, 2026).
            if (watt > 500f)
            {
                anomalous = true;
                return "Critical";
            }
            return "Normal";
        }

        // Valve state changes are normal operational traffic (IIE, 2026).
        if (entry.SensorType.Equals("Valve", StringComparison.OrdinalIgnoreCase))
            return "Normal";

        // Anything else (unknown sensor type) is informational only (IIE, 2026).
        return "Info";
    }

    // Reads the leading number out of a reading such as "55.5 %" (IIE, 2026).
    private bool TryExtractValue(string reading, out float value)
    {
        value = 0;
        if (string.IsNullOrWhiteSpace(reading)) return false;

        var numPart = new string(reading.Trim()
            .TakeWhile(c => char.IsDigit(c) || c == '.' || c == '-')
            .ToArray());

        // Parsed with InvariantCulture to match how the API formats the line.
        // Under a locale such as en-ZA the default parse expects a comma decimal
        // separator and would silently fail on "52.4", so no anomaly would ever
        // be detected (Microsoft Docs, 2026).
        return float.TryParse(numPart,
                              NumberStyles.Float,
                              CultureInfo.InvariantCulture,
                              out value);
    }

    // Manual refresh button handler (IIE, 2026).
    private async void OnRefreshClicked(object? sender, EventArgs e)
    {
        await RefreshAsync();
    }
}

/* Reference List
Albahari, J. and Albahari, B., 2022. C# 10 in a nutshell: The definitive reference. Sebastopol: O'Reilly Media.

IIE, 2026. PROG7312 POE. The Independent Institute of Education (Pty) Ltd.

Microsoft Docs, 2026. Animation in .NET MAUI. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/maui/user-interface/animation/> [Accessed 13 September 2026].

Microsoft Docs, 2026. Bindable layouts in .NET MAUI. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/maui/user-interface/layouts/bindablelayout> [Accessed 13 September 2026].

Microsoft Docs, 2026. DispatcherTimer class. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/api/microsoft.maui.dispatching.idispatchertimer> [Accessed 13 September 2026].

Microsoft Docs, 2026. ObservableCollection<T> class. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/api/system.collections.objectmodel.observablecollection-1> [Accessed 13 September 2026].

Sedgewick, R. and Wayne, K., 2011. Algorithms. 4th ed. Boston: Addison-Wesley.
*/

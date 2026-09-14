

namespace SmartX.Maui.Models;

/// Dashboard summary returned by GET /api/dashboard/summary (IIE, 2026).
public class DashboardSummary
{
    public int TotalSensors { get; set; }
    public int TotalReadings { get; set; }

    // List<T> for sequential log lines (Albahari and Albahari, 2022).
    public List<string> RecentLog { get; set; } = new();
}

/* Reference List
Albahari, J. and Albahari, B., 2022. C# 10 in a nutshell: The definitive reference. Sebastopol: O'Reilly Media.
IIE, 2026. PROG7312 POE. The Independent Institute of Education (Pty) Ltd.
*/
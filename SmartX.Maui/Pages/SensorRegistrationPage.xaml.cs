
/// Rubric requirement: file upload + registration + client validation (IIE, 2026).

using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using SmartX.Maui.Models;
using SmartX.Maui.Services;

namespace SmartX.Maui.Pages;

public partial class SensorRegistrationPage : ContentPage
{
    private readonly ApiService _api;
    private FileResult? _selectedFile;

    // ObservableCollection updates UI automatically on change
    // (Microsoft Docs, 2026).
    public ObservableCollection<SensorRegistration> Sensors { get; } = new();

    public SensorRegistrationPage(ApiService api)
    {
        InitializeComponent();
        _api = api;
        SensorCollectionView.ItemsSource = Sensors;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await LoadSensorsAsync();
    }

    // Load sensors from API (IIE, 2026).
    private async Task LoadSensorsAsync()
    {
        var sensors = await _api.GetSensorsAsync();
        Sensors.Clear();
        foreach (var s in sensors) Sensors.Add(s);
    }

    // File picker handler (Microsoft Docs, 2026).
    private async void OnPickFileClicked(object sender, EventArgs e)
    {
        try
        {
            // FilePicker.PickAsync opens native file dialog (Microsoft Docs, 2026).
            var result = await FilePicker.Default.PickAsync(new PickOptions
            {
                PickerTitle = "Select configuration file or photo"
            });

            if (result != null)
            {
                _selectedFile = result;
                SelectedFileLabel.Text = $"Selected: {result.FileName}";
                SelectedFileLabel.TextColor = Color.FromArgb("#1F4E79");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"File pick failed: {ex.Message}", "OK");
        }
    }

    // Register sensor handler (IIE, 2026).
    private async void OnRegisterClicked(object sender, EventArgs e)
    {
        var mac = MacEntry.Text?.Trim() ?? "";
        var location = LocationEntry.Text?.Trim() ?? "";
        var category = CategoryPicker.SelectedItem?.ToString() ?? "Environmental";

        // Client-side validation (IIE, 2026).
        if (string.IsNullOrWhiteSpace(mac))
        {
            ShowMessage("MAC address is required.", isError: true);
            return;
        }

        // Regex MAC validation (Microsoft Docs, 2026).
        if (!Regex.IsMatch(mac, @"^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$"))
        {
            ShowMessage("MAC format invalid. Use AA:BB:CC:DD:EE:FF", isError: true);
            return;
        }

        if (string.IsNullOrWhiteSpace(location))
        {
            ShowMessage("Deployment location is required.", isError: true);
            return;
        }

        RegisterButton.IsEnabled = false;
        RegisterButton.Text = "Registering...";

        var reg = new SensorRegistration
        {
            MacAddress = mac,
            Location = location,
            Category = category
        };

        var success = await _api.RegisterSensorAsync(reg);

        if (success)
        {
            // Upload attachment if selected (Microsoft Docs, 2026).
            if (_selectedFile != null)
            {
                await _api.UploadAttachmentAsync(mac, _selectedFile);
            }

            ShowMessage($"Sensor {mac} registered successfully!", isError: false);

            // Clear form (IIE, 2026).
            MacEntry.Text = "";
            LocationEntry.Text = "";
            _selectedFile = null;
            SelectedFileLabel.Text = "No file selected";
            SelectedFileLabel.TextColor = Color.FromArgb("#888");

            await LoadSensorsAsync();
        }
        else
        {
            ShowMessage("Registration failed. Check the API is running.", isError: true);
        }

        RegisterButton.IsEnabled = true;
        RegisterButton.Text = "Register Sensor";
    }

    // Show colour-coded status message (IIE, 2026).
    private void ShowMessage(string text, bool isError)
    {
        MessageLabel.Text = text;
        MessageLabel.TextColor = isError
            ? Color.FromArgb("#991b1b")
            : Color.FromArgb("#2e7d32");
        MessageBorder.BackgroundColor = isError
            ? Color.FromArgb("#F8CECC")
            : Color.FromArgb("#D5E8D4");
        MessageBorder.IsVisible = true;
    }
}

/* Reference List
IIE, 2026. PROG7312 POE. The Independent Institute of Education (Pty) Ltd.

Microsoft Docs, 2026. File picker in .NET MAUI. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/maui/platform-integration/storage/file-picker> [Accessed 12 September 2026].

Microsoft Docs, 2026. ObservableCollection<T> class. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/api/system.collections.objectmodel.observablecollection-1> [Accessed 12 September 2026].

Microsoft Docs, 2026. .NET regular expressions. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/standard/base-types/regular-expressions> [Accessed 12 September 2026].
*/
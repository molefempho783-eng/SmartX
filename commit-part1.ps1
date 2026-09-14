# ---------------------------------------------------------------------------
#  Smart-X PoE Part 1 - staged commit helper
#
#  Commits the Part 1 work as a series of focused commits instead of one lump,
#  because the rubric rewards a clear commit history.
#
#  Usage:
#      cd "C:\Users\User\Documents\school stuff\prog7312\SmartX"
#      .\commit-part1.ps1            # commit only
#      .\commit-part1.ps1 -Push      # commit, then push
#
#  If PowerShell blocks the script, run it once as:
#      powershell -ExecutionPolicy Bypass -File .\commit-part1.ps1
# ---------------------------------------------------------------------------

param([switch]$Push)

# git writes routine notices (CRLF conversion, etc.) to stderr. With
# ErrorActionPreference set to Stop, PowerShell turns any native stderr output
# into a thrown exception, which aborts the script on a harmless warning.
$ErrorActionPreference = "Continue"

# Runs git with stderr folded into stdout and discarded, and hands back the
# real exit code so callers can branch on it.
function Run-Git {
    param([Parameter(ValueFromRemainingArguments = $true)][string[]] $GitArgs)
    $null = & git @GitArgs 2>&1
    return $LASTEXITCODE
}

if (-not (Test-Path ".git")) {
    Write-Host "Not a git repository. Run this from the SmartX folder." -ForegroundColor Red
    exit 1
}

$script:made = 0
$script:skipped = 0

function Commit-Set {
    param(
        [string]   $Message,
        [string[]] $Paths
    )

    $existing = @()
    foreach ($p in $Paths) {
        if (Test-Path $p) { $existing += $p }
    }

    if ($existing.Count -eq 0) {
        Write-Host "  skip  (no files)  $($Message.Split("`n")[0])" -ForegroundColor DarkGray
        $script:skipped++
        return
    }

    $null = Run-Git @("add", "--") + $existing

    # nothing actually changed for these paths
    $changed = Run-Git "diff" "--cached" "--quiet"
    if ($changed -eq 0) {
        Write-Host "  skip  (no change) $($Message.Split("`n")[0])" -ForegroundColor DarkGray
        $script:skipped++
        return
    }

    $null = Run-Git "commit" "-q" "-m" $Message
    Write-Host "  ok    $($Message.Split("`n")[0])" -ForegroundColor Green
    $script:made++
}

Write-Host ""
Write-Host "Smart-X Part 1 - staged commits" -ForegroundColor Cyan
Write-Host "--------------------------------"

# --- 0. stop tracking build output -----------------------------------------
Write-Host ""
Write-Host "Untracking build output and IDE state..." -ForegroundColor Cyan
$null = Run-Git "rm" "-r" "--cached" "--quiet" "--ignore-unmatch" `
    "SmartX.Api/bin" "SmartX.Api/obj" `
    "SmartX.Maui/bin" "SmartX.Maui/obj" ".vs"

$null = Run-Git "add" "--" ".gitignore"
if ((Run-Git "diff" "--cached" "--quiet") -ne 0) {
    $null = Run-Git "commit" "-q" "-m" @"
Add .gitignore and stop tracking build output

bin/, obj/ and .vs/ were being committed because the repository predates
the .gitignore file. gitignore does not untrack files git already follows,
so they are removed from the index here.
"@
    Write-Host "  ok    Add .gitignore and stop tracking build output" -ForegroundColor Green
    $script:made++
} else {
    Write-Host "  skip  (no change) .gitignore" -ForegroundColor DarkGray
    $script:skipped++
}

# --- backend ----------------------------------------------------------------
Write-Host ""
Write-Host "Backend..." -ForegroundColor Cyan

Commit-Set @"
Fix build: remove Dockerfile from the csproj compile items

The Dockerfile was listed as <Compile Include="dockerfile" />, so the C#
compiler tried to parse it and failed with CS1024. Also removed the
CommunityToolkit.Maui and CommunityToolkit.Mvvm package references, which
are MAUI-only and unused in a web API.
"@ @("SmartX.Api/SmartX.Api.csproj")

Commit-Set @"
Serve the API on port 5000 for both dotnet run and Docker

The Kestrel endpoint block in appsettings.json overrode launchSettings and
bound 0.0.0.0:8080, so the MAUI client could not reach the API. Both launch
profiles now use port 5000, matching the host port published by
docker-compose. Added a Telemetry section for the batch store dimensions.
"@ @("SmartX.Api/appsettings.json", "SmartX.Api/Properties/launchSettings.json")

Commit-Set @"
Register TelemetryBatchStore and expose the jagged array endpoints

TelemetryBatchStore has no parameterless constructor, so the DI container
could not resolve its two int parameters and the host failed to start. It is
now registered through a factory that reads its dimensions from
configuration. Added POST /api/telemetry/batch and
GET /api/telemetry/batches/summary so the float[][] and int[][] storage is
reachable and the transfer into List<T> is observable.
"@ @("SmartX.Api/Program.cs")

Commit-Set @"
Format telemetry values with invariant culture

Under a locale such as en-ZA a float was logged as "52,4" rather than
"52.4", and the client parsed it with the current culture, so anomaly
detection silently never triggered.
"@ @("SmartX.Api/Models/TelemetryPacket.cs")

Commit-Set @"
Fix the Docker build and use the runtime image's non-root user

The aspnet:10.0 image has no adduser binary, so the RUN step exited with
code 127. Switched to USER `$APP_UID, which the .NET runtime images provide,
moved the publish COPY above the privilege drop, and created the uploads
directory so a non-root process can write attachments. Removed the obsolete
version key from docker-compose.yml.
"@ @("SmartX.Api/Dockerfile", "docker-compose.yml")

# --- client models ----------------------------------------------------------
Write-Host ""
Write-Host "Client models..." -ForegroundColor Cyan

Commit-Set @"
Remove Android-only using and parse log lines into structured fields

using Java.Security broke the Windows build. LogEntry now also carries the
timestamp, device id, sensor type and reading as separate properties, so the
dashboard can label each field and classify on the sensor type rather than a
substring match.
"@ @("SmartX.Maui/Models/LogEntry.cs")

Commit-Set @"
Add client-side TelemetryPacket<T>

Mirrors the API's generic wrapper so float, int and bool payloads serialise
through one class with no boxing.
"@ @("SmartX.Maui/Models/TelemetryPacket.cs")

Commit-Set @"
Add DeviceStatus model for the per-device roll-up
"@ @("SmartX.Maui/Models/DeviceStatus.cs")

Commit-Set @"
Point the client at port 5000 and add telemetry and diagnostic calls

Added a single ApiPort constant, generic packet posting, batch storage,
deployment validation and the operator overloading demo, plus an IsApiOnline
flag the dashboard uses for its connection indicator without a second
request.
"@ @("SmartX.Maui/Services/ApiService.cs")

# --- client UI --------------------------------------------------------------
Write-Host ""
Write-Host "Client UI..." -ForegroundColor Cyan

Commit-Set @"
Pin the application to the light theme

Every page uses a hardcoded light palette, so a device running dark mode
resolved the default AppThemeBinding styles to white Entry text on a light
background and the registration form was unreadable.
"@ @("SmartX.Maui/App.xaml.cs")

Commit-Set @"
Style the Shell navigation bar and flyout

Navy title bar with a white hamburger and title, white flyout panel with
dark item text, so the navigation chrome matches the page palette instead of
inheriting the platform default.
"@ @("SmartX.Maui/AppShell.xaml")

Commit-Set @"
Set explicit text colours on the registration form controls
"@ @("SmartX.Maui/Pages/SensorRegistrationPage.xaml")

Commit-Set @"
Add direct navigation buttons to the landing page
"@ @("SmartX.Maui/Pages/LandingPage.xaml", "SmartX.Maui/Pages/LandingPage.xaml.cs")

Commit-Set @"
Rebuild the telemetry dashboard around operator workflow

Adds a connection indicator, a per-device roll-up built through a Dictionary
for O(1) lookup, an alert banner naming the most recent breach, a severity
filter and a pause control. Log rows now show time, device, sensor type and
reading as labelled columns, newest first.
"@ @("SmartX.Maui/Pages/TelemetryDashboardPage.xaml", "SmartX.Maui/Pages/TelemetryDashboardPage.xaml.cs")

# --- docs and assets --------------------------------------------------------
Write-Host ""
Write-Host "Documentation..." -ForegroundColor Cyan

Commit-Set @"
Rewrite the README with setup, run and troubleshooting instructions

Covers restoring dependencies, building, booting the API, running the MAUI
client, the Docker route, every endpoint, where each language requirement
lives, and the failures most likely to be hit first.
"@ @("README.md")

Commit-Set @"
Add the recording run sheet for the Part 1 video demo
"@ @("VIDEO_CHECKLIST.md")

Commit-Set @"
Add sample device files for upload testing
"@ @("SampleUploads")

# --- anything left over -----------------------------------------------------
Write-Host ""
Write-Host "Remaining changes..." -ForegroundColor Cyan

$null = Run-Git "add" "-A"
if ((Run-Git "diff" "--cached" "--quiet") -ne 0) {
    $null = Run-Git "commit" "-q" "-m" @"
Remove the unused SmartX.Web project and tidy the solution

The Blazor project was created by accident; the MAUI client is the frontend
for this PoE.
"@
    Write-Host "  ok    Remove the unused SmartX.Web project and tidy the solution" -ForegroundColor Green
    $script:made++
} else {
    Write-Host "  skip  (nothing left)" -ForegroundColor DarkGray
}

# --- summary ----------------------------------------------------------------
Write-Host ""
Write-Host "--------------------------------" -ForegroundColor Cyan
Write-Host "Commits created : $script:made"
Write-Host "Skipped         : $script:skipped"
Write-Host "Total on branch : $(git rev-list --count HEAD)"
Write-Host ""

if ($Push) {
    Write-Host "Pushing..." -ForegroundColor Cyan
    git push
} else {
    Write-Host "Review with:  git log --oneline -20" -ForegroundColor Yellow
    Write-Host "Then push  :  git push" -ForegroundColor Yellow
}
Write-Host ""

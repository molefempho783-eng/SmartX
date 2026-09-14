# Smart-X IoT Gateway — PROG7312 PoE Part 1

Smart-X is a two-layer .NET 10 system for ingesting and validating IoT sensor telemetry.

| Layer | Project | What it is |
|---|---|---|
| Backend | `SmartX.Api` | ASP.NET Core Minimal API — receives sensor registrations, telemetry and file uploads |
| Frontend | `SmartX.Maui` | .NET MAUI client — landing page, registration form, live telemetry dashboard |

The frontend talks to the backend over HTTP on **port 5000**. Nothing is stored in a database; state lives in memory for the lifetime of the API process.

## Demo video

**▶ [Watch the Part 1 demo on YouTube](https://youtu.be/ziT9EdiiUUw)**

A walkthrough of the running system: the API and Swagger, sensor registration with file upload, and the live telemetry dashboard — plus where generics, operator overloading, jagged arrays and recursion are implemented.

---

## 1. Prerequisites

| Requirement | Check it with |
|---|---|
| .NET 10 SDK | `dotnet --version` |
| .NET MAUI workload | `dotnet workload list` |
| Windows 10 build 19041+ (to run the MAUI app on desktop) | — |
| Docker Desktop (optional — only for the containerised API) | `docker --version` |

If the MAUI workload is missing:

```powershell
dotnet workload install maui
```

---


From the solution root (`SmartX`):

```powershell
dotnet restore
dotnet build
```

---

## 3. Run the backend API

```powershell
cd SmartX.Api
dotnet run
```

The API starts on **http://localhost:5000** and opens Swagger automatically.

| URL | What it shows |
|---|---|
| http://localhost:5000/swagger | Interactive API explorer |
| http://localhost:5000/openapi/v1.json | Raw OpenAPI document |

Leave this terminal running. Swagger is only exposed in the Development environment.

### Running the API in Docker instead

```powershell
docker compose up --build
```

This also publishes the API on **http://localhost:5000**, so the MAUI client needs no changes. Uploaded files are persisted to `SmartX.Api/Uploads` on the host.

---

## 4. Run the MAUI client

Open a **second** terminal (the API must stay running):

```powershell
cd SmartX.Maui
dotnet build -t:Run -f net10.0-windows10.0.19041.0
```

Or in Visual Studio: set **SmartX.Maui** as the startup project, choose **Windows Machine**, and press F5.

> **Android emulator:** the client automatically switches its base address to `http://10.0.2.2:5000`, which is the emulator's alias for the host machine. No configuration needed.

---

## 5. Using the app

1. **Landing page** — three architectural pillars. Only *Sensor Data Ingestion* is active; the other two are greyed out for Part 2 and the final PoE.
2. **Sensor Registration** — enter a MAC address (format `AA:BB:CC:DD:EE:FF`), a deployment location and a category. Optionally attach a config file or photo, then press **Register Sensor**.
3. **Telemetry Dashboard** — polls the API every 3 seconds and colour-codes each log line by severity. The anomaly card pulses red when out-of-range readings arrive.

### Feeding the dashboard with telemetry

The dashboard reads whatever the API has received. To push readings in, use Swagger at http://localhost:5000/swagger:

| To show | Endpoint | Example body |
|---|---|---|
| A normal reading | `POST /api/telemetry/moisture` | `{ "deviceId": "ESP32-01", "sensorType": "Moisture", "value": 55.5, "unit": "%" }` |
| A **critical** moisture anomaly | `POST /api/telemetry/moisture` | `{ "deviceId": "ESP32-07", "sensorType": "Moisture", "value": 8.2, "unit": "%" }` |
| A **critical** power anomaly | `POST /api/telemetry/power` | `{ "deviceId": "ESP32-07", "sensorType": "Wattage", "value": 780, "unit": "W" }` |
| A boolean reading | `POST /api/telemetry/valve` | `{ "deviceId": "ESP32-01", "sensorType": "Valve", "value": true, "unit": "" }` |
| Jagged-array batch storage | `POST /api/telemetry/batch` | `{ "batchIndex": 0, "moistureValues": [45.1, 52.0, 61.3], "wattageValues": [120, 240, 310] }` |

The dashboard picks these up on its next 3-second poll — no restart needed.

> The anomaly thresholds are moisture below 20% or above 90%, and wattage above 500 W. `sensorType` must be spelled `Moisture` or `Wattage` for the dashboard's parser to classify the line.

### Sample test data

The **SampleUploads** folder holds realistic attachment files, and `VIDEO_CHECKLIST.md` has a table of five sensor records that pair with them — enough to populate the app without inventing data each time you test.

---

## 6. API endpoints

| Method | Route | Purpose |
|---|---|---|
| `POST` | `/api/sensors/register` | Register a sensor (MAC, location, category) |
| `GET` | `/api/sensors` | List all registered sensors |
| `POST` | `/api/telemetry/moisture` | Ingest `TelemetryPacket<float>` |
| `POST` | `/api/telemetry/power` | Ingest `TelemetryPacket<int>` |
| `POST` | `/api/telemetry/valve` | Ingest `TelemetryPacket<bool>` |
| `POST` | `/api/telemetry/batch` | Store a raw batch in the jagged arrays |
| `GET` | `/api/telemetry/batches/summary` | Transfer jagged arrays into `List<T>` and summarise |
| `POST` | `/api/deployment/validate` | Recursively validate a deployment tree |
| `POST` | `/api/sensors/{mac}/attachment` | Multipart file upload |
| `GET` | `/api/dashboard/summary` | Counts plus the 20 most recent log lines |
| `GET` | `/api/demo/operator-overloading` | Aggregate two meters using overloaded `+`, `-`, `>` |

---

## 7. Where each requirement lives

| Requirement | File |
|---|---|
| **Generics** — `TelemetryPacket<T>` | `SmartX.Api/Models/TelemetryPacket.cs` |
| **Operator overloading** — `+`, `-`, `>`, `<`, `==`, `!=` | `SmartX.Api/Models/SensorReading.cs` |
| **Jagged arrays** — `float[][]`, `int[][]`, `bool[][]` | `SmartX.Api/Services/TelemetryBatchStore.cs` |
| **Recursion** — nested deployment-tree validation | `SmartX.Api/Services/DeploymentValidator.cs` |
| **Collections** — `List<T>`, `ConcurrentDictionary`, `ObservableCollection<T>` | `SmartX.Api/Models/SensorStore.cs`, MAUI pages |
| **File upload** — multipart streaming | `SmartX.Api/Program.cs`, `SmartX.Maui/Services/ApiService.cs` |
| **Engagement feature** — severity-coded live log + pulsing anomaly card | `SmartX.Maui/Pages/TelemetryDashboardPage.xaml(.cs)` |

---

## 8. Troubleshooting

| Symptom | Cause and fix |
|---|---|
| Dashboard shows **API Offline** | The API isn't running, or it's on a different port. Confirm `http://localhost:5000/swagger` opens in a browser. |
| `Registration failed. Check the API is running.` | Same as above — the client could not reach port 5000. |
| Port 5000 already in use | Change `applicationUrl` in `SmartX.Api/Properties/launchSettings.json` **and** `ApiPort` in `SmartX.Maui/Services/ApiService.cs` to the same new port. |
| `dotnet run` reports a CS1024 preprocessor error in `Dockerfile` | A `<Compile Include="dockerfile" />` entry has crept back into `SmartX.Api.csproj`. Remove it — the Dockerfile is not a C# source file. |
| Android emulator can't reach the API | The emulator must use `10.0.2.2`, not `localhost`. This is handled automatically in `ApiService`. |
| MAUI build fails on a missing workload | Run `dotnet workload install maui`. |

---

## Reference List

Docker Docs, 2026. *Compose file reference*. [online] Available at: <https://docs.docker.com/compose/compose-file/> [Accessed 13 September 2026].

IIE, 2026. *PROG7312 PoE*. The Independent Institute of Education (Pty) Ltd.

Microsoft Docs, 2026. *Minimal APIs overview*. [online] Available at: <https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/overview> [Accessed 13 September 2026].

Microsoft Docs, 2026. *Call a web API from a .NET MAUI app*. [online] Available at: <https://learn.microsoft.com/en-us/dotnet/maui/data-cloud/local-web-services> [Accessed 13 September 2026].

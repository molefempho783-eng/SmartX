# Smart-X Part 1 — Recording Script

Read top to bottom. Every step contains the data it needs — you never need to scroll back.

Target length: **~18 minutes** (the step timings below add to 18:15). Marks shown per step add up to 100.

If your lecturer caps it shorter, trim Steps 2, 7 and 15 first — they are the least mark-dense.

---

# PART A — Before you press record

Do all of this first. None of it is filmed.

**A1. Start the API.** New terminal:

```powershell
cd "C:\Users\User\Documents\school stuff\prog7312\SmartX\SmartX.Api"
dotnet run
```

Wait for `Now listening on: http://localhost:5000`. Leave it running.

**A2. Open Swagger** in a browser tab: `http://localhost:5000/swagger`

**A3. Launch the MAUI app** from Visual Studio (F5, Windows Machine). Check the dashboard pill says **API Connected**.

**A4. Pre-register 3 sensors** so the app doesn't look empty on camera. Register these on the Sensor Registration page now, no attachments needed:

| MAC | Location | Category |
|---|---|---|
| `AA:BB:CC:DD:EE:02` | Plant Room, Zone A | Power Consumption |
| `AA:BB:CC:DD:EE:03` | Greenhouse 2, Sub-Zone C | Actuator |
| `AA:BB:CC:DD:EE:05` | Server Room, Zone A | Power Consumption |

**A5. Open File Explorer** at `SmartX.Api\Uploads\` and leave the window ready.

**A6. Open the SampleUploads folder** so file dialogs start there.

**A7. Housekeeping.** Close Discord/Slack/email. Set display scaling so code is readable at 1080p. Close every Visual Studio tab except the ones you'll show.

---

# PART B — Record

---

## Step 1 — Show the two layers
### Gateway Startup and API Integration · 10 marks · ~60s

**Do:** Open Solution Explorer. Point at `SmartX.Api` and `SmartX.Maui`.

**Say:** "Smart-X has two layers. SmartX.Api is an ASP.NET Core Minimal API — the backend that receives telemetry. SmartX.Maui is the client. They're separate projects that talk over HTTP on port 5000."

---

## Step 2 — Show the API is real
### Gateway Startup and API Integration · same 10 marks · ~60s

**Do:** Switch to the Swagger tab. Scroll the endpoint list slowly. Expand one endpoint to show the schema.

**Say:** "Every endpoint is documented through OpenAPI, grouped by tag — Sensors, Telemetry, Deployment, Dashboard. This is the live API, not a mock."

---

## Step 3 — Startup menu
### Gateway Startup and API Integration · same 10 marks · ~45s

**Do:** Switch to the MAUI app, on the landing page.

**Say:** "Three architectural pillars. Sensor Data Ingestion is active. Real-Time Command Stream is greyed out for Part 2. Network Topology is greyed out for the final PoE."

**Point at:** the ACTIVE / PART 2 / FINAL PoE badges.

---

## Step 4 — Navigation
### Gateway Startup and API Integration · same 10 marks · ~30s

**Do:** Landing → Sensor Registration → open the flyout (☰) → Telemetry Dashboard → flyout → back to Home.

**Say:** "Navigation is asynchronous — the UI thread never blocks, and state is preserved between views."

**Point at:** the green **API Connected** pill on the dashboard.

---

## Step 5 — Register a sensor with a config file
### Sensor Telemetry Data Ingestion · 10 marks + Media/Log Upload · 10 marks · ~90s

**Do:** Sensor Registration page. Fill in:

| Field | Value |
|---|---|
| MAC Address | `AA:BB:CC:DD:EE:01` |
| Deployment Location | `Room 4, Zone B` |
| Sensor Category | `Environmental` |

Press **Choose File** → `SampleUploads\esp32-config.json` → press **Register Sensor**.

**Say:** "MAC, deployment location, category — and an optional device configuration file, uploaded to the API as a multipart stream."

**Point at:** the green success message and the new row in the sensor list.

---

## Step 6 — Register a sensor with a photo
### Media/Log Upload · same 10 marks · ~45s

**Do:** Fill in:

| Field | Value |
|---|---|
| MAC Address | `AA:BB:CC:DD:EE:04` |
| Deployment Location | `Roof Deck, Zone D` |
| Sensor Category | `Environmental` |

Attach `SampleUploads\deployment-photo.jpg` → **Register Sensor**.

**Say:** "The same uploader handles a deployment photo — the brief asks for config files, photos or hardware logs."

---

## Step 7 — Prove the files landed
### Media/Log Upload · same 10 marks · ~30s

**Do:** Switch to File Explorer. Open `SmartX.Api\Uploads\`. Show the folders `AA_BB_CC_DD_EE_01` and `AA_BB_CC_DD_EE_04` and the files inside.

**Say:** "Files are stored per device. Note the MAC is sanitised — colons become underscores — so a device ID can't be used to escape the uploads folder. The server also enforces an extension allow-list."

---

## Step 8 — Validation
### Sensor Telemetry Data Ingestion · same 10 marks · ~60s

**Do:** Back on the registration form, try each of these and show the red message:

| Input | Expected message |
|---|---|
| MAC empty | MAC address is required. |
| MAC = `12345` | MAC format invalid. Use AA:BB:CC:DD:EE:FF |
| MAC = `GG:HH:II:JJ:KK:LL` | MAC format invalid (not hexadecimal) |
| Location empty | Deployment location is required. |

**Say:** "Client-side regex validation on the MAC, plus required-field checks. The API validates independently — the client is never trusted."

---

## Step 9 — Generics
### Advanced OOP: Generics and Overloading · 10 marks · ~2 min

**Do first:** Open `SmartX.Api\Models\TelemetryPacket.cs` in Visual Studio.

**Say:** "One generic wrapper, `TelemetryPacket<T>`, carries every payload type. T is bound at compile time, so a float, an int and a bool all flow through this one class with no boxing."

**Then:** Go to Swagger and post these three, one at a time. Overwrite the example body each time.

`POST /api/telemetry/moisture` — **float**
```json
{ "deviceId": "ESP32-01", "sensorType": "Moisture", "value": 55.5, "unit": "%" }
```

`POST /api/telemetry/power` — **int**
```json
{ "deviceId": "ESP32-02", "sensorType": "Wattage", "value": 248, "unit": "W" }
```

`POST /api/telemetry/valve` — **bool**
```json
{ "deviceId": "ESP32-03", "sensorType": "Valve", "value": true, "unit": "" }
```

**Then:** Switch to the dashboard. Wait 3 seconds.

**Point at:** three rows — `Moisture`, `Wattage`, `Valve` — each showing its device and value. "Three different C# types, one generic class."

> Swagger pre-fills every field with the word `string` and `0`. Overwrite it or the rows will read `string string=0 string`.

---

## Step 10 — Operator overloading
### Advanced OOP: Generics and Overloading · same 10 marks · ~60s

**Do first:** Open `SmartX.Api\Models\SensorReading.cs`. Scroll through the overloaded `+`, `-`, `>`, `<`, `==`.

**Say:** "Two smart meters can be added directly — `Meter3 = Meter1 + Meter2` — and subtracted for a delta. I also overrode Equals and GetHashCode so they stay consistent with `==`."

**Then:** In Swagger run `GET /api/demo/operator-overloading`.

**Point at:** the aggregate load and the delta in the response.

---

## Step 11 — Jagged arrays
### Data Handling: Arrays and Recursion · 10 marks · ~90s

**Do first:** Open `SmartX.Api\Services\TelemetryBatchStore.cs`.

**Say:** "Historical batches are held in jagged arrays — `float[][]`, `int[][]`, `bool[][]`. Jagged rather than multi-dimensional because each batch can have a different length, which is realistic for IoT."

**Then:** Swagger, `POST /api/telemetry/batch`:
```json
{
  "batchIndex": 0,
  "moistureValues": [45.1, 52.0, 61.3, 58.7, 49.2],
  "wattageValues": [120, 240, 310, 275, 198]
}
```

**Then:** `GET /api/telemetry/batches/summary`

**Point at:** the average and peak. "The raw jagged arrays are transferred into `List<float>` and `List<int>` for downstream processing."

---

## Step 12 — Recursion
### Data Handling: Arrays and Recursion · same 10 marks · ~90s

**Do first:** Open `SmartX.Api\Services\DeploymentValidator.cs`. Point at the base case (leaf node, no children) and the recursive case (the `foreach` over children).

**Say:** "The validator walks a nested deployment tree recursively. The base case is a leaf sensor, so it always terminates — deep hierarchies can't stack-overflow."

**Then:** Swagger, `POST /api/deployment/validate`:
```json
{
  "name": "Facility A",
  "nodeType": "Facility",
  "isConfigured": true,
  "children": [
    {
      "name": "Zone 1",
      "nodeType": "Zone",
      "isConfigured": true,
      "children": [
        {
          "name": "Sub-Zone B",
          "nodeType": "SubZone",
          "isConfigured": true,
          "children": [
            { "name": "Moisture Node 1", "nodeType": "Sensor", "isConfigured": true, "children": [] },
            { "name": "Valve Node 2", "nodeType": "Sensor", "isConfigured": false, "children": [] }
          ]
        }
      ]
    }
  ]
}
```

**Point at:** the error naming the full path `Facility A > Zone 1 > Sub-Zone B > Valve Node 2`.

> Only one line in that tree is `false` — `Valve Node 2`. Everything above it is `true`, which is what makes the failure specific.

**Then post this second tree** — identical except `Valve Node 2` is now configured — and show it passes:

```json
{
  "name": "Facility A",
  "nodeType": "Facility",
  "isConfigured": true,
  "children": [
    {
      "name": "Zone 1",
      "nodeType": "Zone",
      "isConfigured": true,
      "children": [
        {
          "name": "Sub-Zone B",
          "nodeType": "SubZone",
          "isConfigured": true,
          "children": [
            { "name": "Moisture Node 1", "nodeType": "Sensor", "isConfigured": true, "children": [] },
            { "name": "Valve Node 2", "nodeType": "Sensor", "isConfigured": true, "children": [] }
          ]
        }
      ]
    }
  ]
}
```

**Say:** "Same tree, one leaf corrected — and it validates clean. That proves the recursion is evaluating every node, not just failing by default."

**Why both runs matter:** a single failing run doesn't show the validator makes a decision. Failure then success does.

---

## Step 13 — The live dashboard
### Dynamic Dashboard Engagement · 10 marks · ~2 min
### This is the step most often lost. Show things *changing*.

**13a. Start calm.** Dashboard open, anomaly count 0, card white.

**13b. Post two normal readings** in Swagger:
```json
{ "deviceId": "ESP32-02", "sensorType": "Moisture", "value": 38.0, "unit": "%" }
```
```json
{ "deviceId": "ESP32-05", "sensorType": "Wattage", "value": 412, "unit": "W" }
```
**Point at:** counters climbing, rows green, devices appearing in **Reporting Devices**.

**13c. Post an overload:**
```json
{ "deviceId": "ESP32-05", "sensorType": "Wattage", "value": 780, "unit": "W" }
```
**Point at:** the red alert banner naming ESP32-05, the anomaly card **pulsing**, the red row, ESP32-05 flipping to **ALERT** and floating to the top of the device list.

**13d. Post a dry-out:**
```json
{ "deviceId": "ESP32-07", "sensorType": "Moisture", "value": 8.2, "unit": "%" }
```
**Say:** "Thresholds are moisture below 20 or above 90, and wattage above 500."

**13e. Click the Critical filter.** Only breaches remain. Click **All** to restore.

**13f. Press Pause live feed.** Header changes to "Paused". Press **Resume**.

**13g. Say the why:** "An operator watching a hundred nodes can't read every line. Colour coding, the banner and the device roll-up mean a dropout or a spike is spotted in seconds rather than by scanning text."

---

## Step 14 — Interface design
### User Interface Design · 10 marks · ~60s

**Do:**
1. Drag the window narrow, then wide — cards reflow instead of clipping
2. Scroll the telemetry log — stays smooth
3. Stop the API in its terminal, wait ~3 seconds — pill turns red **API Offline**
4. Restart the API — pill recovers on its own

**Say:** "One palette throughout — the navy in the navigation bar is the same navy as the headings. The app is pinned to a light theme so it looks identical regardless of the host OS setting. Every action gives feedback."

---

## Step 15 — Collections
### Collections Optimisation and Lists · 5 marks · ~45s

**Do:** Open `SmartX.Api\Models\SensorStore.cs`, then `SmartX.Maui\Pages\TelemetryDashboardPage.xaml.cs`.

**Say:** "The API uses a ConcurrentDictionary for O(1) sensor lookup with a lock around the ordered log, because a web API is multi-threaded. The client parses the log into a List, builds a per-device roll-up through a Dictionary, then transfers into an ObservableCollection so the UI updates itself."

---

## Step 16 — Repository and README
### Documentation and README Quality · 5 marks (+ up to −5 GitHub penalty) · ~60s

**Do:**
1. Show `README.md` rendered on GitHub
2. Scroll through the restore → build → run API → run client commands
3. Show the commit history — **20+ commits** with descriptive messages
4. Show there are no `bin/` or `obj/` folders committed
5. Show `Dockerfile` and `docker-compose.yml` exist

---

## Step 17 — Close
### ~30s

**Say:** "Two layers communicating over HTTP. Generics in TelemetryPacket<T>, operator overloading in SensorReading, jagged arrays in TelemetryBatchStore, recursion in DeploymentValidator. Pillars two and three are scaffolded for Part 2 and the final PoE."

---

# PART C — Before you submit

- [ ] Watch it back once at 1.5× — check nothing is unreadable
- [ ] Confirm your name and student number appear on screen or are spoken in the first 15 seconds
- [ ] Save the file using the naming convention below
- [ ] Upload to YouTube as **Unlisted**, not Private — Private means the marker can't open it
- [ ] Paste the link into the README and commit

## File name

Replace the bracketed parts. Check your brief or lecturer first — if they specify a format, theirs wins.

```
PROG7312_PoE_Part1_[StudentNumber]_[Surname]_[Initials].mp4
```

Example: `PROG7312_PoE_Part1_ST10123456_Molefe_M.mp4`

Rules that matter: underscores not spaces, no special characters, `.mp4` not `.mkv` or `.mov`, and the student number early enough that it survives being truncated in a file list.

## YouTube title

```
Smart-X IoT Gateway — PROG7312 PoE Part 1 Demo | [Student Number]
```

## YouTube description

Paste this in, fill the brackets, and fix the timestamps after you watch it back.

```
Smart-X IoT Data Ingestion and Validation Gateway
PROG7312 Portfolio of Evidence — Part 1

Student: [Full Name]
Student number: [Student Number]
Module: PROG7312 — Programming 3B
Institution: The Independent Institute of Education (IIE)

GitHub repository: [repo URL]

ABOUT
Smart-X is a two-layer .NET 10 system for ingesting and validating IoT sensor
telemetry. SmartX.Api is an ASP.NET Core Minimal API that receives sensor
registrations, telemetry and file uploads. SmartX.Maui is a .NET MAUI client
that communicates with it over HTTP. This video demonstrates the working
application and the advanced C# concepts required by the brief.

DEMONSTRATED IN THIS VIDEO
- Startup gateway with three architectural pillars (one active, two scaffolded)
- ASP.NET Core Minimal API with OpenAPI/Swagger documentation
- Sensor registration with MAC, deployment location and category validation
- Multipart file upload of device configuration files and deployment photos
- Generics — TelemetryPacket<T> carrying float, int and bool without boxing
- Operator overloading — +, -, >, <, == on SensorReading for meter aggregation
- Jagged arrays — float[][] / int[][] batch storage transferred into List<T>
- Recursion — validation of nested Facility > Zone > Sub-Zone > Sensor trees
- Live dashboard with severity colour coding, anomaly alerting and filtering

CHAPTERS
00:00 Solution structure — two layers
01:00 Swagger and the API surface
02:00 Startup menu and three pillars
02:45 Navigation between views
03:15 Sensor registration with a config file
04:45 Registration with a deployment photo
05:30 Uploaded files on disk
06:00 Input validation
07:00 Generics — TelemetryPacket<T>
09:00 Operator overloading
10:00 Jagged arrays and batch storage
11:30 Recursive deployment validation
13:00 Live telemetry dashboard
15:00 Interface design and responsiveness
16:00 Collections
16:45 Repository and README
17:45 Summary

TECH STACK
.NET 10 · ASP.NET Core Minimal APIs · .NET MAUI · Swagger / OpenAPI · Docker
```

> Those chapter times come from the planned step durations. Watch the video back and correct them — YouTube only creates chapters if the first one is `00:00` and there are at least three, each at least 10 seconds apart.

---

# Things that cost marks on camera

| Risk | Prevention |
|---|---|
| App can't reach the API | Confirm the pill is green before recording (Part A) |
| Swagger logs `string string=0 string` | Overwrite the example body every time |
| Anomaly card never lights up | Use the exact values in Step 13 — don't improvise |
| `sensorType` misspelled | Must be exactly `Moisture`, `Wattage` or `Valve` |
| Build errors mid-demo | Clean build immediately before recording |
| Reading code line by line | Explain what it does and why; keep each file under 45s on screen |

# ZeroCool Gardening

A cross-platform **.NET 10** garden planning application that runs as a **Web app**, a **Windows desktop app**, and an **Android app** from one shared Blazor Hybrid UI.

## Features
- **Yard & Grid Designer:** Create yards of any size, paint custom yard shapes on a grid, drag and drop garden beds and pavers/walkways, and toggle between **Soil View** and **Plants View**.
- **Three Gardening Styles:**
  - **Square-Foot Gardening:** 1×1 ft cells with automatic density defaults (1, 4, 9, 16/sq ft) and multi-cell plant spans (e.g. 2×2 tomatoes, 2×3 winter squash).
  - **Row Gardening:** Enter seed-packet in-row spacing, between-row spacing, and seed depth to automatically calculate rows and plants per row (horizontal or vertical).
  - **Container Gardening:** Round pots, planter boxes, and grow bags with volume presets (1–25 gal) and plant-count guidance.
- **Companion & Antagonist Highlighting:** Green edges highlight adjacent companion cells/rows and red edges highlight adjacent antagonist conflicts — strictly along the shared boundary between affected cells.
- **64+ Plants & 157+ Varieties (No Trees):** Vegetables, herbs, berries/fruits, flowers, ornamentals, groundcovers, and vines — each with spacing, USDA zones, companions/antagonists, health benefits, and ASPCA/safety warnings. Every plant and variety is user-editable (add, edit, delete, reset to default).
- **USDA Growing Zones & Planting Calendar:** Offline USDA Zones 1–13 frost-date lookup (with custom frost-date overrides), automated **Start indoors / Direct sow / Transplant / Harvest** schedules, actual-date tracking, and **`.ics` calendar export**.
- **Templates & JSON Persistence:** Debounced autosave to JSON files (per-browser server storage on Web; local app-data folder on Windows/Android), starter & custom templates, and JSON plan/backup import & export.

## Solution Structure
```
ZeroCoolGardening.slnx
 ├─ src/ZeroCool.Core     # Models, layout engines, adjacency, zone/calendar logic
 ├─ src/ZeroCool.Data     # Atomic JSON storage, repositories, 64-plant seed, import/export
 ├─ src/ZeroCool.UI       # Shared Razor Class Library (pages, SVG surfaces, bundled SVG icons)
 ├─ src/ZeroCool.Web      # ASP.NET Core Blazor Server host (per-client App_Data JSON storage)
 ├─ src/ZeroCool.Maui     # .NET MAUI Blazor Hybrid host (Windows + Android)
 └─ tests/ZeroCool.Tests  # xUnit test suite
```

## Prerequisites
- **.NET 10 SDK** (pinned via `global.json`)
- For desktop/mobile builds: `.NET MAUI` workloads:
  ```powershell
  dotnet workload install maui-windows maui-android
  ```

## Running & Building

### Web App
```powershell
dotnet run --project src/ZeroCool.Web
```

### Windows Desktop (MAUI)
```powershell
dotnet build src/ZeroCool.Maui -f net10.0-windows10.0.19041.0
dotnet run --project src/ZeroCool.Maui -f net10.0-windows10.0.19041.0
```

### Android (MAUI)
```powershell
dotnet build src/ZeroCool.Maui -f net10.0-android
# Package release APK/AAB:
dotnet publish src/ZeroCool.Maui -f net10.0-android -c Release
```

### Tests
```powershell
dotnet test tests/ZeroCool.Tests
```

## Credits & Licenses
See [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md) and the in-app **About & Credits** screen (`/about`) for icon attributions (**OpenMoji** CC BY-SA 4.0, **game-icons.net** CC BY 3.0, and **Icon Monks** MIT).

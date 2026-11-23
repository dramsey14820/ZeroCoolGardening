# ZeroCoolGardening – Project Plan

**Status: Approved as Version 1 (2026-11-22).**

## 1. Vision
A garden-planning app that runs as a **web app**, a **Windows desktop app**, and an **Android app** from one shared codebase on **.NET 10**. Users design a yard, add beds (row, square-foot, container), drag and drop plants and features, and get companion/antagonist feedback based on their growing zone.

## 2. Decisions (from Q&A)
| Topic | Decision |
|---|---|
| Architecture | **Blazor Hybrid**: shared Razor Class Library (RCL) UI hosted by MAUI (Windows, Android) and an ASP.NET Core web host. Note: MAUI cannot host MVC views, so the UI is Razor components. The ASP.NET Core host still uses MVC-style controllers/minimal APIs for file endpoints. |
| Storage | JSON files. Web: stored on the server, per browser session/client id. Desktop/Android: local app-data folder. Desktop users can upload and download (import/export) JSON files. |
| Accounts/sync | None in v1. Import/export now; sync later. |
| Plant data | Curated seed JSON (~50–100 plants: USDA zones, spacing, companions, antagonists), editable by the user. |
| Icons | Bundled open-license SVGs in `garden-icons/`: **OpenMoji** (CC BY-SA 4.0; modified icons stay CC BY-SA 4.0) and **game-icons.net** (CC BY 3.0; Lorc, Delapouite & contributors), plus **Icon Monks** (MIT, Design Monks; credit shown anyway, and `iconmonks-svg/LICENSE.md` must stay with the icons). **Required:** credits shown on an About & Credits screen (`/about`, linked from the hamburger menu) and in `THIRD-PARTY-NOTICES.md`. |
| Platforms | Web, Windows (MAUI), Android (MAUI). Imperial units by default, metric optional. macOS/iOS/Linux are out of scope. |
| Autosave | Debounced save after every change. |

## 3. Solution Structure
```
ZeroCoolGardening.sln
 ├─ src/ZeroCool.Core            (models, services, interfaces, companion/zone logic; no UI)
 ├─ src/ZeroCool.Data            (JSON repositories, seed data, import/export)
 ├─ src/ZeroCool.UI              (Razor Class Library: pages, components, canvas, CSS, JS interop)
 ├─ src/ZeroCool.Web             (ASP.NET Core host; file-storage API; Blazor Server first)
 ├─ src/ZeroCool.Maui            (MAUI BlazorWebView: Windows + Android; local file storage)
 └─ tests/ZeroCool.Tests         (xUnit/bUnit)
```
Storage abstraction: `IStorageProvider` has a `ServerFileStorage` (web), a `LocalFileStorage` (MAUI) and an import/export service.

## 4. Domain Model (JSON)
- **GardenPlan / Yard**: id, name, zone, units, grid (width × height, cell size, per-cell active mask for custom shapes), beds, features.
- **Bed**: id, name, position/size on yard grid, type (`Row | SquareFoot | Container`), soil type, fertilizer (both optional), cells/rows/plantings.
- **Planting**: plant id, cell span (w×h), position, planted date (optional).
- **Feature**: paver/walkway, path, structure, water, etc.: icon, size, position.
- **Plant** (catalog, species level): name, category, icon, zones, sun, water, soil preferences, row spacing, plant spacing, plants per square foot, cell span, days to maturity, companions[], antagonists[], notes, varieties[], `isUserDefined`.
- **Plant categories**: vegetables, herbs, fruits/berries (non-tree), flowers, ornamentals/grasses, groundcovers, vines. **Trees are out of scope.**
- **Plant info fields (Plant and Variety)**: `healthBenefits[]` (e.g. nutrients, culinary/medicinal notes), `warnings[]` (each with type and severity: toxic to pets/humans/children, skin irritant/sap, allergen, invasive/aggressive spreader, thorny, unsafe parts such as raw/leaves, etc.), `edible` flag and edible parts, and `sources[]`. Health content is informational, not medical advice.
- **Variety** (child of Plant): name, description, days to maturity, days to germination, spacing/cell-span overrides, sun/zone overrides, planting-calendar offsets (see Calendar), optional icon/color, notes, `isUserDefined`. Any field not set inherits from the parent Plant.
- **Planting** references a plant id and an optional variety id.
- **CalendarRule** (on Plant or Variety): start indoors / direct sow / transplant / harvest windows, expressed as weeks relative to the zone's last/first frost date, plus succession interval (optional).
- **Template**: a GardenPlan flagged as a template; built-in templates plus user templates.
- **Settings**: zone, units, theme.

Files: `settings.json`, `plants.json` (seed plus user overrides), `plans/{id}.json`, `templates/{id}.json`.

## 5. Feature Breakdown
### 5.1 Yard and grid
- Create a yard and define its shape in a grid (paint or erase cells; rectangle presets).
- Add beds by drag/drop or the (+) button. Resize, rotate, and snap to the grid.

### 5.2 Beds
- Choose type, soil, and fertilizer (optional) per bed.
- **Soil view / Plant view** toggle on the design surface (soil is color-coded and labeled).

### 5.3 Row gardening
- User enters seed-packet spacing (in-row spacing, row spacing, and optionally depth). The bed layout is auto-computed: number of rows and plants per row for the bed dimensions.
- Rows can be re-spaced when the values change.

### 5.4 Square-foot gardening
- 1 ft × 1 ft cells. Defaults come from the plant's recommended plants-per-square-foot (derived from the packet spacing, e.g. 1, 4, 9, 16 per cell), or the user sets the count per cell.
- **Multi-cell plants** (squash, tomatoes, etc.) occupy N×M cells.
- Plant icons are shown in cells. Clicking a plant opens an **info dialog**.

### 5.5 Containers
- Container size/shape/volume. Plants-per-container guidance and a single drop target.

### 5.6 Growing zone
- The user sets a USDA zone. The app filters or flags plants by suitability, and shows planting windows and frost-date guidance from the zone data.

### 5.6a Plant and variety management (always available)
- Users can **add, edit, and delete** plants **and varieties**, including all their information (spacing, zones, sun, days to maturity, companions/antagonists, calendar rules, icon, notes). Accessible from the hamburger menu and from the plant info dialog.
- Seed data is editable too: edits are stored as user overrides (a "Reset to default" action restores the original). Deleting a built-in plant hides it (restorable).
- The (+) palette and the placement flow let users pick a plant, then optionally a **variety** (e.g. Tomato → Roma, Cherokee Purple). Variety-specific spacing, cell span and dates are used when set.
- Deleting a plant or variety that is in use prompts the user (keep placements as "custom/unknown" or remove them).

### 5.6c Health benefits and warnings
- The plant/variety info dialog has **Health Benefits** and **Warnings** sections; warnings show badge icons (e.g. pet-toxic, skin irritant, invasive) and also appear on the placement card and the (+) palette.
- Both are fully user-editable (add/remove/edit) like all other plant data, with a "not medical advice" disclaimer and cited sources.
- Non-edible ornamentals use the same fields (e.g. toxicity and invasiveness warnings).

### 5.6b Planting calendar
- A Calendar page (hamburger menu) shows, for the user's zone and each plant/variety in their plans: **start indoors, direct sow, transplant, and harvest** dates (month/week view and a per-plan list).
- Dates are computed from the zone's average last/first frost dates plus the plant's/variety's CalendarRule and days to maturity. Users can override the frost dates and any individual date, and may record actual planted/transplanted/harvested dates.
- Calendar rules are editable per plant and variety (see 5.6a). Optional export to .ics.
- Research: per-plant and per-variety timing from extension-service sources, plus zone → frost-date table (see Section 7).

### 5.7 Companion / antagonist highlighting
- Between **adjacent cells only**: a **green** edge for companions, a **red** edge for antagonists.
- Rendered on the shared edge between the two cells (not the whole cell). Optional hover tooltip with the reason.

### 5.8 Add (+) menu
- After setup, a floating (+) opens a palette of plants, pavers/walkways, and other features. Drag from it or tap to place.

### 5.9 Responsive UI
- Desktop: side palette, properties panel, mouse drag/drop and wheel zoom.
- Phone: bottom sheet palette, tap-to-select then tap-to-place (as a fallback to drag), pinch zoom and pan, large touch targets.

### 5.10 Hamburger menu / Edit screen
- Manage plans: add, edit, delete, duplicate.
- Save a plan as a template. Start a new plan from a template.
- Edit the plant catalog. Settings (zone, units). Import/export JSON.

### 5.11 Persistence
- Autosave with debounce, atomic writes (temp file then rename), schema `version` field and migrations.
- Desktop/Android: import/export through the file picker/share sheet. Web: upload/download.

## 6. Technical Approach
- **Design surface**: SVG (or HTML Canvas) in Razor with JS interop for pointer events (unified mouse/touch via Pointer Events), snap-to-grid, and zoom/pan.
- **State**: a scoped `GardenState` service with undo/redo (command stack).
- **Rendering**: layered (grid, soil layer, plants, features, adjacency highlights, selection).
- **Adjacency engine**: a pure function in Core that checks cell neighbors (4-neighbor) against the companion tables and returns edge highlights. Heavily unit-tested.
- **Row layout engine**: a pure function in Core: bed dimensions + spacing → positions.
- **Web storage**: the web host exposes `/api/files` endpoints; files are stored under `App_Data/{clientId}/`. Keep the client id as a cookie. Validate and sanitize file names and sizes.
- **Testing**: xUnit for Core logic, bUnit for components, manual device tests.

## 7. Research Tasks (before/while coding)
1. Build the plant seed dataset (50–100 plants): spacing, plants per sq ft, cell span, zones, sun, companions, antagonists. Use reputable sources (university extension services, Almanac, USDA).
1c. Health benefits and safety/toxicity warnings (human, pet, allergen, invasive) per plant/variety from reputable sources (extension services, ASPCA, USDA, NIH).
1d. Cover vegetables, herbs, fruits/berries, flowers, and ornamentals in the seed set (no trees).
1a. Variety data for the most common plants (e.g. 3–6 popular varieties each: days to maturity, spacing differences), with source citations.
1b. Planting timing rules (start indoors / sow / transplant / harvest, weeks relative to frost) per plant and variety.
2. USDA zone data (frost dates, planting windows). Offline zone → frost-date table. Provide a ZIP lookup later if wanted.
3. Soil and fertilizer type list (loam, clay, sandy, raised-bed mix; N-P-K, compost, etc.).
4. Icon set chosen: OpenMoji + game-icons.net (licenses confirmed; attribution required, see Section 2). Keep the About & Credits screen up to date with any new icon source.

## 8. Milestones
| # | Milestone | Deliverables |
|---|---|---|
| M0 | Setup | Solution, projects, CI build, .NET 10 and MAUI workloads, Hello World on all 3 targets |
| M1 | Core and data | Models, JSON repositories, autosave, seed data loader, tests |
| M2 | Yard and grid | Yard shape editor, beds, drag/drop surface, soil/plant views |
| M3 | Square-foot beds | Plant placement, multi-cell span, icons, info dialog |
| M4 | Row and container beds | Spacing calculator, containers |
| M5 | Zones and companions | Zone setting, filtering, green/red edge highlights |
| M6 | (+) menu, hamburger, templates | Plan CRUD, templates, plant and variety editor (add/edit/delete, reset to default), import/export |
| M6b | Calendar | Frost-date based planting calendar (indoors/sow/transplant/harvest), date overrides, actual-date tracking, .ics export |
| M7 | Responsive and platform polish | Phone UX, Windows packaging, Android APK/AAB, web deploy |
| M8 | QA and release | Testing, accessibility, docs |

## 9. Risks
- Touch drag/drop inside the BlazorWebView and on mobile browsers. Mitigation: Pointer Events plus a tap-to-place fallback.
- The plant data must be accurate and complete. Mitigation: cite sources in the data, and make it user-editable.
- Web storage without accounts: files are tied to a client id. Mitigation: clear export/import, and add auth later.
- MAUI workload/tooling versions with .NET 10. Mitigation: pin the SDK in `global.json`.

## 10. Out of Scope for v1
Trees, accounts/cloud sync, iOS/macOS/Linux, online plant APIs, weather integration, and AI suggestions.

## 11. Open Items
- Blazor Server vs WebAssembly for the web host (default: Server for simplicity; revisit for offline).
- Final icon set choice.
- App name, branding and license.


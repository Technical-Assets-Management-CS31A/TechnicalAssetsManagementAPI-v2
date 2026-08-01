# Backend Technical Equipment Borrowing System

ASP.NET Core 10 Web API for managing a school's technical equipment inventory (HDMI cables, extensions, keys, AC remotes, etc.), borrowing/return workflows, and RFID-based hardware integration.

This is a clean remake of the earlier `BackendTechnicalAssetsManagement` project: same core problem, same layered architecture (Controller → Service → Repository), but with a cleaner domain model, consistent naming, and fixes for issues found in the original.

> **Status:** Design phase. This README describes the target design; implementation has not started yet. ESP32/hardware firmware details are intentionally out of scope here and will get their own spec.

## Why the remake

The original project accumulated some rough edges over time:

- User types (`Student`/`Teacher`/`Staff`) used EF Core TPT inheritance directly off a catch-all `User`, mixing admin roles in with profile subtypes.
- Some statuses were enums, others were raw strings.
- An unused `RefreshTokenMiddleware` sat registered but dead.
- Leftover template files (`WeatherForecastController`) never got cleaned up.
- Entity names like `LentItems` didn't fit the domain well (reads like a financial loan, not an equipment checkout).
- Vulnerable dependency versions (AutoMapper) went unpatched.

This remake fixes those in place rather than carrying them forward.

## Tech stack

- **Framework:** .NET 10 / ASP.NET Core Web API
- **Database:** PostgreSQL (Supabase-hosted) via EF Core + Npgsql
- **Auth:** JWT bearer tokens + refresh tokens (handled in `AuthService`, no middleware), BCrypt password hashing
- **Realtime:** SignalR (`/notificationHub`)
- **Storage:** Supabase Storage for item/user images
- **Mapping:** AutoMapper (kept current, patched)
- **Bulk import:** ExcelDataReader for `.xlsx` inventory ingestion
- **API docs:** Swagger/OpenAPI + Scalar

## Architecture

Single Web API project, layered by responsibility with folders directly under the project directory:

```
Controllers/     HTTP endpoints
Services/        Business rules
IService/        Service interfaces
Repository/      EF Core data access
IRepository/     Repository interfaces
Entities/        Domain/database models (renamed from "Classes")
DTOs/            Request/response contracts
Profiles/        AutoMapper profiles
Data/            DbContext
Hubs/            SignalR hubs
Middleware/      Global exception handling
Authorization/   Custom policy handlers
Filters/         Swagger/OpenAPI filters
Utils/           Small shared helpers
```

No dead middleware, no leftover template files. Every status field (item, borrowing, session) is a proper C# enum stored via EF Core enum-to-string conversion — no raw strings.

## Domain model

### Users

A single `Users` table holds shared fields (name, email, role, login/blocked state). `Admin` and `SuperAdmin` live directly as `User` rows — they don't need extra profile data, so no subtype.

Two subtypes via EF Core TPT (separate physical tables joined by ID), kept apart for maintainability:

- **`Student`** — student-specific fields (student ID number, etc.)
- **`Faculty`** — faculty-specific fields, plus a `Position` enum (`Teacher`, `StaffUtilities`, `AdmissionStaff`, ...) so faculty sub-roles are just a categorization tag rather than more subtypes.

### Equipment & borrowing

- **`Item`** — a piece of equipment: serial number, optional RFID UID, category, condition, status, location, image.
- **`Borrowing`** — a borrow or reservation record for an item (renamed from `LentItems` — "Loan" reads as financial, this is equipment checkout).
- **`ActivityLog`** — audit trail for actions across the system.

### Archiving

Archiving moves a record out of its live table into a dedicated archive table, preserving history while keeping the live tables lean as data grows — active queries never scan archived rows:

- **`ArchivedUser`**, **`ArchivedItem`**, **`ArchivedBorrowing`**

### RFID / hardware session workflow

The web/mobile UI and the borrowing station coordinate through short-lived, database-backed session records — no direct device connections. Each session is one row the station updates as the scan/capture completes. The station is a PC running the web UI with a **webcam** for face capture, plus an ESP32 + RFID reader for the card/item scans.

Borrowing is moving toward a **three-input capture** at the station: the student's face (image), their RFID card, and the item's RFID tag. The face image is captured browser-side from the webcam and uploaded to Supabase Storage; the borrow session row holds the image reference plus both scanned UIDs.

Session records (each keeps its own table and only the fields it needs):

- **`ItemRfidRegistration`** — bind an RFID tag to an item
- **`StudentRfidRegistration`** — enroll a student's RFID card (and reference face image, for later matching)
- **`HardwareBorrowSession`** — web-initiated borrow, completed at the station by the three-input capture (face + card + item)
- **`HardwareReturnSession`** — web-initiated return, completed by a scan at the station
- **`GuestScanSession`** — guest borrowing driven by an item scan (no card/face)
- **`RfidTag`** — a physical RFID tag record

All session types share one lifecycle — `Pending → Completed | Failed | Cancelled` with a ~5-minute expiry — via a common `ISession` interface and a single expiry sweep, so the state/expiry logic lives in one place instead of being copied per table.

> The face **matching/recognition** engine (how a captured face is compared against enrolled images, and where it runs) is out of scope here and gets its own spec alongside the firmware — the API side only captures and stores the image plus the scan UIDs.

## Features

- **Equipment management** — serials, RFID tags, categories, conditions, status, images, location tracking
- **Borrowing & returns** — reserve, approve/deny, borrow, return, with automatic expiry of unclaimed reservations
- **RFID hardware integration** — session-based coordination between web/mobile and ESP32 stations (see separate hardware spec)
- **Real-time notifications** — SignalR push for borrow/return/approval/status events
- **Live stock summary** — aggregated inventory counts factoring in active borrows
- **Excel bulk import** — `.xlsx` ingestion for inventory seeding
- **RBAC** — SuperAdmin, Admin, Staff-type roles, Faculty (with Position), Student, plus resource-based policies (e.g. users can view their own profile; privileged roles can view any)
- **Activity logging & archiving** — full audit trail, archive-and-restore instead of hard deletes

## Getting started

> Setup instructions will be filled in once the project scaffolding lands. Expected shape, based on the previous project:

```bash
dotnet restore
dotnet ef database update
dotnet watch run --launch-profile http
```

Config comes from `.env` / `appsettings.{Environment}.json`:

```env
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection="Host=...;Database=postgres;Username=postgres;Password=..."
Jwt__Key=YourSuperSecretKeyThatIsAtLeast32Bytes!
Jwt__Issuer=TechnicalEquipmentAPI
```

## Testing

Light coverage focused on the riskiest logic rather than exhaustive suites: auth flow, borrowing/return state transitions, and archiving. Expand as issues surface.

## Out of scope (separate spec)

- ESP32 firmware and device-side implementation
- Device authentication for hardware endpoints (known gap carried from the original project; to be addressed alongside the firmware spec)

## Related project

- `BackendTechnicalAssetsManagement` — the original system this remake replaces. See its own README for the as-built reference during the port.

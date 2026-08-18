# GymFlow — Modular Monolith Restructure + Multi-Role Identity

This replaces your single `GymFlow.API` project with 4 projects, and folds in the
User/Gym-approval work from the previous step at the same time.

```
GymFlow.sln
├── GymFlow.Domain          entities + enums only, zero dependencies
├── GymFlow.Application     interfaces + DTOs, organized by Modules/, references Domain only
├── GymFlow.Infrastructure  AppDbContext, DbSeeder, concrete services, references Application
└── GymFlow.API             Controllers, Middleware, Program.cs, references Application + Infrastructure
```

Dependency direction: `API → Infrastructure → Application → Domain`. Nothing points backwards.

## What changed functionally (same as before, now in the new layout)
- `AdminUser` → `User` (unified login for SuperAdmin / GymAdmin / Trainer / Member), `GymId` nullable
- `Gym.Status` gates login (`PendingApproval` → `Active` / `Suspended`)
- `POST /api/auth/register-gym` — public onboarding, creates Gym + GymAdmin atomically, no auto-login
- `GET/PATCH /api/gyms` — SuperAdmin-only approve/suspend, now routed through `IGymService` (Application) →
  `GymService` (Infrastructure) instead of the controller touching `AppDbContext` directly
- Seeded demo gym is `Active` (so `admin@gymflow.com` / `Admin@123` still logs in immediately)
- New seeded SuperAdmin: `superadmin@gymflow.com` / `SuperAdmin@123`, `GymId = null` — use this to test the approval flow end-to-end

## Steps to swap this into Visual Studio

1. **Back up your current project first** (or just work from a fresh git branch).
2. In your repo root, delete the old `GymFlow.API` folder and `GymFlow_API.sln` — or better, rename them to `.bak` so you have a fallback while you verify the new layout builds.
3. Copy the entire `GymFlow-Solution` folder contents (all 4 project folders + `GymFlow.sln`) into your repo root.
4. Open `GymFlow.sln` in Visual Studio. All 4 projects should load; project references are already wired.
5. **Migrations**: your old `Migrations/` folder (`20260618171904_InitialCreate.cs` + snapshot) referenced the old `AdminUser`/single-project namespace and won't apply cleanly to the new schema. Simplest path given this is still pre-launch with mostly seed data:
   - Delete the old `Migrations` folder entirely (don't copy it into `GymFlow.Infrastructure/Persistence/Migrations`)
   - In Package Manager Console, set **Default project** to `GymFlow.Infrastructure` and **Startup project** (right-click solution → Properties) to `GymFlow.API`
   - Run:
     ```
     Add-Migration InitialCreate -Project GymFlow.Infrastructure -StartupProject GymFlow.API
     Update-Database -Project GymFlow.Infrastructure -StartupProject GymFlow.API
     ```
   - Or via CLI from the repo root: `dotnet ef migrations add InitialCreate --project GymFlow.Infrastructure --startup-project GymFlow.API`
6. Copy `appsettings.Development.json` (not uploaded, so not included here) from your old API project into the new `GymFlow.API/` folder.
7. `dotnet build` (or Build Solution in VS) — I can't compile this here since NuGet isn't reachable from this environment, so treat this as a first-pass migration. Likely small things to fix on your end:
   - Any `.csproj` files that had additional packages I didn't see (only the 5 shown were carried over)
   - GitHub Actions workflow (`.github/workflows/...`) if it references the old single-project path for build/publish steps
8. Run the API and confirm:
   - Swagger loads at `/swagger`
   - `POST /api/auth/login` with `admin@gymflow.com` / `Admin@123` works
   - `POST /api/auth/login` with `superadmin@gymflow.com` / `SuperAdmin@123` works
   - `POST /api/auth/register-gym` creates a new gym, and logging in with it returns `403 pending_approval` until you `PATCH /api/gyms/{id}/activate` as the SuperAdmin

## Notes / things worth knowing
- **AutoMapper** is registered in `Program.cs` but I didn't see any `Profile` classes in the files you shared — if you're not actually using it yet, it's safe to leave as-is (harmless) or remove later.
- **Stub services** (Finance/Inventory/Product/PromoCode/Staff Create/Update) still return placeholder objects, carried over exactly as they were — this restructure didn't change their behavior, only their location (`GymFlow.Infrastructure/Services/StubServices.cs`).
- The **SQL Server package reference** you had is unused (commented out in `Program.cs`, Postgres is active) — I dropped it from the new `GymFlow.Infrastructure.csproj` to avoid an unused dependency. Let me know if you actually need dual-provider support.

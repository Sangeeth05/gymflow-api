# 🏋️ GymFlow — Gym Management Admin Portal

> **A production-ready, full-stack gym management system.** One portal to manage members, finances, inventory, products, promotions, staff, and analytics.

![React](https://img.shields.io/badge/React-18-61DAFB?logo=react) ![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?logo=typescript) ![.NET](https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet) ![SQL Server](https://img.shields.io/badge/SQL%20Server-2022-CC2927?logo=microsoftsqlserver) ![Tailwind](https://img.shields.io/badge/Tailwind-3-06B6D4?logo=tailwindcss)

---

## 📋 Table of Contents

1. [Business Model & Vision](#-business-model--vision)
2. [Feature Overview](#-feature-overview)
3. [Tech Stack Decision](#-tech-stack-decision)
4. [Project Structure](#-project-structure)
5. [Quick Start (Mock Mode — No Backend Needed)](#-quick-start-mock-mode)
6. [Full Stack Setup](#-full-stack-setup)
7. [Docker Setup](#-docker-setup)
8. [API Documentation](#-api-documentation)
9. [Database Schema](#-database-schema)
10. [Environment Variables](#-environment-variables)
11. [Roadmap](#-roadmap)

---

## 🚀 Business Model & Vision

**GymFlow** is designed as a **SaaS platform for gym owners globally** — a single portal replacing the fragmented tools gyms currently use (spreadsheets, separate billing apps, WhatsApp for member communication, manual inventory tracking).

### Target Market
| Segment | Description |
|---|---|
| **Primary** | Independent gym owners (50–500 members) |
| **Secondary** | Mid-size fitness chains (multi-branch) |
| **Global** | India, UAE, Southeast Asia, Africa (high gym-density, low-SaaS penetration) |

### Revenue Model
- **Subscription SaaS** — ₹999–₹4,999/month per gym (tiered by member count)
- **Transaction fee** — 0.5% on digital payment processing (future)
- **Mobile app** — Member-facing app (planned Phase 2)
- **Marketplace** — Commission on supplement/product sales through platform

### Competitive Advantage vs. Existing Tools
| Feature | GymFlow | Mindbody | Gym Master | Spreadsheets |
|---|---|---|---|---|
| Member management | ✅ | ✅ | ✅ | ❌ |
| Finance & billing | ✅ | ✅ | ✅ | ❌ |
| Inventory tracking | ✅ | ❌ | ❌ | ❌ |
| Promo codes | ✅ | ✅ | ❌ | ❌ |
| Brand product catalogue | ✅ | ❌ | ❌ | ❌ |
| Staff management | ✅ | ✅ | ✅ | ❌ |
| Analytics & reports | ✅ | ✅ | ❌ | ❌ |
| Indian payment methods (UPI) | ✅ | ❌ | ❌ | ❌ |
| Open API | ✅ | Partial | ❌ | ❌ |
| Price (INR/month) | ₹999+ | ₹8,000+ | ₹5,000+ | Free |

---

## ✨ Feature Overview

### 1. 📊 Dashboard
- Real-time KPIs: active members, revenue, check-ins, occupancy
- Revenue vs. expenses area chart (6-month trend)
- Revenue split by category (pie chart)
- Recent activity feed
- Quick overview progress bars

### 2. 👥 Members
- Full CRUD — add, edit, view, delete members
- Member ID auto-generation (GF-0001 format)
- Status management: Active / Inactive / Suspended / Expired
- Membership plan assignment with auto-expiry calculation
- Emergency contact tracking
- Search, filter by status, sort
- Detailed member profile modal
- Check-in recording

### 3. 💳 Finance
- Transaction ledger with full audit trail
- Payment status: Paid / Pending / Overdue / Refunded
- Payment methods: Cash, Card, UPI, Bank Transfer, Cheque
- Transaction types: Membership, Product Sale, PT Session, Locker, Other
- Revenue summary cards
- Month-over-month comparison

### 4. 📦 Inventory
- SKU-based tracking for equipment, supplements, accessories
- Auto stock status: In Stock / Low Stock / Out of Stock
- Min quantity alerts
- Purchase price vs. selling price
- Supplier and location tracking
- Last restocked date

### 5. 🛍️ Products
- Gym brand product catalogue
- Featured products, ratings, reviews
- Discount / original price tracking
- Category-based browsing
- Stock management

### 6. 🎟️ Promotions
- Promo code creation with percentage or fixed discounts
- Usage limits and tracking (used/max with progress bar)
- Validity windows (from/to dates)
- Auto status: Active / Scheduled / Expired / Inactive
- Target audience: All / New Members / Renewal Only / Products Only
- One-click code copy

### 7. 👨‍💼 Staff
- Staff profiles with role assignment
- Specialization tags for trainers
- Salary tracking
- Monthly payroll summary

### 8. 📈 Reports & Analytics
- Revenue vs. expenses bar chart
- Member growth (new vs. churned) line chart
- Plan distribution pie chart
- Peak hours analysis bar chart
- Monthly summary table with profit margins
- Export ready (wired for PDF/Excel in production)

### 9. ⚙️ Settings
- Gym profile management (name, address, hours, capacity, currency)
- Notification preferences with toggles
- Password change
- Appearance (theme, accent color, sidebar width)

---

## 🛠 Tech Stack Decision

### Why React + TypeScript?
> You have 4.4 years of React/TS experience — this is the right call. No learning curve, maximum productivity.

| Choice | Rationale |
|---|---|
| **React 18** | Component ecosystem, your expertise |
| **TypeScript** | Type safety across the entire codebase — critical for a business app |
| **Zustand** | Lightweight state management (auth store) — simpler than Redux |
| **TanStack Query** | Server state, caching, background refetch |
| **React Router v6** | Nested routes, protected route pattern |
| **Tailwind CSS v3** | Utility-first — fast iteration, dark theme, consistent design tokens |
| **Recharts** | React-native charting, SVG-based |
| **React Hot Toast** | Beautiful notifications |
| **Axios** | HTTP client with interceptors for auth |
| **date-fns** | Lightweight date formatting |
| **Lucide React** | Clean, consistent icon set |

### Why .NET 8?
| Choice | Rationale |
|---|---|
| **ASP.NET Core 8** | Highest-performance web API framework, industry-standard for enterprise |
| **Entity Framework Core 8** | Code-first migrations, LINQ queries, SQL Server support |
| **SQL Server** | ACID compliance, JSON support, scalable for SaaS multi-tenancy |
| **JWT + Refresh Tokens** | Stateless auth with token rotation for security |
| **BCrypt** | Industry-standard password hashing |

### Monorepo vs. Separate Repos?

**Recommendation: Monorepo (what this project uses)**

| | Monorepo | Separate Repos |
|---|---|---|
| **Local dev** | ✅ One `git clone`, one IDE window | ❌ Multiple clones, context switching |
| **Shared types** | ✅ Can share TS types with backend via codegen | ❌ Manual sync |
| **CI/CD** | ✅ Single pipeline, deploy together | ✅ Independent deploy (better at scale) |
| **Team size** | ✅ Best for 1–5 devs | ✅ Better for 10+ devs |
| **Recommended for** | **Your stage (solo/small team)** | Large orgs, microservices |

**Verdict**: Stay monorepo until you have separate frontend and backend teams.

---

## 📁 Project Structure

```
gymflow/
├── frontend/                   # React + TypeScript app
│   ├── src/
│   │   ├── api/
│   │   │   ├── client.ts       # Axios instance with JWT interceptors
│   │   │   ├── services.ts     # All API calls (+ mock mode)
│   │   │   └── mockData.ts     # Rich mock data for demo/dev
│   │   ├── components/
│   │   │   ├── auth/
│   │   │   │   └── ProtectedRoute.tsx
│   │   │   ├── layout/
│   │   │   │   ├── MainLayout.tsx
│   │   │   │   └── Sidebar.tsx
│   │   │   └── ui/
│   │   │       └── index.tsx   # Button, Badge, Modal, Input, Card, etc.
│   │   ├── pages/
│   │   │   ├── LoginPage.tsx
│   │   │   ├── Dashboard/
│   │   │   ├── Members/
│   │   │   ├── Finance/
│   │   │   ├── Inventory/
│   │   │   ├── Products/
│   │   │   ├── Promotions/
│   │   │   ├── Staff/
│   │   │   ├── Reports/
│   │   │   └── Settings/
│   │   ├── store/
│   │   │   └── authStore.ts    # Zustand auth store (persisted)
│   │   └── types/
│   │       └── index.ts        # All TypeScript interfaces
│   ├── .env.example
│   ├── Dockerfile
│   ├── nginx.conf
│   └── tailwind.config.js
│
├── backend/
│   └── GymFlow.API/            # ASP.NET Core 8 Web API
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   ├── MembersController.cs
│       │   └── OtherControllers.cs  # Dashboard, Finance, Inventory, Products, Promo, Staff
│       ├── Data/
│       │   ├── AppDbContext.cs      # EF Core DbContext
│       │   └── DbSeeder.cs         # Initial seed data
│       ├── DTOs/
│       │   └── Dtos.cs
│       ├── Middleware/
│       │   └── ExceptionMiddleware.cs
│       ├── Models/
│       │   └── Entities.cs         # All domain entities
│       ├── Services/
│       │   ├── IServices.cs        # Service interfaces
│       │   ├── AuthService.cs      # JWT auth implementation
│       │   └── Services.cs         # All other service implementations
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── Dockerfile
│       └── Program.cs
│
├── docker-compose.yml
├── .gitignore
└── README.md                   # This file
```

---

## ⚡ Quick Start (Mock Mode)

**Run the frontend instantly — no backend, no database required.**

```bash
# 1. Clone / unzip the project
cd gymflow

# 2. Install frontend dependencies
cd frontend
npm install

# 3. Create environment file
cp .env.example .env
# Make sure REACT_APP_USE_MOCK=true in .env

# 4. Start
npm start
```

Open **http://localhost:3000**

### Demo Login Credentials
| Field | Value |
|---|---|
| **Email** | `admin@gymflow.com` |
| **Password** | `Admin@123` |

> In mock mode, all data is in-memory. Changes (add member, etc.) persist only for the current browser session.

---

## 🔧 Full Stack Setup

### Prerequisites

| Tool | Version | Download |
|---|---|---|
| Node.js | 18+ | https://nodejs.org |
| .NET SDK | 8.0 | https://dotnet.microsoft.com/download |
| SQL Server | 2019+ or 2022 | https://www.microsoft.com/sql-server |
| EF Core Tools | Latest | `dotnet tool install -g dotnet-ef` |

---

### Step 1 — Frontend Setup

```bash
cd gymflow/frontend
npm install
cp .env.example .env
```

Edit `.env`:
```env
REACT_APP_API_URL=http://localhost:5000/api
REACT_APP_USE_MOCK=false     # Switch to real backend
```

```bash
npm start     # http://localhost:3000
```

---

### Step 2 — Backend Setup

#### 2a. Create the solution and install packages

```bash
cd gymflow/backend

# Create solution
dotnet new sln -n GymFlow

# Add the API project to solution
dotnet sln add GymFlow.API/GymFlow.API.csproj

# Restore packages
cd GymFlow.API
dotnet restore
```

#### 2b. Configure connection string

Edit `appsettings.Development.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=GymFlowDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

> **Using SQL Server with password?**
> ```
> Server=localhost,1433;Database=GymFlowDB;User Id=sa;Password=YourPassword;TrustServerCertificate=True;
> ```

#### 2c. Run database migrations

```bash
# From backend/GymFlow.API directory
dotnet ef migrations add InitialCreate
dotnet ef database update
```

This will:
- Create the `GymFlowDB` database
- Create all tables
- Run `DbSeeder.SeedAsync()` on first startup (seeds demo data + admin user)

#### 2d. Start the API

```bash
dotnet run
# API runs at: http://localhost:5000
# Swagger UI:  http://localhost:5000/swagger
```

---

### Step 3 — Verify Full Stack

1. Open http://localhost:3000
2. Login: `admin@gymflow.com` / `Admin@123`
3. Open http://localhost:5000/swagger to explore all API endpoints

---

## 🐳 Docker Setup

Run everything with one command:

```bash
cd gymflow
docker-compose up --build
```

| Service | URL |
|---|---|
| Frontend | http://localhost:3000 |
| API | http://localhost:5000 |
| Swagger | http://localhost:5000/swagger |
| SQL Server | localhost:1433 |

> **First run takes ~3–5 minutes** to pull images and build. SQL Server health check ensures the API waits before connecting.

### Stop everything
```bash
docker-compose down

# Remove volumes (deletes database data)
docker-compose down -v
```

---

## 📡 API Documentation

Full Swagger docs available at **http://localhost:5000/swagger** when running.

### Auth Endpoints
| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/login` | Login, returns JWT + refresh token |
| `POST` | `/api/auth/refresh` | Refresh access token |
| `POST` | `/api/auth/logout` | Revoke refresh token |
| `GET`  | `/api/auth/me` | Get current user profile |

### Members
| Method | Endpoint | Description |
|---|---|---|
| `GET`    | `/api/members` | List members (paginated, search, filter) |
| `GET`    | `/api/members/:id` | Get member detail |
| `POST`   | `/api/members` | Create member |
| `PUT`    | `/api/members/:id` | Update member |
| `DELETE` | `/api/members/:id` | Soft-delete member |
| `POST`   | `/api/members/:id/check-in` | Record check-in |
| `GET`    | `/api/members/expiring` | Members expiring in N days |

### Finance
| Method | Endpoint | Description |
|---|---|---|
| `GET`  | `/api/finance/transactions` | List transactions |
| `POST` | `/api/finance/transactions` | Create transaction |
| `GET`  | `/api/finance/summary` | Revenue summary |

### Dashboard
| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/dashboard/stats` | KPI stats |
| `GET` | `/api/dashboard/activity` | Recent activity |
| `GET` | `/api/dashboard/revenue-chart` | 6-month revenue data |
| `GET` | `/api/dashboard/summary` | Revenue by type |

### Other Modules
All follow the same REST pattern:
- `GET /api/inventory`
- `GET /api/products`
- `GET /api/promo-codes`
- `GET /api/staff`
- `GET /api/membership-plans`

### Authentication
All endpoints (except `/api/auth/login`) require:
```
Authorization: Bearer <your-jwt-token>
```

---

## 🗄 Database Schema

```
Gyms ──────────────────────────────────────────────┐
  │                                                  │
  ├── AdminUsers ──── RefreshTokens                 │
  │                                                  │
  ├── MembershipPlans                               │
  │       │                                          │
  ├── Members ────────────────────────────────────  │
  │       │                                          │
  │       ├── Transactions                          │
  │       └── MemberCheckIns                        │
  │                                                  │
  ├── InventoryItems                                │
  ├── Products                                      │
  ├── PromoCodes                                    │
  └── Staff                                         │
```

### Key Design Decisions
- **Soft deletes** — `IsDeleted` flag on all entities (data is never lost)
- **GymId on every table** — multi-tenant ready from day one
- **Enums stored as integers** — better query performance vs strings
- **Decimal(18,2)** on all money columns — no floating-point currency bugs
- **DateOnly for dates** — join dates, expiry dates use `DateOnly` not `DateTime`

---

## 🔐 Environment Variables

### Frontend (`frontend/.env`)
| Variable | Default | Description |
|---|---|---|
| `REACT_APP_API_URL` | `http://localhost:5000/api` | Backend API base URL |
| `REACT_APP_USE_MOCK` | `true` | `true` = mock data, `false` = real API |

### Backend (`appsettings.json`)
| Key | Description |
|---|---|
| `ConnectionStrings:DefaultConnection` | SQL Server connection string |
| `JwtSettings:SecretKey` | **CHANGE THIS** — min 32 chars, use a secrets manager in prod |
| `JwtSettings:ExpiryMinutes` | Access token lifetime (default: 60) |
| `JwtSettings:RefreshTokenExpiryDays` | Refresh token lifetime (default: 7) |
| `Cors:AllowedOrigins` | Array of allowed frontend origins |

> **⚠️ Production Security Checklist**
> - [ ] Change `JwtSettings:SecretKey` to a strong random secret (32+ chars)
> - [ ] Use Azure Key Vault / AWS Secrets Manager for secrets
> - [ ] Set `ASPNETCORE_ENVIRONMENT=Production`
> - [ ] Enable HTTPS
> - [ ] Restrict `Cors:AllowedOrigins` to your exact domain

---

## 🗺 Roadmap

### Phase 2 — Member Mobile App (React Native)
- [ ] Member self-service: view plan, payment history
- [ ] QR-code check-in
- [ ] Push notifications (expiry, offers)
- [ ] Workout tracking & progress photos
- [ ] Direct chat with trainer

### Phase 3 — Advanced Admin Features
- [ ] Multi-branch support (one account, multiple gyms)
- [ ] Automated payment reminders (WhatsApp/SMS via Twilio)
- [ ] Online payment integration (Razorpay / Stripe)
- [ ] Biometric/RFID attendance integration
- [ ] Personal training session booking calendar
- [ ] Automated membership renewal

### Phase 4 — SaaS Platform
- [ ] Self-serve gym onboarding (sign up, configure, go live)
- [ ] Subscription billing (Stripe/Razorpay subscriptions)
- [ ] White-label (gym's own branding/domain)
- [ ] Marketplace for supplements/equipment
- [ ] Analytics benchmark (compare vs. avg gym)

---

## 👨‍💻 Development Notes

### Adding a New Page
1. Create `src/pages/YourPage/index.tsx`
2. Add route in `src/App.tsx`
3. Add nav item in `src/components/layout/Sidebar.tsx`
4. Add API calls in `src/api/services.ts`
5. Add mock data in `src/api/mockData.ts`

### Adding a New API Endpoint
1. Add entity to `Models/Entities.cs`
2. Add `DbSet` to `Data/AppDbContext.cs`
3. Create migration: `dotnet ef migrations add YourMigration`
4. Add interface to `Services/IServices.cs`
5. Implement in `Services/Services.cs`
6. Add controller in `Controllers/`
7. Register service in `Program.cs`

### Mock Mode vs Real API
Toggle `REACT_APP_USE_MOCK` in `.env`. All API functions in `services.ts` check this flag and either return mock data or call the real backend. This lets you build and demo the frontend without running the backend.

---

## 📄 License

MIT — free to use, modify, and deploy for your own gym or as a SaaS product.

---

*Built with ❤️ for gym owners who deserve better software.*

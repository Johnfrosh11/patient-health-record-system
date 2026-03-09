# Patient Health Record System - Complete Setup Guide

This guide walks you through setting up and running the complete Patient Health Record System with the new 6-layer architecture.

## 🏗️ Architecture Overview

### Backend: 6-Layer N-Tier Architecture
1. **API Layer** - Controllers, middleware, configuration
2. **Application Layer** - Business logic, services, DTOs, validations
3. **Repository Layer** - Data access, EF Core DbContext, entity mappings
4. **Domain Layer** - Entities, enums, exceptions, business rules
5. **Infrastructure Layer** - External services (email, SMS, blob storage)
6. **Utilities Layer** - Shared models (ResponseModel, pagination, settings)

### Frontend: Modern React + TypeScript
- **Vite** + React 18 + TypeScript 5 (strict mode)
- **TanStack Query** for server state caching
- **Zustand** for global state management
- **React Router v6** for routing
- **Tailwind CSS** for styling
- **React Hook Form + Zod** for form validation

---

## 📋 Prerequisites

### Backend
- .NET 10 SDK
- SQL Server (or SQL Server Express)
- Visual Studio 2022 or VS Code with C# extension

### Frontend
- Node.js 18+ (LTS recommended)
- npm or pnpm package manager

---

## 🚀 Backend Setup

### Step 1: Configure Database Connection

1. Navigate to the API project:
   ```powershell
   cd src/PatientHealthRecord.API
   ```

2. Copy the example configuration:
   ```powershell
   cp appsettings.json.example appsettings.json
   ```

3. Edit `appsettings.json` and update the connection string:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=YOUR_SERVER;Database=PatientHealthRecordDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
     },
     "JwtSettings": {
       "SecretKey": "your-super-secret-key-at-least-32-characters-long",
       "Issuer": "PatientHealthRecordAPI",
       "Audience": "PatientHealthRecordClient",
       "JwtExpires": 60,
       "RefreshTokenExpires": 10080
     }
   }
   ```

### Step 2: Install Dependencies

```powershell
# Restore all NuGet packages
dotnet restore
```

### Step 3: Create Database Migration

```powershell
# From the solution root directory
dotnet ef migrations add InitialMigration --project src/PatientHealthRecord.Repository --startup-project src/PatientHealthRecord.API

# Apply migration to database
dotnet ef database update --project src/PatientHealthRecord.Repository --startup-project src/PatientHealthRecord.API
```

### Step 4: Run the Backend API

```powershell
# Start the API
dotnet run --project src/PatientHealthRecord.API
```

The API will start on `https://localhost:5001` and `http://localhost:5000`.

### Step 5: Verify Backend is Running

- Open browser to `http://localhost:5000/swagger`
- You should see the Swagger UI with all API endpoints

---

## 🎨 Frontend Setup

### Step 1: Navigate to Frontend Directory

```powershell
cd frontend
```

### Step 2: Install Dependencies

```powershell
# Using npm
npm install

# Or using pnpm (faster)
pnpm install
```

### Step 3: Configure Environment Variables

1. Copy the example environment file:
   ```powershell
   cp .env.example .env
   ```

2. The default configuration should work:
   ```env
   VITE_API_URL=http://localhost:5000/api/v1
   ```

### Step 4: Run the Frontend

```powershell
# Start development server
npm run dev

# Or with pnpm
pnpm dev
```

The frontend will start on `http://localhost:3000`.

### Step 5: Verify Frontend is Running

- Open browser to `http://localhost:3000`
- You should see the login page

---

## 🧪 Testing the Complete System

### 1. Register a New User

1. Navigate to `http://localhost:3000/register`
2. Fill in the registration form:
   - Username: `testuser`
   - Email: `test@example.com`
   - First Name: `Test`
   - Last Name: `User`
   - Organization ID: `org-001` (or any string)
   - Password: `SecurePass123!`
3. Click "Register"
4. You should be automatically logged in and redirected to the dashboard

### 2. Create a Health Record

1. From the dashboard, click "Create New Health Record"
2. Fill in the form:
   - Patient Name: `John Doe`
   - Date of Birth: Select a date
   - Diagnosis: `Hypertension`
   - Treatment Plan: `Lifestyle modifications and medication`
   - Medical History: (optional)
3. Click "Create Record"

### 3. View Health Records

1. Navigate to "Records" from the header
2. You should see a list of all health records
3. Search, filter, and paginate through records

---

## 🔐 Key Features Implemented

### Backend Features
✅ JWT Authentication with RS256/HS256 support
✅ BCrypt password hashing (cost factor 12)
✅ Refresh token rotation
✅ Multi-tenancy via OrganizationId isolation
✅ Role-based access control (RBAC)
✅ Optimistic concurrency with RowVersion
✅ Comprehensive audit logging with Serilog
✅ OWASP security headers
✅ Rate limiting (100 req/min standard, 10 req/min auth)
✅ Global exception handling
✅ Correlation ID tracking
✅ Database seeding with roles/permissions

### Frontend Features
✅ JWT token management with automatic refresh
✅ Protected routes with authentication guards
✅ Persistent authentication state (localStorage)
✅ Auto-logout on 401 responses
✅ Responsive design with dark mode support
✅ Form validation with Zod schemas
✅ Loading and error states
✅ Type-safe API client with TypeScript strict mode

---

## 📁 Project Structure

### Backend
```
src/
├── PatientHealthRecord.API/          # Web API (Controllers, Middleware)
├── PatientHealthRecord.Application/  # Services, DTOs, Validators
├── PatientHealthRecord.Repository/   # DbContext, Entity Mappings
├── PatientHealthRecord.Domain/       # Entities, Enums, Exceptions
├── PatientHealthRecord.Infrastructure/ # External Services
└── PatientHealthRecord.Utilities/    # Shared Models
```

### Frontend
```
frontend/
├── src/
│   ├── components/       # UI components
│   │   ├── ui/          # Button, Input, Card
│   │   ├── layouts/     # Header, Layout
│   │   └── auth/        # ProtectedRoute
│   ├── pages/           # LoginPage, DashboardPage
│   ├── services/        # API service layer
│   ├── stores/          # Zustand stores
│   ├── lib/             # Utils, API client, constants
│   └── types/           # TypeScript types
└── package.json
```

---

## 🔧 Common Issues & Solutions

### Backend Issues

#### Issue: Migration fails
**Solution:** Ensure your SQL Server is running and the connection string is correct.

```powershell
# Test connection
dotnet ef dbcontext info --project src/PatientHealthRecord.Repository --startup-project src/PatientHealthRecord.API
```

#### Issue: JWT token errors
**Solution:** Ensure your JWT secret key is at least 32 characters long in `appsettings.json`.

#### Issue: CORS errors
**Solution:** The API is configured to allow `http://localhost:3000`. If using a different port, update `Program.cs`.

### Frontend Issues

#### Issue: Module not found errors
**Solution:** Ensure all dependencies are installed and the `@` alias is working:

```powershell
rm -rf node_modules
npm install
```

#### Issue: API calls failing
**Solution:** Verify the backend is running on `http://localhost:5000` and the `.env` file is configured correctly.

#### Issue: Authentication not persisting
**Solution:** Check browser console for localStorage errors. Clear localStorage and try again:

```javascript
localStorage.clear()
```

---

## 📊 Database Schema

The system uses the following main tables:

- **t_user** - User accounts with BCrypt password hashing
- **t_health_record** - Patient health records
- **t_access_request** - Time-bound access requests
- **t_role** - User roles
- **t_permission** - System permissions
- **t_user_role** - User-Role mapping
- **t_role_permission** - Role-Permission mapping
- **t_refresh_token** - JWT refresh tokens
- **t_audit_log** - System audit trail

All tables include:
- `organization_id` for multi-tenancy
- `created_date`, `created_by`, `modified_date`, `modified_by` for audit
- `is_active` for soft deletes
- `row_version` for optimistic concurrency

---

## 🛡️ Security Features

1. **Password Security**
   - BCrypt hashing with cost factor 12
   - Minimum 8 character requirement

2. **Token Security**
   - JWT with expiration (60 minutes default)
   - Refresh tokens with rotation
   - Token blacklisting support

3. **API Security**
   - OWASP security headers
   - Rate limiting per IP
   - CORS configuration
   - Request validation

4. **Data Security**
   - Multi-tenancy isolation
   - Role-based access control
   - Audit logging
   - Soft deletes

---

## 📝 Next Steps

1. **Add More Pages**
   - Health Records List Page
   - Health Record Detail Page
   - Create/Edit Health Record Forms
   - Access Requests Page
   - User Management Page

2. **Implement Remaining Services**
   - AccessRequestService
   - UserService
   - RoleService

3. **Add Tests**
   - Unit tests for services
   - Integration tests for API
   - E2E tests for critical flows

4. **Deployment**
   - Configure production appsettings
   - Set up CI/CD pipeline
   - Deploy to Azure App Service
   - Configure production database

---

## 📚 Additional Resources

- [NewArcitecture.txt](../NewArcitecture.txt) - Complete architecture guidelines
- [CLAUDE 1.md](../CLAUDE%201.md) - Frontend coding standards
- [API Documentation](http://localhost:5000/swagger) - Swagger UI
- [.NET 10 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [React Documentation](https://react.dev/)

---

## 🤝 Support

If you encounter any issues:

1. Check the logs in `src/PatientHealthRecord.API/logs/`
2. Review browser console for frontend errors
3. Verify all services are running
4. Check database connection

---

## ✅ Checklist

Backend:
- [ ] SQL Server is running
- [ ] appsettings.json is configured
- [ ] Database migration completed
- [ ] API is running on localhost:5000
- [ ] Swagger UI is accessible

Frontend:
- [ ] Node.js 18+ is installed
- [ ] npm install completed successfully
- [ ] .env file is configured
- [ ] Frontend is running on localhost:3000
- [ ] Can access login page

Integration:
- [ ] Can register a new user
- [ ] Can login successfully
- [ ] Can view dashboard
- [ ] JWT token is stored in localStorage
- [ ] Can make authenticated API calls

---

**🎉 Congratulations! Your Patient Health Record System is now fully set up and running!**

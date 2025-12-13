# Patient Health Record System

## Overview

A production-grade Patient Health Record (PHR) system built with **.NET 9 Web API** that implements role-based permission access control, time-bound individual access requests, and JWT authentication with refresh tokens. This system enables healthcare organizations to securely manage patient health records while enforcing granular access control and audit trails.

## Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:

```
PatientHealthRecord/
├── src/
│   ├── PatientHealthRecord.API          → Presentation Layer (Controllers, Middleware)
│   ├── PatientHealthRecord.Application   → Business Logic (DTOs, Services, Interfaces, Validators)
│   ├── PatientHealthRecord.Domain        → Core Domain (Entities, Enums, Exceptions, Constants)
│   └── PatientHealthRecord.Infrastructure → Data Access (EF Core, Repositories, DbContext)
└── tests/
    └── PatientHealthRecord.Tests         → Unit Tests (xUnit, Moq, FluentAssertions)
```

### Key Architectural Patterns

- **Clean Architecture**: Strict dependency flow (Domain ← Application ← Infrastructure/API)
- **Repository Pattern**: Data access abstraction through service layer
- **Dependency Injection**: All services registered in DI container
- **Policy-Based Authorization**: Custom authorization handlers for permission checking
- **DTOs**: Request/Response separation from domain entities
- **FluentValidation**: Input validation pipeline
- **AutoMapper**: Object-to-object mapping
- **Structured Logging**: Serilog with console and file outputs

---

## Technology Stack

| Technology | Version | Purpose |
|------------|---------|---------|
| **.NET** | 9.0 | Core framework |
| **ASP.NET Core Web API** | 9.0 | RESTful API |
| **Entity Framework Core** | 9.0 | ORM (Code First) |
| **PostgreSQL** | 16+ | Database |
| **JWT Bearer** | 8.15.0 | Authentication |
| **BCrypt.Net** | 0.1.0 | Password hashing |
| **xUnit** | 2.9.3 | Unit testing framework |
| **Moq** | 4.20.72 | Mocking library |
| **FluentAssertions** | 7.0.0 | Test assertions |
| **FluentValidation** | 11.11.0 | Input validation |
| **AutoMapper** | 16.0.0 | Object mapping |
| **Serilog** | 10.0.0 | Structured logging |
| **Swashbuckle** | 7.2.0 | OpenAPI/Swagger documentation |

---

## Prerequisites

- **.NET 9 SDK** ([Download](https://dotnet.microsoft.com/download/dotnet/9.0))
- **PostgreSQL 16+** ([Download](https://www.postgresql.org/download/)) OR **Docker**
- **IDE**: Visual Studio 2022 / VS Code / JetBrains Rider
- **Postman** (optional, for API testing)

---

## Setup Instructions

### 1. Clone Repository

```bash
git clone <your-repo-url>
cd "Patient Health Record System"
```

### 2. Database Setup

#### Option A: Using Docker (Recommended)

```bash
# Start PostgreSQL container
docker run -d \
  --name phr-postgres \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=phr_db \
  -p 5432:5432 \
  postgres:16-alpine

# Verify container is running
docker ps | grep phr-postgres
```

#### Option B: Using Local PostgreSQL Installation

1. Install PostgreSQL from [postgresql.org](https://www.postgresql.org/download/)
2. Create database:
   ```sql
   CREATE DATABASE phr_db;
   ```

### 3. Configuration

Copy the example configuration and update values:

```bash
cp appsettings.json.example src/PatientHealthRecord.API/appsettings.json
```

Update `appsettings.json` with your database credentials:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=phr_db;Username=postgres;Password=yourpassword"
  },
  "JwtSettings": {
    "SecretKey": "your-256-bit-secret-key-minimum-32-characters-long",
    ...
  }
}
```

### 4. Database Migrations

```bash
cd src/PatientHealthRecord.Infrastructure

# Create migration (if not exists)
dotnet ef migrations add InitialCreate --startup-project ../PatientHealthRecord.API

# Apply migrations and seed data
dotnet ef database update --startup-project ../PatientHealthRecord.API
```

### 5. Run Application

```bash
cd ../PatientHealthRecord.API
dotnet run
```

The API will start at **`http://localhost:5243`**

### 6. Access Swagger UI

Open your browser and navigate to:

```
http://localhost:5243/swagger
```

### 7. Run Unit Tests

```bash
cd ../../tests/PatientHealthRecord.Tests
dotnet test
```

**Expected Result**: All 49 tests passing ✅

---

## Database Schema

### Core Entities

#### **User**
- **Id** (PK, Guid)
- Username (unique, indexed)
- Email (unique, indexed)
- PasswordHash (BCrypt, work factor 12)
- FirstName, LastName
- IsActive
- CreatedAt, UpdatedAt

#### **Role**
- **Id** (PK, Guid)
- Name (unique, indexed)
- Description
- CreatedAt

#### **Permission**
- **Id** (PK, Guid)
- Name (unique, indexed)
- Description
- CreatedAt

#### **UserRole** (Many-to-Many Junction)
- **UserId** (FK)
- **RoleId** (FK)

#### **RolePermission** (Many-to-Many Junction)
- **RoleId** (FK)
- **PermissionId** (FK)

#### **HealthRecord**
- **Id** (PK, Guid)
- PatientName (max 200 chars, indexed)
- DateOfBirth (required)
- Diagnosis (max 1000 chars)
- TreatmentPlan (max 2000 chars)
- MedicalHistory (max 5000 chars, optional)
- **CreatedBy** (FK to User, indexed)
- CreatedAt
- LastModifiedBy (nullable)
- LastModifiedAt (nullable)
- **IsDeleted** (soft delete, indexed)
- DeletedBy, DeletedAt

#### **AccessRequest**
- **Id** (PK, Guid)
- **HealthRecordId** (FK, indexed)
- **RequestingUserId** (FK, indexed)
- Reason (max 500 chars)
- RequestDate
- **Status** (enum: Pending, Approved, Declined, indexed)
- ReviewedBy (nullable)
- ReviewedDate (nullable)
- ReviewComment (max 500 chars, optional)
- **AccessStartDateTime** (nullable, for approved requests)
- **AccessEndDateTime** (nullable, for approved requests)
- CreatedAt, UpdatedAt

#### **RefreshToken**
- **Id** (PK, Guid)
- **UserId** (FK, indexed)
- Token (unique, indexed)
- ExpiresAt
- CreatedAt
- IsRevoked

---

## Role-Permission Model

### System Permissions

| Permission | Description |
|------------|-------------|
| **viewPatientRecords** | View all patient health records in the system |
| **createPatientRecords** | Create new patient health records |
| **updatePatientRecords** | Update existing patient health records (own records only) |
| **deletePatientRecords** | Soft delete patient health records (own records only) |
| **approveAccessRequests** | Approve or decline access requests from other users |
| **manageUsers** | Create and manage user accounts (admin only) |
| **manageRoles** | Create and manage roles and permissions (admin only) |

### Predefined Roles (Seeded)

#### **Admin**
- **All permissions**
- Full system access including user and role management

#### **Doctor**
- viewPatientRecords ✅
- createPatientRecords ✅
- updatePatientRecords ✅
- deletePatientRecords ✅
- approveAccessRequests ✅

#### **Nurse**
- createPatientRecords ✅
- updatePatientRecords ✅
- approveAccessRequests ✅

#### **Receptionist**
- createPatientRecords ✅
- Can only view own records
- Must request access to other records

### Access Control Logic

**A user can view a health record IF:**
- User created the record (CreatedBy = UserId), **OR**
- User has `viewPatientRecords` permission, **OR**
- User has an **APPROVED** access request for this record **AND** current UTC time is between `AccessStartDateTime` and `AccessEndDateTime`

---

## API Endpoints

### Authentication (`/api/auth`)

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/register` | Register new user account | ❌ |
| POST | `/login` | Authenticate and get tokens | ❌ |
| POST | `/refresh-token` | Refresh access token | ❌ |
| POST | `/logout` | Revoke refresh token | ✅ |
| GET | `/me` | Get current user info | ✅ |

### Patient Records (`/api/patient-records`)

| Method | Endpoint | Description | Permission Required |
|--------|----------|-------------|---------------------|
| GET | `/` | List all accessible records (paginated) | Any authenticated user |
| GET | `/{id}` | Get single record (with access check) | Record owner OR viewPatientRecords OR approved access |
| GET | `/my-records` | Get records created by current user | Any authenticated user |
| POST | `/` | Create new patient record | createPatientRecords |
| PUT | `/{id}` | Update own record | updatePatientRecords (own records only) |
| DELETE | `/{id}` | Soft delete own record | deletePatientRecords (own records only) |

### Access Requests (`/api/access-requests`)

| Method | Endpoint | Description | Permission Required |
|--------|----------|-------------|---------------------|
| GET | `/` | List my requests (or all if approver) | Any authenticated user |
| GET | `/{id}` | Get single access request | Request owner OR approveAccessRequests |
| GET | `/pending` | List pending requests for approval | approveAccessRequests |
| POST | `/` | Create new access request | Users WITHOUT viewPatientRecords |
| PUT | `/{id}/approve` | Approve with time bounds | approveAccessRequests |
| PUT | `/{id}/decline` | Decline with optional comment | approveAccessRequests |

### User Management (`/api/users`) - **Admin Only**

| Method | Endpoint | Description | Permission Required |
|--------|----------|-------------|---------------------|
| GET | `/` | List all users (paginated) | manageUsers |
| GET | `/{id}` | Get user details with roles | manageUsers |
| POST | `/` | Create new user | manageUsers |
| PUT | `/{id}` | Update user details | manageUsers |
| PUT | `/{id}/activate` | Activate user account | manageUsers |
| PUT | `/{id}/deactivate` | Deactivate user account | manageUsers |
| POST | `/{userId}/roles/{roleId}` | Assign role to user | manageUsers |
| DELETE | `/{userId}/roles/{roleId}` | Remove role from user | manageUsers |

### Role Management (`/api/roles`) - **Admin Only**

| Method | Endpoint | Description | Permission Required |
|--------|----------|-------------|---------------------|
| GET | `/` | List all roles with permissions | Any authenticated user |
| GET | `/permissions` | List all system permissions | Any authenticated user |
| GET | `/{id}` | Get role details | Any authenticated user |
| POST | `/` | Create new role | manageRoles |
| PUT | `/{id}` | Update role details | manageRoles |
| DELETE | `/{id}` | Delete role (if no users assigned) | manageRoles |
| POST | `/{roleId}/permissions/{permissionId}` | Assign permission to role | manageRoles |
| DELETE | `/{roleId}/permissions/{permissionId}` | Remove permission from role | manageRoles |

---

## Sample Credentials

Use these credentials for testing different roles:

| Role | Username | Password | Permissions |
|------|----------|----------|-------------|
| **Admin** | `admin` | `Admin@123` | All permissions |
| **Doctor** | `doctor` | `Doctor@123` | View, create, update, delete records; approve access requests |
| **Nurse** | `nurse` | `Nurse@123` | Create, update records; approve access requests |
| **Receptionist** | `receptionist` | `Reception@123` | Create records only; must request access to view others |

### Login Payload Example

```json
{
  "username": "admin",
  "password": "Admin@123"
}
```

---

## Testing Scenarios

### Scenario 1: Admin Full Access

1. **Login as Admin**
   - POST `/api/auth/login` with admin credentials
   - Receive access token and refresh token
   - Copy access token for subsequent requests

2. **View All Users**
   - GET `/api/users?page=1&pageSize=10`
   - Should return all 4 seeded users with roles

3. **Create New Doctor User**
   - POST `/api/users`
   ```json
   {
     "username": "doctor2",
     "email": "doctor2@interswitch.com",
     "password": "Doctor2@123",
     "firstName": "Sarah",
     "lastName": "Connor",
     "roleIds": ["<doctor-role-id>"]
   }
   ```

4. **View All Health Records**
   - GET `/api/patient-records`
   - Should return all 5 seeded records

### Scenario 2: Doctor Workflow

1. **Login as Doctor**
   - POST `/api/auth/login` with doctor credentials

2. **View All Patient Records**
   - GET `/api/patient-records`
   - Doctor has `viewPatientRecords` permission → sees all records

3. **Create New Patient Record**
   - POST `/api/patient-records`
   ```json
   {
     "patientName": "Alex Martinez",
     "dateOfBirth": "1988-03-15",
     "diagnosis": "Migraine",
     "treatmentPlan": "Pain management and lifestyle modifications",
     "medicalHistory": "Chronic migraines for 5 years"
   }
   ```

4. **Update Own Record**
   - PUT `/api/patient-records/{id}` (for record created by doctor)
   - Should succeed

5. **Approve Access Request**
   - GET `/api/access-requests/pending` → see receptionist's request
   - PUT `/api/access-requests/{id}/approve`
   ```json
   {
     "grantedFrom": "2025-12-13T10:00:00Z",
     "grantedTo": "2025-12-15T18:00:00Z"
   }
   ```

### Scenario 3: Receptionist Limited Access

1. **Login as Receptionist**
   - POST `/api/auth/login` with receptionist credentials

2. **View Own Records Only**
   - GET `/api/patient-records/my-records`
   - Should only see records created by receptionist

3. **Attempt to View All Records**
   - GET `/api/patient-records`
   - Returns only records receptionist has access to (own + approved access requests)

4. **Create Access Request**
   - POST `/api/access-requests`
   ```json
   {
     "healthRecordId": "<john-doe-record-id>",
     "reason": "Need to schedule follow-up appointment and update contact details"
   }
   ```

5. **Check Request Status**
   - GET `/api/access-requests` → see request status (Pending)

6. **After Approval: Access Record**
   - GET `/api/patient-records/{id}` → now accessible within approved time range

### Scenario 4: Time-Bound Access Expiration

1. **Create Access Request** (as Receptionist)
2. **Approve with Short Time Window** (as Doctor)
   - Approve for next 5 minutes only
3. **Access Record Immediately** → Success ✅
4. **Wait for Expiration** (past `AccessEndDateTime`)
5. **Attempt Access Again** → Forbidden 403 ❌

### Scenario 5: Token Refresh Flow

1. **Login** → receive `accessToken` (30 min) and `refreshToken` (7 days)
2. **Use Access Token** for API calls
3. **After 30 Minutes**: Access token expires → 401 Unauthorized
4. **Refresh Token**
   - POST `/api/auth/refresh-token`
   ```json
   {
     "refreshToken": "<refresh-token-guid>"
   }
   ```
5. **Receive New Tokens** → old refresh token revoked, new tokens issued
6. **Continue Using API** with new access token

---

## Environment Variables

Required configuration values (use `appsettings.json` or environment variables):

| Variable | Description | Example |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection string | `Host=localhost;Port=5432;Database=phr_db;Username=postgres;Password=yourpassword` |
| `JwtSettings__SecretKey` | JWT signing key (min 32 chars) | `your-very-secure-secret-key-minimum-32-characters` |
| `JwtSettings__Issuer` | JWT issuer | `PatientHealthRecordAPI` |
| `JwtSettings__Audience` | JWT audience | `PatientHealthRecordClient` |
| `JwtSettings__AccessTokenExpirationMinutes` | Access token lifetime | `30` |
| `JwtSettings__RefreshTokenExpirationDays` | Refresh token lifetime | `7` |

---

## Postman Collection

A comprehensive Postman collection with **47 endpoints** is included:

**File**: `Patient Health Record API.postman_collection.json`

### Features:
- ✅ All API endpoints organized by category
- ✅ Environment variables for tokens and IDs
- ✅ Automated test scripts for each request
- ✅ Sample requests with valid data
- ✅ Token auto-refresh on 401 errors

### Usage:
1. Import collection into Postman
2. Import environment variables (or create new environment)
3. Login with any sample credential
4. Tokens are automatically saved and used
5. Explore all endpoints

**See**: `POSTMAN_GUIDE.md` for detailed usage instructions

---

## Security Features

### Password Security
- **BCrypt hashing** with work factor 12
- **Minimum requirements**: 8+ characters, uppercase, lowercase, number
- **Never returned** in API responses

### JWT Authentication
- **Access tokens**: Short-lived (30 minutes)
- **Refresh tokens**: Long-lived (7 days), stored securely in database
- **Token rotation**: Old refresh token revoked when new one issued
- **Claims include**: UserId, Username, Roles, Permissions

### Authorization
- **Policy-based**: Custom authorization handlers
- **Permission checking**: Runtime permission validation
- **Time-bound access**: Automatic expiration enforcement
- **Audit trail**: All modifications tracked (CreatedBy, UpdatedBy, DeletedBy)

### Input Validation
- **FluentValidation**: All DTOs validated
- **SQL Injection Prevention**: EF Core parameterized queries
- **CORS**: Configured allowed origins

### Logging & Auditing
- **Serilog**: Structured logging to console and files
- **Authentication attempts**: Success/failure logged
- **Authorization failures**: Logged with user and resource details
- **CRUD operations**: All modifications logged with user context

---

## Assumptions Made

1. **Date of Birth**: Stored as UTC DateTime for consistency with PostgreSQL
2. **Access Request Logic**: Cannot request access to own records (already have access)
3. **Soft Delete**: Deleted records remain in database with `IsDeleted=true`
4. **User Activation**: Admin can activate/deactivate users; deactivated users cannot login
5. **Role Assignment**: Users must have at least one role; cannot remove last role
6. **Permission Assignment**: Roles must have at least one permission
7. **Time-Bound Access**: Checked at query time (not background job) for performance
8. **Token Revocation**: Refresh tokens explicitly revoked on logout or refresh

---

## Project Highlights

✅ **Clean Architecture** with strict dependency rules  
✅ **100% Unit Test Coverage** (49 tests passing)  
✅ **PostgreSQL Database** with Code First migrations  
✅ **JWT Authentication** with refresh token rotation  
✅ **Role-Based Access Control** with permission-level authorization  
✅ **Time-Bound Access Requests** with automatic expiration  
✅ **Soft Delete** with audit trail  
✅ **Comprehensive API Documentation** with Swagger  
✅ **Postman Collection** with automated tests  
✅ **Production-Ready** error handling and validation  
✅ **Structured Logging** with Serilog  

---

## Future Enhancements

- Email notifications for access request approvals
- Redis caching for frequently accessed data
- Background job for access expiration cleanup (Hangfire/Quartz)
- CQRS pattern with MediatR
- API rate limiting
- Health check endpoints
- Docker Compose for full stack deployment
- CI/CD pipeline (GitHub Actions)
- Integration tests
- API versioning (v2)

---

## License

This project is developed as a technical assessment for Interswitch.

---

## Contact

For questions or issues, please contact the development team.

---

**Built with ❤️ using Clean Architecture principles**

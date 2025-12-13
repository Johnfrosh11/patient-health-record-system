# Patient Health Record API - Postman Collection

## Overview
Comprehensive Postman collection for testing the Patient Health Record System API. This collection includes all API endpoints with automated testing scripts, environment variables, and example flows.

## Features
- ✅ **47 API Endpoints** across 5 categories
- ✅ **Automated Token Management** - Access tokens saved automatically
- ✅ **Test Scripts** - Automated validation for each response
- ✅ **Environment Variables** - Dynamic IDs stored for chaining requests
- ✅ **Complete Workflows** - Login → Create → Update → Delete flows
- ✅ **Pre-configured Test Data** - Matching seeded database users

## Quick Start

### 1. Import Collection
1. Open Postman
2. Click **Import** button
3. Select `Patient Health Record API.postman_collection.json`
4. Collection will appear in left sidebar

### 2. Configure Environment
Create a new environment in Postman with:
```
baseUrl = http://localhost:5243
```

Or use the collection variables (already configured):
- All necessary variables are pre-defined in the collection
- Tokens and IDs are automatically saved during test execution

### 3. Start API Server
```bash
cd "src/PatientHealthRecord.API"
dotnet run
```
API will start at: http://localhost:5243

### 4. Run Collection
**Option A: Run entire collection**
- Click on collection name → **Run**
- Select all folders → **Run Patient Health Record API**

**Option B: Run individual requests**
- Navigate to any request → **Send**
- Check **Tests** tab for automated validation results

## Collection Structure

### 1. Authentication (6 endpoints)
- **Register New User** - Create account (no auth required)
- **Login (Admin)** - Get admin tokens ✅ Saves tokens automatically
- **Login (Doctor)** - Get doctor tokens for testing
- **Get Current User** - View authenticated user details
- **Refresh Token** - Renew access token
- **Logout** - Revoke refresh token

### 2. Health Records (6 endpoints)
- **Get All Health Records** - Paginated list (Admin/Doctor)
- **Get Health Record by ID** - View single record
- **Get My Health Records** - Records created by current user
- **Create Health Record** - Add new patient record ✅ Saves recordId
- **Update Health Record** - Modify existing record
- **Delete Health Record** - Soft delete record

### 3. Access Requests (6 endpoints)
- **Get All Access Requests** - Paginated list
- **Get Access Request by ID** - View single request
- **Get Pending Access Requests** - Filter by status
- **Create Access Request** - Request time-bound access ✅ Saves requestId
- **Approve Access Request** - Grant access with dates
- **Decline Access Request** - Reject with reason

### 4. User Management (8 endpoints - Admin only)
- **Get All Users** - Paginated user list
- **Get User by ID** - View user with roles/permissions
- **Create User** - Add new user with roles ✅ Saves userId
- **Update User** - Modify user details
- **Activate User** - Enable account
- **Deactivate User** - Disable account (soft delete)
- **Assign Role to User** - Grant role
- **Remove Role from User** - Revoke role

### 5. Role Management (8 endpoints - Admin only)
- **Get All Roles** - List all roles with permissions ✅ Saves roleIds
- **Get All Permissions** - List all system permissions ✅ Saves permissionId
- **Get Role by ID** - View role with user count
- **Create Role** - Add new role with permissions ✅ Saves roleId
- **Update Role** - Modify role details
- **Assign Permission to Role** - Grant permission
- **Remove Permission from Role** - Revoke permission
- **Delete Role** - Remove role (if no users assigned)

## Environment Variables

### Automatically Saved
These variables are automatically populated by test scripts:
- `accessToken` - JWT access token (30 min expiry)
- `refreshToken` - Refresh token (7 day expiry)
- `userId` - Current user ID
- `recordId` - Last created health record ID
- `accessRequestId` - Last created access request ID
- `targetUserId` - Last created user ID (for admin operations)
- `targetRoleId` - Last created role ID (for admin operations)
- `doctorRoleId` - Doctor role ID (from Get All Roles)
- `nurseRoleId` - Nurse role ID (from Get All Roles)
- `permissionId` - Sample permission ID (from Get All Permissions)

### Manually Set
- `baseUrl` - API base URL (default: http://localhost:5243)

## Test Users

All test users have password: **Test@123**

| Username | Email | Roles | Permissions |
|----------|-------|-------|-------------|
| admin | admin@phr.com | Administrator | Full system access |
| doctor | doctor@phr.com | Doctor | View all records, Create/Update/Delete records |
| nurse | nurse@phr.com | Nurse | View own records, Request access |
| receptionist | receptionist@phr.com | Receptionist | Create records, Request access |

## Example Workflows

### Workflow 1: Complete Health Record Management
1. **Login (Admin)** - Get authenticated
2. **Create Health Record** - Add patient record
3. **Get Health Record by ID** - View created record
4. **Update Health Record** - Modify diagnosis/treatment
5. **Get My Health Records** - List all records you created
6. **Delete Health Record** - Soft delete record

### Workflow 2: Access Request Flow
1. **Login (Doctor)** - Login as admin/doctor
2. **Create Health Record** - Create a record
3. **Login (Nurse)** - Switch to nurse account
4. **Create Access Request** - Request access to record
5. **Login (Doctor)** - Switch back to doctor
6. **Get Pending Access Requests** - View pending requests
7. **Approve Access Request** - Grant time-bound access

### Workflow 3: User & Role Management (Admin)
1. **Login (Admin)** - Get admin tokens
2. **Get All Permissions** - View available permissions
3. **Create Role** - Create custom role with permissions
4. **Get All Roles** - Verify role created
5. **Create User** - Add new user with role
6. **Get All Users** - View user list
7. **Assign Role to User** - Add additional role
8. **Update User** - Modify user details
9. **Deactivate User** - Disable account

## Automated Tests

Each request includes automated test scripts that verify:
- ✅ Correct HTTP status codes
- ✅ Response structure and required fields
- ✅ Data types and formats
- ✅ Business rules and constraints
- ✅ Token and ID extraction

**Example test output:**
```
✓ Status code is 201 Created
✓ Record created with ID
✓ Response has required fields
✅ Health record created: 550e8400-e29b-41d4-a716-446655440000
```

## Authorization

### Global Authorization
Collection uses Bearer Token authentication with `{{accessToken}}` variable.

### Per-Request Authorization
Some requests override global auth:
- **Register** - No auth required
- **Login** - No auth required
- **Refresh Token** - No auth required

### Permission Requirements
- **Admin Only**: User Management, Role Management endpoints
- **Doctor/Admin**: View All Health Records
- **Any Authenticated**: Create Records, Access Requests

## Tips & Best Practices

### 1. Start with Authentication
Always run **Login (Admin)** first to populate tokens in environment variables.

### 2. Use Environment Variables
Variables like `{{recordId}}` are automatically saved. Reference them in subsequent requests.

### 3. Check Test Results
After each request, view the **Tests** tab to see validation results and console logs.

### 4. Handle Token Expiry
If you get 401 Unauthorized:
1. Check if access token expired (30 min)
2. Run **Refresh Token** request
3. Or run **Login** again

### 5. Clean Up Test Data
Run delete operations after testing to clean up:
- Delete created health records
- Delete created users/roles (if no dependencies)

### 6. Export Results
After running collection:
- Click **Runner** → View results
- Export as HTML/JSON for documentation

## Troubleshooting

### Issue: "accessToken is not defined"
**Solution**: Run **Login (Admin)** or **Login (Doctor)** first to populate tokens.

### Issue: "404 Not Found" for recordId/userId
**Solution**: Run the corresponding Create request first to generate and save the ID.

### Issue: "403 Forbidden"
**Solution**: Current user lacks required permission. Login as admin or user with appropriate role.

### Issue: "401 Unauthorized"
**Solution**: Access token expired. Run **Refresh Token** or **Login** again.

### Issue: Connection refused
**Solution**: Ensure API is running:
```bash
cd src/PatientHealthRecord.API
dotnet run
```

## Collection Variables Reference

```json
{
  "baseUrl": "http://localhost:5243",
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "550e8400-e29b-41d4-a716-446655440000",
  "userId": "550e8400-e29b-41d4-a716-446655440001",
  "recordId": "550e8400-e29b-41d4-a716-446655440002",
  "accessRequestId": "550e8400-e29b-41d4-a716-446655440003",
  "targetUserId": "550e8400-e29b-41d4-a716-446655440004",
  "targetRoleId": "550e8400-e29b-41d4-a716-446655440005",
  "doctorRoleId": "550e8400-e29b-41d4-a716-446655440006",
  "nurseRoleId": "550e8400-e29b-41d4-a716-446655440007",
  "permissionId": "550e8400-e29b-41d4-a716-446655440008"
}
```

## Support & Documentation

- **API Documentation**: http://localhost:5243/swagger
- **GitHub Issues**: Report bugs and feature requests
- **Contact**: For technical assessment queries

---

**Created for**: Interswitch Technical Assessment  
**Version**: 1.0.0  
**Last Updated**: December 13, 2024

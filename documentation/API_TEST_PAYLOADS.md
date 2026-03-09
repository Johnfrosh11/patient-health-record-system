# Patient Health Record API - Complete Test Payloads

**Base URL:** `http://localhost:5243`  
**Swagger UI:** `http://localhost:5243/index.html`

---

## Test Users

| Role | Username | Password | Permissions |
|------|----------|----------|-------------|
| Admin | admin | Admin@123 | All permissions (manage users, roles, view/create/update/delete records, approve access) |
| Doctor | doctor | Doctor@123 | View all records, create, update own, delete own, approve access requests |
| Nurse | nurse | Nurse@123 | View all records, create, update own, delete own, approve access requests |
| Receptionist | receptionist | Reception@123 | Create records only (needs access request for others' records) |

---

## 1. AUTHENTICATION

### 1.1 Login (All Users)

**POST** `/api/v1/Auth/login`

```json
// Admin
{
    "username": "admin",
    "password": "Admin@123"
}

// Doctor
{
    "username": "doctor",
    "password": "Doctor@123"
}

// Nurse
{
    "username": "nurse",
    "password": "Nurse@123"
}

// Receptionist
{
    "username": "receptionist",
    "password": "Reception@123"
}
```

**Expected Response (200 OK):**
```json
{
    "success": true,
    "message": "Login successful",
    "statusCode": 200,
    "data": {
        "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        "refreshToken": "abc123...",
        "expiresAt": "2025-12-14T20:00:00Z",
        "user": {
            "id": "guid",
            "username": "admin",
            "email": "admin@interswitch.com",
            "firstName": "System",
            "lastName": "Administrator",
            "roles": ["Admin"],
            "permissions": ["viewPatientRecords", "createPatientRecords", ...]
        }
    }
}
```

### 1.2 Login - Invalid Credentials

**POST** `/api/v1/Auth/login`

```json
{
    "username": "admin",
    "password": "WrongPassword"
}
```

**Expected Response (401 Unauthorized):**
```json
{
    "success": false,
    "message": "Invalid username or password",
    "statusCode": 401,
    "data": null
}
```

### 1.3 Refresh Token

**POST** `/api/v1/Auth/refresh-token`

```json
{
    "refreshToken": "your-refresh-token-from-login"
}
```

### 1.4 Logout

**POST** `/api/v1/Auth/logout`

```json
{
    "refreshToken": "your-refresh-token-from-login"
}
```

### 1.5 Get Current User Profile

**GET** `/api/v1/Auth/me`

Headers: `Authorization: Bearer {token}`

---

## 2. PATIENT RECORDS (CRUD)

### 2.1 Create Patient Record

**POST** `/api/v1/patient-records`

Headers: `Authorization: Bearer {token}`

```json
{
    "patientName": "John Doe",
    "dateOfBirth": "1985-06-15",
    "diagnosis": "Hypertension Stage 1",
    "treatmentPlan": "Lifestyle modifications, low sodium diet, Lisinopril 10mg daily",
    "medicalHistory": "Family history of heart disease. Non-smoker. Occasional alcohol use."
}
```

**Expected Response (201 Created):**
```json
{
    "success": true,
    "message": "Health record created successfully",
    "statusCode": 201,
    "data": {
        "id": "guid",
        "patientName": "John Doe",
        "dateOfBirth": "1985-06-15T00:00:00Z",
        "diagnosis": "Hypertension Stage 1",
        "treatmentPlan": "Lifestyle modifications, low sodium diet, Lisinopril 10mg daily",
        "medicalHistory": "Family history of heart disease. Non-smoker. Occasional alcohol use.",
        "createdBy": "guid",
        "createdByUsername": "doctor",
        "createdAt": "2025-12-13T20:30:00Z",
        "lastModifiedBy": null,
        "lastModifiedDate": null,
        "isDeleted": false
    }
}
```

### 2.2 Create - Validation Error (Empty Fields)

**POST** `/api/v1/patient-records`

```json
{
    "patientName": "",
    "dateOfBirth": "1985-06-15",
    "diagnosis": "",
    "treatmentPlan": "",
    "medicalHistory": ""
}
```

**Expected Response (400 Bad Request):**
```json
{
    "success": false,
    "message": "Validation failed",
    "statusCode": 400,
    "errors": [
        "Patient name is required",
        "Diagnosis is required",
        "Treatment plan is required"
    ]
}
```

### 2.3 Create - Future Date of Birth

**POST** `/api/v1/patient-records`

```json
{
    "patientName": "Future Patient",
    "dateOfBirth": "2030-01-01",
    "diagnosis": "Test",
    "treatmentPlan": "Test",
    "medicalHistory": "Test"
}
```

**Expected Response (400 Bad Request):**
```json
{
    "success": false,
    "message": "Validation failed",
    "statusCode": 400,
    "errors": ["Date of birth cannot be in the future"]
}
```

### 2.4 Get All Records (Paginated)

**GET** `/api/v1/patient-records?page=1&pageSize=10`

Headers: `Authorization: Bearer {token}`

**Expected Response (200 OK):**
```json
{
    "success": true,
    "message": "Success",
    "statusCode": 200,
    "data": {
        "items": [...],
        "page": 1,
        "pageSize": 10,
        "totalCount": 5,
        "totalPages": 1,
        "hasPreviousPage": false,
        "hasNextPage": false
    }
}
```

### 2.5 Get My Records Only

**GET** `/api/v1/patient-records/my-records`

Headers: `Authorization: Bearer {token}`

### 2.6 Get Record by ID

**GET** `/api/v1/patient-records/{id}`

Headers: `Authorization: Bearer {token}`

Example: `GET /api/v1/patient-records/e8e454a1-f10a-4e4a-9af9-d2e14cb814f9`

### 2.7 Get Record - Not Found

**GET** `/api/v1/patient-records/00000000-0000-0000-0000-000000000000`

**Expected Response (404 Not Found):**
```json
{
    "success": false,
    "message": "Health record not found",
    "statusCode": 404,
    "data": null
}
```

### 2.8 Update Patient Record (Own Record)

**PUT** `/api/v1/patient-records/{id}`

Headers: `Authorization: Bearer {token}`

```json
{
    "patientName": "John Doe Updated",
    "dateOfBirth": "1985-06-15",
    "diagnosis": "Hypertension Stage 2 - Controlled",
    "treatmentPlan": "Continue Lisinopril 10mg, add Hydrochlorothiazide 12.5mg",
    "medicalHistory": "Family history of heart disease. Non-smoker. Blood pressure now controlled."
}
```

**Expected Response (200 OK):**
```json
{
    "success": true,
    "message": "Health record updated successfully",
    "statusCode": 200,
    "data": {
        "id": "guid",
        "patientName": "John Doe Updated",
        "lastModifiedBy": "guid",
        "lastModifiedDate": "2025-12-13T21:00:00Z",
        ...
    }
}
```

### 2.9 Update - Not Owner (Should Fail)

Try to update a record created by another user without ViewPatientRecords permission.

**Expected Response (403 Forbidden):**
```json
{
    "success": false,
    "message": "You can only update records you created",
    "statusCode": 403,
    "data": null
}
```

### 2.10 Delete Patient Record (Soft Delete)

**DELETE** `/api/v1/patient-records/{id}`

Headers: `Authorization: Bearer {token}`

**Expected Response (200 OK):**
```json
{
    "success": true,
    "message": "Success",
    "statusCode": 200,
    "data": {
        "id": "guid",
        "patientName": "John Doe",
        "deletedAt": "2025-12-13T21:30:00Z",
        "deletedBy": "doctor",
        "message": "Patient record for 'John Doe' has been archived successfully. The record is retained for compliance purposes but is no longer accessible in normal queries."
    }
}
```

### 2.11 Delete - Not Owner (Should Fail)

**Expected Response (403 Forbidden):**
```json
{
    "success": false,
    "message": "You can only delete records you created",
    "statusCode": 403,
    "data": null
}
```

---

## 3. ACCESS REQUESTS

### 3.1 Create Access Request (Receptionist)

Login as receptionist first, then request access to a record they don't own.

**POST** `/api/v1/access-requests`

Headers: `Authorization: Bearer {receptionist-token}`

```json
{
    "healthRecordId": "e8e454a1-f10a-4e4a-9af9-d2e14cb814f9",
    "reason": "Patient requested appointment scheduling and needs record verification"
}
```

**Expected Response (201 Created):**
```json
{
    "success": true,
    "message": "Access request created successfully",
    "statusCode": 201,
    "data": {
        "id": "guid",
        "healthRecordId": "e8e454a1-f10a-4e4a-9af9-d2e14cb814f9",
        "patientName": "Jane Smith",
        "requestingUserId": "guid",
        "requestingUsername": "receptionist",
        "reason": "Patient requested appointment scheduling and needs record verification",
        "status": "Pending",
        "requestDate": "2025-12-13T21:00:00Z",
        "reviewedBy": null,
        "reviewedByUsername": null,
        "reviewedDate": null,
        "reviewComment": null,
        "accessStartDateTime": null,
        "accessEndDateTime": null
    }
}
```

### 3.2 Create Access Request - Invalid Reason

**POST** `/api/v1/access-requests`

```json
{
    "healthRecordId": "e8e454a1-f10a-4e4a-9af9-d2e14cb814f9",
    "reason": "abc"
}
```

**Expected Response (400 Bad Request):**
```json
{
    "success": false,
    "message": "Validation failed",
    "statusCode": 400,
    "errors": ["Reason must be at least 10 characters"]
}
```

### 3.3 Get All Access Requests (Admin/Doctor/Nurse)

**GET** `/api/v1/access-requests`

Headers: `Authorization: Bearer {doctor-token}`

### 3.4 Get My Access Requests

**GET** `/api/v1/access-requests/my-requests`

Headers: `Authorization: Bearer {receptionist-token}`

### 3.5 Get Pending Access Requests

**GET** `/api/v1/access-requests/pending`

Headers: `Authorization: Bearer {doctor-token}`

### 3.6 Approve Access Request (Doctor/Nurse/Admin)

**PUT** `/api/v1/access-requests/{id}/review`

Headers: `Authorization: Bearer {doctor-token}`

```json
{
    "status": "Approved",
    "reviewComment": "Approved for patient scheduling purposes",
    "accessStartDateTime": "2025-12-13T00:00:00Z",
    "accessEndDateTime": "2025-12-20T23:59:59Z"
}
```

**Expected Response (200 OK):**
```json
{
    "success": true,
    "message": "Access request approved successfully",
    "statusCode": 200,
    "data": {
        "id": "guid",
        "status": "Approved",
        "reviewedBy": "guid",
        "reviewedByUsername": "doctor",
        "reviewedDate": "2025-12-13T21:15:00Z",
        "reviewComment": "Approved for patient scheduling purposes",
        "accessStartDateTime": "2025-12-13T00:00:00Z",
        "accessEndDateTime": "2025-12-20T23:59:59Z"
    }
}
```

### 3.7 Decline Access Request

**PUT** `/api/v1/access-requests/{id}/review`

Headers: `Authorization: Bearer {doctor-token}`

```json
{
    "status": "Declined",
    "reviewComment": "Insufficient justification for access"
}
```

### 3.8 Approve - Missing Time Window

**PUT** `/api/v1/access-requests/{id}/review`

```json
{
    "status": "Approved",
    "reviewComment": "Approved"
}
```

**Expected Response (400 Bad Request):**
```json
{
    "success": false,
    "message": "Access start and end date/time are required when approving",
    "statusCode": 400,
    "data": null
}
```

---

## 4. USER MANAGEMENT (Admin Only)

### 4.1 Get All Users

**GET** `/api/v1/users`

Headers: `Authorization: Bearer {admin-token}`

### 4.2 Get User by ID

**GET** `/api/v1/users/{id}`

Headers: `Authorization: Bearer {admin-token}`

### 4.3 Create New User

**POST** `/api/v1/users`

Headers: `Authorization: Bearer {admin-token}`

```json
{
    "username": "newdoctor",
    "email": "newdoctor@interswitch.com",
    "password": "NewDoctor@123",
    "firstName": "New",
    "lastName": "Doctor",
    "roleIds": ["role-id-for-doctor"]
}
```

**Note:** Get the role ID from `GET /api/v1/roles` first.

### 4.4 Create User - Duplicate Username

**POST** `/api/v1/users`

```json
{
    "username": "admin",
    "email": "another@interswitch.com",
    "password": "Test@123",
    "firstName": "Test",
    "lastName": "User",
    "roleIds": []
}
```

**Expected Response (409 Conflict):**
```json
{
    "success": false,
    "message": "Username already exists",
    "statusCode": 409,
    "data": null
}
```

### 4.5 Create User - Weak Password

**POST** `/api/v1/users`

```json
{
    "username": "weakuser",
    "email": "weak@interswitch.com",
    "password": "123",
    "firstName": "Weak",
    "lastName": "User",
    "roleIds": []
}
```

**Expected Response (400 Bad Request):**
```json
{
    "success": false,
    "message": "Validation failed",
    "statusCode": 400,
    "errors": [
        "Password must be at least 8 characters",
        "Password must contain at least one uppercase letter",
        "Password must contain at least one lowercase letter",
        "Password must contain at least one special character"
    ]
}
```

### 4.6 Update User

**PUT** `/api/v1/users/{id}`

Headers: `Authorization: Bearer {admin-token}`

```json
{
    "email": "updated@interswitch.com",
    "firstName": "Updated",
    "lastName": "Name",
    "isActive": true,
    "roleIds": ["role-id"]
}
```

### 4.7 Deactivate User

**PUT** `/api/v1/users/{id}`

```json
{
    "email": "user@interswitch.com",
    "firstName": "Test",
    "lastName": "User",
    "isActive": false,
    "roleIds": []
}
```

### 4.8 Delete User

**DELETE** `/api/v1/users/{id}`

Headers: `Authorization: Bearer {admin-token}`

---

## 5. ROLE MANAGEMENT (Admin Only)

### 5.1 Get All Roles

**GET** `/api/v1/roles`

Headers: `Authorization: Bearer {admin-token}`

### 5.2 Get Role by ID

**GET** `/api/v1/roles/{id}`

Headers: `Authorization: Bearer {admin-token}`

### 5.3 Create New Role

**POST** `/api/v1/roles`

Headers: `Authorization: Bearer {admin-token}`

```json
{
    "name": "Pharmacist",
    "description": "Hospital pharmacist with limited record access",
    "permissionIds": ["permission-id-for-view"]
}
```

**Note:** Get permission IDs from `GET /api/v1/roles` (permissions are included).

### 5.4 Create Role - Duplicate Name

**POST** `/api/v1/roles`

```json
{
    "name": "Admin",
    "description": "Another admin role",
    "permissionIds": []
}
```

**Expected Response (409 Conflict):**
```json
{
    "success": false,
    "message": "Role with this name already exists",
    "statusCode": 409,
    "data": null
}
```

### 5.5 Update Role

**PUT** `/api/v1/roles/{id}`

Headers: `Authorization: Bearer {admin-token}`

```json
{
    "name": "Pharmacist",
    "description": "Updated description for pharmacist role",
    "permissionIds": ["permission-id-1", "permission-id-2"]
}
```

### 5.6 Delete Role

**DELETE** `/api/v1/roles/{id}`

Headers: `Authorization: Bearer {admin-token}`

---

## 6. AUTHORIZATION TESTS

### 6.1 Access Without Token

**GET** `/api/v1/patient-records`

(No Authorization header)

**Expected Response (401 Unauthorized)**

### 6.2 Access With Expired Token

Use an expired JWT token.

**Expected Response (401 Unauthorized)**

### 6.3 Receptionist Trying to View All Records

Login as receptionist, try to access a record they don't own and don't have approved access for.

**GET** `/api/v1/patient-records/{doctor-created-record-id}`

Headers: `Authorization: Bearer {receptionist-token}`

**Expected Response (403 Forbidden):**
```json
{
    "success": false,
    "message": "You don't have permission to view this record",
    "statusCode": 403,
    "data": null
}
```

### 6.4 Receptionist Trying to Access User Management

**GET** `/api/v1/users`

Headers: `Authorization: Bearer {receptionist-token}`

**Expected Response (403 Forbidden)**

### 6.5 Doctor Trying to Access Role Management

**POST** `/api/v1/roles`

Headers: `Authorization: Bearer {doctor-token}`

**Expected Response (403 Forbidden)**

---

## 7. COMPLETE WORKFLOW TEST

### Scenario: New Patient Registration to Access Request Approval

1. **Login as Receptionist**
   ```
   POST /api/v1/Auth/login
   {"username": "receptionist", "password": "Reception@123"}
   ```

2. **Create Patient Record**
   ```
   POST /api/v1/patient-records
   {
       "patientName": "Alice Johnson",
       "dateOfBirth": "1992-04-10",
       "diagnosis": "Initial consultation - General checkup",
       "treatmentPlan": "Schedule follow-up with doctor",
       "medicalHistory": "No significant medical history"
   }
   ```

3. **Login as Doctor**
   ```
   POST /api/v1/Auth/login
   {"username": "doctor", "password": "Doctor@123"}
   ```

4. **View All Records (Doctor sees all)**
   ```
   GET /api/v1/patient-records
   ```

5. **Update Patient Record with Diagnosis**
   ```
   PUT /api/v1/patient-records/{alice-record-id}
   {
       "patientName": "Alice Johnson",
       "dateOfBirth": "1992-04-10",
       "diagnosis": "Mild iron deficiency anemia",
       "treatmentPlan": "Ferrous sulfate 325mg daily, recheck in 3 months",
       "medicalHistory": "No significant medical history. Lab work shows low hemoglobin."
   }
   ```

6. **Login as Receptionist Again**
   ```
   POST /api/v1/Auth/login
   {"username": "receptionist", "password": "Reception@123"}
   ```

7. **Request Access to Another Patient's Record**
   ```
   POST /api/v1/access-requests
   {
       "healthRecordId": "{another-doctor-record-id}",
       "reason": "Patient calling to schedule follow-up appointment, need to verify last visit details"
   }
   ```

8. **Login as Doctor**
   ```
   POST /api/v1/Auth/login
   {"username": "doctor", "password": "Doctor@123"}
   ```

9. **View Pending Access Requests**
   ```
   GET /api/v1/access-requests/pending
   ```

10. **Approve Access Request**
    ```
    PUT /api/v1/access-requests/{request-id}/review
    {
        "status": "Approved",
        "reviewComment": "Approved for scheduling purposes",
        "accessStartDateTime": "2025-12-13T00:00:00Z",
        "accessEndDateTime": "2025-12-15T23:59:59Z"
    }
    ```

11. **Login as Receptionist**
    ```
    POST /api/v1/Auth/login
    {"username": "receptionist", "password": "Reception@123"}
    ```

12. **Now Receptionist Can View the Record**
    ```
    GET /api/v1/patient-records/{approved-record-id}
    ```

---

## 8. EDGE CASES

### 8.1 Very Long Patient Name

**POST** `/api/v1/patient-records`

```json
{
    "patientName": "A very long patient name that exceeds reasonable limits and should be validated by the system to ensure data integrity and prevent potential issues with display or storage",
    "dateOfBirth": "1990-01-01",
    "diagnosis": "Test",
    "treatmentPlan": "Test",
    "medicalHistory": "Test"
}
```

### 8.2 Special Characters in Fields

**POST** `/api/v1/patient-records`

```json
{
    "patientName": "José García-López",
    "dateOfBirth": "1988-12-25",
    "diagnosis": "Diagnóstico con caracteres especiales: ñ, ü, é",
    "treatmentPlan": "Treatment with symbols: ®, ™, ©",
    "medicalHistory": "History with quotes: \"previous condition\", 'notes'"
}
```

### 8.3 Access Request After Time Window Expires

After an access request's `accessEndDateTime` has passed, the receptionist should no longer see that record in their list.

### 8.4 Self Access Request (Should Fail)

Receptionist tries to request access to their own record.

**POST** `/api/v1/access-requests`

```json
{
    "healthRecordId": "{receptionist-own-record-id}",
    "reason": "Testing self access"
}
```

---

## Quick Reference - HTTP Status Codes

| Code | Meaning |
|------|---------|
| 200 | OK - Request successful |
| 201 | Created - Resource created successfully |
| 204 | No Content - Successful with no response body |
| 400 | Bad Request - Validation error |
| 401 | Unauthorized - Invalid or missing token |
| 403 | Forbidden - Insufficient permissions |
| 404 | Not Found - Resource doesn't exist |
| 409 | Conflict - Duplicate resource |
| 500 | Internal Server Error - Server-side error |

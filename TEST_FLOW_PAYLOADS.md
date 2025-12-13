# Complete Test Flow - Fresh Data

**Base URL:** `http://localhost:5243`

---

## STEP 1: Login as Admin

**POST** `/api/v1/Auth/login`
```json
{
    "username": "admin",
    "password": "Admin@123"
}
```
> Save the `accessToken` for subsequent requests

---

## STEP 2: Get All Roles (to get role IDs)

**GET** `/api/v1/roles`

> Note the role IDs for Doctor, Nurse, Receptionist

---

## STEP 3: Create a New Doctor User

**POST** `/api/v1/users`
```json
{
    "username": "testdoctor",
    "email": "testdoctor@interswitch.com",
    "password": "TestDoc@123",
    "firstName": "Test",
    "lastName": "Doctor",
    "roleIds": ["{doctor-role-id}"]
}
```

---

## STEP 4: Create a New Receptionist User

**POST** `/api/v1/users`
```json
{
    "username": "testreceptionist",
    "email": "testreceptionist@interswitch.com",
    "password": "TestRec@123",
    "firstName": "Test",
    "lastName": "Receptionist",
    "roleIds": ["{receptionist-role-id}"]
}
```

---

## STEP 5: Login as Test Doctor

**POST** `/api/v1/Auth/login`
```json
{
    "username": "testdoctor",
    "password": "TestDoc@123"
}
```

---

## STEP 6: Doctor Creates Patient Record 1

**POST** `/api/v1/patient-records`
```json
{
    "patientName": "Patient Alpha",
    "dateOfBirth": "1985-03-15",
    "diagnosis": "Type 2 Diabetes",
    "treatmentPlan": "Metformin 500mg twice daily, diet control",
    "medicalHistory": "Family history of diabetes"
}
```
> Save the record `id`

---

## STEP 7: Doctor Creates Patient Record 2

**POST** `/api/v1/patient-records`
```json
{
    "patientName": "Patient Beta",
    "dateOfBirth": "1990-07-22",
    "diagnosis": "Hypertension Stage 1",
    "treatmentPlan": "Lisinopril 10mg daily, reduce sodium intake",
    "medicalHistory": "No prior conditions"
}
```

---

## STEP 8: Doctor Views All Records

**GET** `/api/v1/patient-records?page=1&pageSize=10`

> Doctor should see all records

---

## STEP 9: Doctor Updates Their Record

**PUT** `/api/v1/patient-records/{record-id-from-step-6}`
```json
{
    "patientName": "Patient Alpha",
    "dateOfBirth": "1985-03-15",
    "diagnosis": "Type 2 Diabetes - Well Controlled",
    "treatmentPlan": "Continue Metformin 500mg, add exercise regimen",
    "medicalHistory": "Family history of diabetes. HbA1c improved."
}
```

---

## STEP 10: Login as Test Receptionist

**POST** `/api/v1/Auth/login`
```json
{
    "username": "testreceptionist",
    "password": "TestRec@123"
}
```

---

## STEP 11: Receptionist Creates Patient Record

**POST** `/api/v1/patient-records`
```json
{
    "patientName": "Patient Gamma",
    "dateOfBirth": "1995-11-08",
    "diagnosis": "Initial Registration",
    "treatmentPlan": "Pending doctor consultation",
    "medicalHistory": "New patient, no records available"
}
```
> Save this record `id`

---

## STEP 12: Receptionist Views All Records

**GET** `/api/v1/patient-records?page=1&pageSize=10`

> Should only see Patient Gamma (their own record)

---

## STEP 13: Receptionist Requests Access to Doctor's Record

**POST** `/api/v1/access-requests`
```json
{
    "healthRecordId": "{record-id-from-step-6}",
    "reason": "Patient called to reschedule appointment, need to verify last visit date"
}
```
> Save the access request `id`

---

## STEP 14: Receptionist Views Their Access Requests

**GET** `/api/v1/access-requests/my-requests`

> Should see pending request

---

## STEP 15: Login as Test Doctor Again

**POST** `/api/v1/Auth/login`
```json
{
    "username": "testdoctor",
    "password": "TestDoc@123"
}
```

---

## STEP 16: Doctor Views Pending Access Requests

**GET** `/api/v1/access-requests/pending`

> Should see receptionist's request

---

## STEP 17: Doctor Approves Access Request

**PUT** `/api/v1/access-requests/{access-request-id}/review`
```json
{
    "status": "Approved",
    "reviewComment": "Approved for scheduling purposes",
    "accessStartDateTime": "2025-12-13T00:00:00Z",
    "accessEndDateTime": "2025-12-20T23:59:59Z"
}
```

---

## STEP 18: Login as Test Receptionist Again

**POST** `/api/v1/Auth/login`
```json
{
    "username": "testreceptionist",
    "password": "TestRec@123"
}
```

---

## STEP 19: Receptionist Views All Records Again

**GET** `/api/v1/patient-records?page=1&pageSize=10`

> Should now see 2 records: Patient Gamma (own) + Patient Alpha (approved access)

---

## STEP 20: Receptionist Views Specific Approved Record

**GET** `/api/v1/patient-records/{record-id-from-step-6}`

> Should succeed now

---

## STEP 21: Receptionist Tries to Update Doctor's Record (Should Fail)

**PUT** `/api/v1/patient-records/{record-id-from-step-6}`
```json
{
    "patientName": "Patient Alpha Modified",
    "dateOfBirth": "1985-03-15",
    "diagnosis": "Trying to modify",
    "treatmentPlan": "Should not work",
    "medicalHistory": "Test"
}
```
> Should get 403 Forbidden

---

## STEP 22: Receptionist Deletes Their Own Record

**DELETE** `/api/v1/patient-records/{record-id-from-step-11}`

> Should succeed with soft delete response

---

## STEP 23: Verify Deleted Record Not in List

**GET** `/api/v1/patient-records?page=1&pageSize=10`

> Patient Gamma should no longer appear

---

## STEP 24: Login as Admin

**POST** `/api/v1/Auth/login`
```json
{
    "username": "admin",
    "password": "Admin@123"
}
```

---

## STEP 25: Admin Creates New Role

**POST** `/api/v1/roles`

First get permissions:
**GET** `/api/v1/roles` (to see permission IDs)

Then create:
```json
{
    "name": "Pharmacist",
    "description": "Hospital pharmacist with view-only access",
    "permissionIds": ["{viewPatientRecords-permission-id}"]
}
```

---

## STEP 26: Admin Deactivates Test Receptionist

First get user ID:
**GET** `/api/v1/users`

Then update:
**PUT** `/api/v1/users/{test-receptionist-id}`
```json
{
    "email": "testreceptionist@interswitch.com",
    "firstName": "Test",
    "lastName": "Receptionist",
    "isActive": false,
    "roleIds": ["{receptionist-role-id}"]
}
```

---

## STEP 27: Deactivated User Tries to Login (Should Fail)

**POST** `/api/v1/Auth/login`
```json
{
    "username": "testreceptionist",
    "password": "TestRec@123"
}
```
> Should get 401 or error about inactive account

---

## STEP 28: Test Refresh Token

Using the doctor's refresh token from Step 15:

**POST** `/api/v1/Auth/refresh-token`
```json
{
    "refreshToken": "{refresh-token-from-login}"
}
```

---

## STEP 29: Test Logout

**POST** `/api/v1/Auth/logout`
```json
{
    "refreshToken": "{refresh-token}"
}
```

---

## STEP 30: Verify Logged Out Token Fails

Try using the old access token:

**GET** `/api/v1/patient-records`

> Should still work (JWT is stateless), but refresh should fail

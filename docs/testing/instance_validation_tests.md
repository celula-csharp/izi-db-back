# INSTANCE_VALIDATION_TESTS – Business Rule & Authorization

This document describes the **test plan** for validating the instance assignment logic and the business rule:

> **A student can only have 1 instance assigned.**

The tests focus mainly on:

- Authorization by role (`Admin` vs `Student`).
- Existence and status of `User` and `DatabaseInstance`.
- Enforcement of the **“1 student = 1 instance”** rule.
- Correct HTTP status codes and error messages.

---

## 1. Scope

Endpoints under test:

- `POST /api/admin/assign-instance`
- `GET /api/student/my-instance`

These tests can be executed manually with **Postman**, **Thunder Client**, or automated with integration tests.

---

## 2. Pre-Conditions

Before running the tests, make sure:

1. The API is running (for example at `https://localhost:5001` or similar).
2. The database SystemDB is migrated and contains at least:
    - One **Admin** user.
    - One or more **Student** users.
    - One or more **active** `DatabaseInstance` rows.
3. You have valid JWT tokens for:
    - An `Admin` user.
    - A `Student` user.

You can obtain tokens using:

- `POST /api/auth/login`

---

## 3. Test Cases – `POST /api/admin/assign-instance`

### TC-01 – Successful assignment for a student without instance

- **Description:** Admin assigns an active `DatabaseInstance` to a student that has no previous assignment.
- **Preconditions:**
    - Student `S1` exists with role `Student`.
    - `S1` has no row in `UserInstances`.
    - `DatabaseInstance` `D1` exists and `IsActive = true`.
- **Request:**

  ```http
  POST /api/admin/assign-instance
  Authorization: Bearer <admin_token>
  Content-Type: application/json

  {
    "userId": <S1_Id>,
    "databaseInstanceId": <D1_Id>
  }

    Expected result:

        HTTP 200 OK (or 201 Created, depending on implementation).

        Body contains assignment info (e.g., userId, databaseInstanceId, assignedAt).

        A new row is created in UserInstances with UserId = S1_Id.

TC-02 – Student already has an assigned instance

    Description: Try to assign another instance to a student who already has one.

    Preconditions:

        Student S2 exists with role Student.

        UserInstances contains a row for S2 (any DatabaseInstanceId).

    Request:

POST /api/admin/assign-instance
Authorization: Bearer <admin_token>
Content-Type: application/json

{
"userId": <S2_Id>,
"databaseInstanceId": <some_other_instance_Id>
}

Expected result:

    HTTP 409 Conflict (recommended).

    Body:

        {
          "error": "The student already has an assigned instance."
        }

    Business rule:
    Confirms enforcement of “1 student = 1 instance”.

TC-03 – User does not exist

    Description: Admin tries to assign an instance to a non-existing User.

    Request:

POST /api/admin/assign-instance
Authorization: Bearer <admin_token>
Content-Type: application/json

{
"userId": 999999,
"databaseInstanceId": <D1_Id>
}

Expected result:

    HTTP 404 Not Found

    Body:

        {
          "error": "User not found."
        }

TC-04 – User is not a Student

    Description: Admin tries to assign an instance to another Admin or non-student role.

    Preconditions:

        User U1 exists with role Admin.

    Request:

POST /api/admin/assign-instance
Authorization: Bearer <admin_token>
Content-Type: application/json

{
"userId": <U1_Id>,
"databaseInstanceId": <D1_Id>
}

Expected result:

    HTTP 400 Bad Request (or 409 Conflict, by design).

    Body:

        {
          "error": "Target user must have role 'Student'."
        }

TC-05 – DatabaseInstance does not exist

    Description: Admin uses an invalid databaseInstanceId.

    Request:

POST /api/admin/assign-instance
Authorization: Bearer <admin_token>
Content-Type: application/json

{
"userId": <S1_Id>,
"databaseInstanceId": 999999
}

Expected result:

    HTTP 404 Not Found

    Body:

        {
          "error": "Database instance not found."
        }

TC-06 – DatabaseInstance inactive

    Description: Admin tries to assign an instance where IsActive = false.

    Preconditions:

        DatabaseInstance D2 exists with IsActive = false.

    Request:

POST /api/admin/assign-instance
Authorization: Bearer <admin_token>
Content-Type: application/json

{
"userId": <S1_Id>,
"databaseInstanceId": <D2_Id>
}

Expected result:

    HTTP 400 Bad Request

    Body:

        {
          "error": "Database instance is not active."
        }

TC-07 – Missing or invalid body

    Description: Admin sends body with missing fields.

    Request:

    POST /api/admin/assign-instance
    Authorization: Bearer <admin_token>
    Content-Type: application/json

    {
      "userId": <S1_Id>
    }

    Expected result:

        HTTP 400 Bad Request

        Body with validation errors (e.g. databaseInstanceId is required).

TC-08 – No token / invalid token

    Description: Call without Authorization header or with invalid JWT.

    Request:

    POST /api/admin/assign-instance
    Content-Type: application/json

    {
      "userId": <S1_Id>,
      "databaseInstanceId": <D1_Id>
    }

    Expected result:

        HTTP 401 Unauthorized.

TC-09 – Student token calling admin endpoint

    Description: A student tries to call the admin assignment endpoint.

    Request:

POST /api/admin/assign-instance
Authorization: Bearer <student_token>
Content-Type: application/json

{
"userId": <S1_Id>,
"databaseInstanceId": <D1_Id>
}

Expected result:

    HTTP 403 Forbidden

    Body:

        {
          "error": "Forbidden: Admin role required."
        }

4. Test Cases – GET /api/student/my-instance
   TC-10 – Student with valid assignment

   Description: Student with an assigned instance retrieves their info.

   Preconditions:

        UserInstance row exists linking student S3 to DatabaseInstance D1.

   Request:

GET /api/student/my-instance
Authorization: Bearer <student_token_of_S3>

Expected result:

    HTTP 200 OK

    Body (example):

        {
          "userId": <S3_Id>,
          "databaseInstanceId": <D1_Id>,
          "databaseName": "MySQL-Instance-01",
          "isActive": true,
          "assignedAt": "2025-11-20T15:00:00Z"
        }

TC-11 – Student with no assigned instance

    Description: Student has no row in UserInstances.

    Request:

GET /api/student/my-instance
Authorization: Bearer <student_token_of_S4_without_instance>

Expected result:

    HTTP 404 Not Found

    Body:

        {
          "error": "No instance assigned for this student."
        }

TC-12 – Admin calling student endpoint

    Description: Admin tries to call /api/student/my-instance.

    Request:

GET /api/student/my-instance
Authorization: Bearer <admin_token>

Expected result:

    HTTP 403 Forbidden

    Body:

        {
          "error": "Forbidden: Student role required."
        }

TC-13 – No token / invalid token

    Description: No JWT or invalid JWT.

    Request:

    GET /api/student/my-instance

    Expected result:

        HTTP 401 Unauthorized.

5. Execution with Postman

Recommended structure for the Postman collection:

    Folder: Auth

        POST /api/auth/login (Admin)

        POST /api/auth/login (Student)

    Folder: Admin

        POST /api/admin/assign-instance

            One request per test case (TC-01 to TC-09) or use examples.

    Folder: Student

        GET /api/student/my-instance

            Requests for TC-10 to TC-13.

You can also save example responses in Postman for demo purposes (success + main error cases).
6. Responsibilities (who tested what)
   Emmanuel – Business rule and validation design

   Defined the core scenario tests for:

        “1 student = 1 instance” rule.

        Role-based access control for Admin and Student.

   Wrote this file: INSTANCE_VALIDATION_TESTS.md.

   Suggested HTTP status codes and sample responses.

Daniel – EMManual / Automated Testing

    Implemented the logic needed to pass these tests.

    Created and executed Postman requests for all test cases (TC-01 to TC-13).

    Collected screenshots / evidence of:

        Successful assignment.

        Correct error responses (401, 403, 404, 409).

    Optionally implemented integration tests in the solution to validate these flows automatically.
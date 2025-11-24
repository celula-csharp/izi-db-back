# INSTANCE_ERROR_CASES – Instance Assignment & Access

This document describes the **main error cases** related to:

- Assigning a database instance to a student:
    - `POST /api/admin/assign-instance`
- Getting the current student's instance:
    - `GET /api/student/my-instance`

It is meant to help QA, teammates and teachers understand how the API behaves in **edge cases** and **invalid scenarios**.

---

## 1. POST `/api/admin/assign-instance`

Assigns a `DatabaseInstance` to a `User` (typically a student).  
**Only Admins** can call this endpoint.

### 1.1. No token / invalid token

- **Scenario:** Request without `Authorization` header or with an invalid/expired JWT.
- **Example:**

  ```http
  POST /api/admin/assign-instance

    Expected response:

        HTTP 401 Unauthorized

        Body (example):

        {
          "error": "Unauthorized"
        }

1.2. Authenticated but not Admin

    Scenario: A user with role Student tries to call /api/admin/assign-instance.

    Header:

Authorization: Bearer <student_token>

Expected response:

    HTTP 403 Forbidden

    Reason: Fails [Authorize(Roles = "Admin")].

Example body:

    {
      "error": "Forbidden: Admin role required."
    }

1.3. Missing or invalid body

    Scenario: Body is empty or missing required fields (userId, databaseInstanceId).

{
"userId": 3
}

Expected response:

    HTTP 400 Bad Request

    Body (example):

        {
          "error": "Invalid request data.",
          "details": [
            "databaseInstanceId is required"
          ]
        }

1.4. User does not exist

    Scenario: userId does not match any row in Users.

{
"userId": 9999,
"databaseInstanceId": 1
}

Expected response:

    HTTP 404 Not Found

    Body:

        {
          "error": "User not found."
        }

1.5. User is not a Student

    Scenario: The user exists but does not have role Student (e.g. role is Admin).

    Expected response:

        HTTP 400 Bad Request (or 409 Conflict, depending on the design)

        Body:

        {
          "error": "Target user must have role 'Student'."
        }

1.6. DatabaseInstance does not exist

    Scenario: databaseInstanceId does not match any row in DatabaseInstances.

    Expected response:

        HTTP 404 Not Found

        Body:

        {
          "error": "Database instance not found."
        }

1.7. DatabaseInstance is inactive

    Scenario: The instance exists but IsActive = false.

    Expected response:

        HTTP 400 Bad Request

        Body:

        {
          "error": "Database instance is not active."
        }

1.8. Student already has an assigned instance

 Business rule violation: "1 student = 1 instance"

    Scenario: The student already has a row in UserInstances with the same UserId.

    Expected response:

        HTTP 409 Conflict (recommended)

        Body:

        {
          "error": "The student already has an assigned instance."
        }

        This is one of the key business rules of the project.

2. GET /api/student/my-instance

Returns the instance assigned to the currently authenticated student.
Only Students can call this endpoint.
2.1. No token / invalid token

    Scenario: Request without Authorization header or with an invalid/expired JWT.

    Expected response:

        HTTP 401 Unauthorized

    Example:

    {
      "error": "Unauthorized"
    }

2.2. Authenticated but not Student

    Scenario: A user with role Admin calls /api/student/my-instance.

    Expected response:

        HTTP 403 Forbidden

        Fails [Authorize(Roles = "Student")].

    Example:

    {
      "error": "Forbidden: Student role required."
    }

2.3. Student has no assigned instance

    Scenario: There is no UserInstance row for this UserId.

    Expected response:

        HTTP 404 Not Found

        Body:

        {
          "error": "No instance assigned for this student."
        }

2.4. DatabaseInstance referenced but inactive

    Scenario: UserInstance exists, but the linked DatabaseInstance has IsActive = false.

    Expected response:

        HTTP 200 OK or 409/400 (depending on design).
        A simple, clear option:

    {
      "userId": 3,
      "databaseInstanceId": 1,
      "isActive": false,
      "message": "The assigned database instance is currently inactive."
    }

3. Postman Testing Checklist

To validate the behavior of these endpoints, the Postman collection should include tests for:
3.1. /api/admin/assign-instance

     Success:

        Admin assigns an instance to a student that has no instance yet.

     Errors:

        No token.

        Token from Student (role mismatch).

        Invalid body (missing fields).

        Non-existing userId.

        Non-existing databaseInstanceId.

        User role != Student.

        Inactive DatabaseInstance.

        Student already has an assigned instance (business rule).

3.2. /api/student/my-instance

     Success:

        Student with valid assignment gets their instance data.

     Errors:

        No token.

        Token from Admin (role mismatch).

        Student with no assignment.

        Instance inactive (optional behavior to test).

4. Responsibilities (who covered what)
   Emmanuel – Business Rule & Error Design

   Designed and documented the core business rule:

        “1 student = 1 instance”.

   Defined the main error scenarios related to:

        Instance assignment.

        Instance access for students.

   Helped define proper HTTP status codes (400, 401, 403, 404, 409).

   Documented this file: INSTANCE_ERROR_CASES.md.

Daniel – Implementation & Postman Tests

    Implemented the logic to:

        Validate user role and existence.

        Validate DatabaseInstance.

        Enforce “1 student = 1 instance” in the service / controller.

    Created Postman requests and test cases for:

        Success scenarios.

        Error scenarios listed in this document.

    Verified that the API responses and HTTP status codes match the documentation.
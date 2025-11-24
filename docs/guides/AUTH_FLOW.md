##  `AUTH_FLOW.md`

```markdown
# AUTH_FLOW – Core & Authentication

This document describes the **authentication flow (login / register)** and **role-based authorization (Admin / Student)** of the _izi-db-back_ platform.

---

## 1. Relevant Entities

- **User**
  - Key fields: `Id`, `Username`, `Email`, `PasswordHash`, `RoleId`.
- **Role**
  - Examples: `Admin`, `Student`.
- **DatabaseInstance**
  - Represents a database engine instance (MySQL, PostgreSQL, etc.).
- **UserInstance**
  - Links a `User` to a `DatabaseInstance`.
  - Business rule: **1 student = 1 instance**.

---

## 2. Authentication Flow (JWT)

### 2.1 Register (`POST /api/auth/register`)

1. The client sends `username`, `email`, `password` (and optionally `role`).
2. The backend:
   - Validates that the username/email are not already taken.
   - Hashes the password.
   - Creates the `User` in the `Users` table.
   - Assigns a `Role` (for example, `Student` or `Admin`).
3. It can return:
   - Basic user data.
   - An initial JWT token (depending on the implementation).

> This endpoint is implemented by the **Auth** module (shared team responsibility).

---

### 2.2 Login (`POST /api/auth/login`)

1. The client sends `username` and `password`.
2. The backend:
   - Looks up the user by `username` or `email`.
   - Validates the `PasswordHash`.
   - Retrieves the associated `Role`.
3. If credentials are valid, a **JWT** is generated with:
   - Claim `sub` or `nameid` → `User.Id`.
   - Claim `role` → `Role.Name` (`Admin` or `Student`).
4. The backend returns an object such as:

```json
{
  "accessToken": "<jwt>",
  "expiresIn": 3600,
  "user": {
    "id": 3,
    "username": "student1",
    "role": "Student"
  }
}

2.3 Me (GET /api/auth/me)

    The client sends the header:
    Authorization: Bearer <jwt>.

    The authentication middleware validates the token.

    The controller extracts userId from the token.

    The backend returns information about the authenticated user.

3. JWT & Middleware Configuration
3.1 JWT in Program.cs

    Authentication is registered as follows:

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };
    });

    Values are read from configuration:

        Jwt:Key

        Jwt:Issuer

        Jwt:Audience

3.2 Roles & Authorization

In Program.cs:

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    options.AddPolicy("StudentPolicy", policy => policy.RequireRole("Student"));
});

And in the controllers:

    [Authorize(Roles = "Admin")]

    [Authorize(Roles = "Student")]

4. Instance Assignment Flow
4.1 Assign Instance (Admin)

Endpoint: POST /api/admin/assign-instance
Required role: Admin

    The Admin logs in and gets an accessToken.

    Sends a request with header:

    Authorization: Bearer <token_admin>

    and body:

    {
      "userId": 3,
      "databaseInstanceId": 1
    }

    The backend:

        Validates that the user exists and has role Student.

        Validates that the DatabaseInstance exists and is active.

        Verifies that the user does not already have an assigned instance.

        Creates a record in UserInstances.

    Successful response: 200 OK.

If the student already has an instance, a business error is returned (e.g., 409 Conflict or 400 BadRequest with message "The student already has an assigned instance.").
4.2 View My Instance (Student)

Endpoint: GET /api/student/my-instance
Required role: Student

    The Student logs in and gets an accessToken.

    Sends a request with header:

    Authorization: Bearer <token_student>

    The backend:

        Reads userId from the token (claim NameIdentifier/sub).

        Looks up in UserInstances where UserId = userId.

        Includes the related DatabaseInstance.

    Successful response (200 OK) includes:

        userId

        databaseInstanceId

        databaseName

        isActive

        assignedAt

If the student has no assigned instance, it returns 404 Not Found.
5. Full Flow Summary

    User registers or is created by an Admin.

    User logs in → receives a JWT with userId and role.

    Depending on the role:

        Admin → can call /api/admin/assign-instance.

        Student → can call /api/student/my-instance.

    The authentication middleware validates the token.

    The authorization middleware verifies the role.

    Business rule is applied:
    A student can only have 1 instance assigned.

6. Team & Responsibilities
Emmanuel – Core, Domain & Instance Logic

    Designed the domain model (User, Role, DatabaseInstance, UserInstance).

    Configured SystemDbContext and relationships.

    Defined service interfaces:

        IJwtService

        IAuthService

    Implemented services in infrastructure/Auth:

        JwtService (token generation).

    Implemented instance logic:

        Instance assignment service (InstanceAssignmentService).

        Business rule: “1 student = 1 instance”.

    Implemented endpoints:

        POST /api/admin/assign-instance

        GET /api/student/my-instance

    Documentation:

        README_BD.md

        AUTH_FLOW.md (main structure)

        INSTANCE_ERROR_CASES.md

Daniel – Auth Endpoints & Testing

    Implemented endpoints:

        POST /api/auth/register

        POST /api/auth/login

        GET /api/auth/me

    Integrated AuthService with User and Role.

    Used JwtService to:

        Generate JWT on login.

        Include userId and role claims.

    Configured and tested Postman collection:

        Endpoints:

            Auth (/api/auth/...)

            Admin (/api/admin/...)

            Student (/api/student/...)

        Tested success and error scenarios.

    Helped with JWT and role configuration testing.
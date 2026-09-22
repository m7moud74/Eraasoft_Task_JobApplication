# 📋 Track Application System — EraaSoft Backend Task

> **Note:** This project is developed as a backend graduation/training task for **EraaSoft**. It demonstrates building a clean, scalable, and maintainable RESTful Web API using **ASP.NET Core (.NET 10)** following **Clean Architecture** principles.

---

## 🏗️ Architecture Overview (Clean Architecture)

The solution is divided into 4 decoupled layers following the Clean Architecture dependency rule (`API -> Application -> Infrastructure -> Domain`):

```
├── Domain               # Core enterprise business logic (Entities, Enums, Domain Exceptions)
├── Application          # Application business logic (Services, Interfaces, DTOs, Exceptions)
├── InfraStructure       # External concerns (EF Core, SQL Server, Identity, JWT, Repositories)
└── Api                  # Presentation layer (Controllers, Authentication, Swagger, Middleware)
```

### 1. Domain Layer
- **Entities**:
  - `Job`: Represents job listings (`Id`, `Title`, `Description`, `IsActive`).
  - `Candidate`: Represents candidate profiles (`Id`, `Name`, `Email`, `CvUrl`).
  - `JobCandidateApplication`: Represents applications submitted by candidates for specific jobs, including application status and timestamps.
- **Enums**:
  - `JobApplicationStatus`: (`Applied`, `UnderReview`, `InterView`, `Accepted`, `Rejected`, `Cancelled`).
- **Domain Exceptions**:
  - `DomainException`: Enforces core business invariants (e.g. preventing cancellation of an already cancelled application or an application in the interview phase).

### 2. Application Layer (CQRS with MediatR)
- **CQRS Pattern**:
  - **Commands (`Application/Feature/Command/`)**:
    - `Applications/`: `ApplyJobCommand`, `UpdateApplicationStatusCommand`, `CancelApplicationCommand`
    - `Jobs/`: `CreateJobCommand`, `UpdateJobCommand`, `CloseJobCommand`, `DeleteJobCommand`
    - `Candidates/`: `UpdateCandidateCommand`, `DeleteCandidateCommand`
    - `Auth/`: `RegisterCommand`, `LoginCommand`
  - **Queries (`Application/Feature/Query/`)**:
    - `Applications/`: `GetApplicationByIdQuery`, `GetMyApplicationsQuery`, `GetJobApplicationsQuery`
    - `Jobs/`: `GetAllJobsQuery`, `GetJobByIdQuery`
    - `Candidates/`: `GetAllCandidatesQuery`, `GetCandidateByIdQuery`
- **Interfaces**:
  - `IJobRepository`, `ICandidateRepository`, `IJobCandidateApplicationRepository`, `IAuthService`, `ICurrentUserService`.
- **DTOs**:
  - Strongly-typed request/response models decoupling internal database entities from external clients.

### 3. Infrastructure Layer
- **Persistence**: Entity Framework Core 10 with SQL Server.
- **Identity**: `ApplicationUser : IdentityUser` linked 1-to-1 with the `Candidate` domain entity.
- **Authentication**: `JwtTokenService` generating secure JWT bearer tokens with standard claims (`sub`, `email`, `role`, `candidate_id`).
- **Repositories**: Generic and specialized repositories.
- **Data Seeding**: EF Core `DataSeeder` (`HasData`) seeding roles, users, jobs, candidates, and applications directly through migrations.

### 4. API Layer
- **Controllers**:
  - `AuthController`: User registration (Candidate / Recruiter) and login.
  - `JobsController`: Public browsing, Recruiter/Admin job creation, creator-only closing and updating.
  - `CandidatesController`: Candidate self-service profile update and Candidate/Admin deletion.
  - `ApplicationsController`: Job application submission, job-creator status management, and cancellation.
- **Current User Resolution**: `CurrentUserService` accessing authenticated claims securely via `IHttpContextAccessor`.
- **Swagger / OpenAPI**: Interactive API documentation configured with Bearer Token authentication.

---

## 🔒 Security & Role-Based Authorization

The application supports three primary roles:

1. **Admin**:
   - Create jobs and delete jobs.
   - View all candidates and delete candidate profiles.
   - View all applications submitted for jobs.
   - *Note*: Cannot update candidate profiles (only candidates themselves can update their profile).
   - *Note*: Cannot close a job unless they opened/created it.

2. **Recruiter**:
   - Create new jobs (recorded with `CreatedByUserId`).
   - Update and close jobs they created (`PUT /api/jobs/{id}/close`).
   - View applications submitted for their jobs (`GET /api/applications/job/{jobId}`).
   - Update application review statuses (`PUT /api/applications/{id}/status`) for their jobs.

3. **Candidate**:
   - Apply to active jobs (`candidateId` resolved from authenticated JWT token).
   - View their own submitted applications (`/api/applications/my`).
   - Cancel their own pending applications (`Applied` or `UnderReview`).
   - View and update their own candidate profile (`PUT /api/candidates/{id}`).
   - Delete their own candidate profile (`DELETE /api/candidates/{id}`).

---

## 📡 API Endpoints Reference

### 🔐 Authentication (`/api/auth`)
| Method | Route | Authorization | Description |
|---|---|---|---|
| `POST` | `/api/auth/register` | Public | Register a new candidate or recruiter (`role: "Candidate"` or `"Recruiter"`) |
| `POST` | `/api/auth/login` | Public | Authenticate user and obtain a JWT |

### 💼 Jobs (`/api/jobs`)
| Method | Route | Authorization | Description |
|---|---|---|---|
| `GET` | `/api/jobs` | Public | Get all jobs (supports query `?activeOnly=true`) |
| `GET` | `/api/jobs/{id}` | Public | Get job by ID |
| `POST` | `/api/jobs` | `Recruiter`, `Admin` | Create a new job |
| `PUT` | `/api/jobs/{id}` | Job Creator | Update job details |
| `PUT` | `/api/jobs/{id}/close` | Job Creator | Close job (`IsActive = false`) |
| `DELETE` | `/api/jobs/{id}` | Job Creator / `Admin` | Delete a job (prevented if applications exist) |

### 👤 Candidates (`/api/candidates`)
| Method | Route | Authorization | Description |
|---|---|---|---|
| `GET` | `/api/candidates` | `Admin` | Get all candidate profiles |
| `GET` | `/api/candidates/{id}` | `Candidate` (Owner) / `Admin` | Get candidate profile by ID |
| `PUT` | `/api/candidates/{id}` | `Candidate` (Owner only) | Update candidate profile (Admin excluded) |
| `DELETE` | `/api/candidates/{id}` | `Candidate` (Owner) / `Admin` | Delete candidate profile |

### 📄 Applications (`/api/applications`)
| Method | Route | Authorization | Description |
|---|---|---|---|
| `POST` | `/api/applications` | `Candidate` | Apply to a job (`{"jobId": 1}`) |
| `GET` | `/api/applications/{id}` | `Candidate` (Owner) / Job Creator / `Admin` | Get application by ID |
| `GET` | `/api/applications/my` | `Candidate` | Get all applications of the logged-in candidate |
| `GET` | `/api/applications/job/{jobId}` | Job Creator / `Admin` | Get all applications for a specific job |
| `PUT` | `/api/applications/{id}/status` | Job Creator | Update application status |
| `DELETE` | `/api/applications/{id}` | `Candidate` (Owner) | Cancel application |

---

## 👥 Pre-seeded Test Accounts

The EF Core migration automatically seeds the database with the following accounts:

| Role | Email | Password | Linked Entity |
|---|---|---|---|
| **Admin** | `admin@trackapplication.com` | `Admin@123456` | System Administrator |
| **Recruiter** | `recruiter@trackapplication.com` | `Recruiter@123456` | Job Creator (Jobs #1, #2) |
| **Candidate** | `ahmed@example.com` | `Candidate@123456` | Candidate #1 (Ahmed Ali) |
| **Candidate** | `sara@example.com` | `Candidate@123456` | Candidate #2 (Sara Mohamed) |

---

## 🚀 Getting Started & Execution

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQL Server (LocalDB or SQL Server Express)

### 1. Database Configuration
Verify the connection string in [Api/appsettings.json](file:///d:/Eraa%20soft/Api/appsettings.json):
```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=JobApplicationDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

### 2. Apply Migrations
Update the database schema:
```powershell
dotnet ef database update --project InfraStructure --startup-project Api
```

### 3. Run the API
```powershell
dotnet run --project Api/JobaApplication.Api.csproj
```

### 4. Testing the Endpoints
- **Swagger UI**: Navigate to `http://localhost:5102/swagger` in your browser. Use the **Authorize** button with `Bearer <your_token>`.
- **VS Code / Visual Studio REST Client**: Use the ready-to-run [Api/JobaApplication.Api.http](file:///d:/Eraa%20soft/Api/JobaApplication.Api.http) file.

---

## 🛡️ Key Business Rules Enforced

1. **Secure Candidate Identity**: The client does not supply `candidateId` when applying to a job; it is extracted exclusively from the validated JWT token.
2. **Duplicate Application Prevention**: Candidates cannot submit multiple applications for the same job.
3. **Inactive Job Guard**: Applications cannot be submitted for inactive jobs (`IsActive = false`).
4. **Historical Data Integrity**: Jobs with submitted applications cannot be deleted.
5. **Strict Cancellation Workflow**: Applications can only be cancelled while in `Applied` or `UnderReview` status. Applications that have reached `InterView`, `Accepted`, or `Rejected` cannot be cancelled.

Candidate Management Module
Backend Setup Documentation
________________________________________
1. System Requirements
1.1 Operating System
•	Windows 10 or Windows 11 (64-bit)
________________________________________
1.2 Required Applications (Install in this exact order)
? 1. .NET SDK
•	Version: .NET 8 SDK
•	Download from: https://dotnet.microsoft.com/download
•	Verify installation.
•	dotnet --version
Expected output:
8.x.x
________________________________________
? 2. Visual Studio 202x
•	Edition: Community / Professional
•	During installation, select workloads:
o	ASP.NET and web development
o	.NET desktop development
•	Verify:
o	Open Visual Studio
o	Create/open a .NET Web API project successfully
________________________________________
? 3. SQL Server (Local Development)
Choose one of the following:
Option A (Recommended)
•	Enterprise Developer edition
•	Download: https://www.microsoft.com/en-in/sql-server/sql-server-downloads
Option B
•	SQL Server Express
________________________________________
? 4. SQL Server Management Studio (SSMS)
•	Download: https://learn.microsoft.com/sql/ssms/download-sql-server-management-studio-ssms
•	Used only for:
o	Viewing databases
o	Verifying tables
o	Debugging data
________________________________________
2. Repository Structure (Expected)
Project/
?? CandidateManagement/
?        ?? CandidateService/
?        ?? InterviewService/
?        ?? RequirementService/
?        ?? ReportService/
?   ??? CandidateManagement.sln
??? GatewayAPI/
________________________________________
3. Opening the Solution
1.	Open Visual Studio 202x
2.	Click Open a project or solution
3.	Select:
4.	CandidateManagement/CandidateManagement.sln
5.	Ensure 5 projects are visible:
o	CandidateService
o	InterviewService
o	RequirementService
o	ReportService
o	GatewayAPI
________________________________________
4. Backend Architecture Overview
Service											Responsibility
CandidateService											Candidate CRUD
InterviewService											Interview scheduling
RequirementService											Job requirements
ReportService											Aggregated reporting
GatewayAPI											Single entry point
											
Each service:
•	Is an independent ASP.NET Web API
•	Has its own database
•	Communicates via HTTP (REST)
________________________________________
5. Database Configuration
5.1 Connection Strings
Each service contains:
appsettings.json
Example:
"ConnectionStrings": { “DefaultConnection": "Server = localhost;Database = CandidateDb; Trusted_Connection=True;
TrustServerCertificate=True"}
?? Database names must be unique per service:
•	CandidateDb
•	InterviewDb
•	RequirementDb
•	ReportDb
________________________________________
6. Database Creation & Migrations
6.1 Important Behavior
•	Databases are NOT created automatically on first run
•	Entity Framework Core migrations must be applied manually
•	This ensures predictable schema creation
________________________________________
6.2 Open Package Manager Console
In Visual Studio:
Tools ? NuGet Package Manager ? Package Manager Console
________________________________________
6.3 Run Migrations (For EACH Service) in the Packet Manager Terminal.
1. CandidateService
cd CandidateService
Add-Migration InitialCreate
Update-Database
2. InterviewService
cd InterviewService
Add-Migration InitialCreate
Update-Database
3. RequirementService
cd RequirementService
Add-Migration InitialCreate
Update-Database
4. ReportService
cd ReportService
Add-Migration InitialCreate
Update-Database
________________________________________
6.4 Verify Databases
1.	Open SQL Server Management Studio
2.	Connect to:
3.	(local)
4.	Confirm databases exist:
o	CandidateDb
o	InterviewDb
o	RequirementDb
o	ReportDb
________________________________________
7. Running the Backend (All Services)
7.1 Configure Multiple Startup Projects
1.	Right-click Solution ? Properties
2.	Go to Configure Startup Projects
3.	Select Multiple startup projects
4.	Set all to Start:
o	CandidateService
o	InterviewService
o	RequirementService
o	ReportService
o	GatewayAPI
5.	Click Apply ? OK
________________________________________
7.2 Start the Backend
•	Press F5 (Debug) or Ctrl + F5
•	All services should start simultaneously
________________________________________
8. Verifying Each Service (Swagger)
8.1 CandidateService
https://localhost:<port>/swagger
Verify:
•	POST /api/candidates
•	GET /api/candidates
________________________________________
8.2 InterviewService
Verify:
•	POST /api/interviews
•	GET /api/interviews/candidate/{id}
________________________________________
8.3 RequirementService
Verify:
•	POST /api/requirements
•	GET /api/requirements
________________________________________
8.4 ReportService
Verify:
•	GET /api/reports/summary
________________________________________
8.5 GatewayAPI (Primary Verification)
https://localhost:7221/swagger
Confirm:
•	All routes from all services are visible
•	Requests are forwarded correctly
•	Responses are successful
________________________________________
9. Expected Behavior Validation
9.1 Data Flow Check
1.	Create Candidate via Gateway
2.	Schedule Interview
3.	Create Requirement
4.	Call:
5.	GET /api/reports/summary
Expected:
•	Aggregated counts reflect created database.
________________________________________
10. Common Troubleshooting
? Connection Refused
•	Ensure all services are running
•	Verify correct ports in Gateway configuration
________________________________________
? 400 Bad Request
•	Check DTO validation
•	Ensure request payload matches Swagger schema
________________________________________
? Database Not Found
•	Confirm Update-Database was executed
•	Verify connection string database name
***

# App Service + Azure SQL sample

This .NET 10 minimal web app reads `dbo.TrainingItems` using matching `Microsoft.Data.SqlClient` and `Microsoft.Data.SqlClient.Extensions.Azure` packages with Microsoft Entra authentication. On the App Service, `Authentication=Active Directory Default` uses the app's configured managed identity. No SQL username or password is used.

For Windows prerequisites, VS Code setup, Azure CLI sign-in, database preparation, local run instructions, and troubleshooting, follow the [environment setup guide](./SETUP.md).

**Attendees starting locally:** follow the [SQL Express walkthrough](./SETUP.md#attendee-walkthrough-local-sql-express-from-setup-to-debugging).
It covers installation, database creation, configuration, running/debugging, and
stopping duplicate instances. No Azure resources are required for that path.
The Azure path requires an existing Azure SQL server and database; placeholder
connection strings do not create them.

## Files

- `Program.cs`: web endpoints and the SQL query.
- `sql/setup.sql`: sample table, one training row, contained database user, and table-scoped read permission.
- `AppServiceSqlSample.csproj`: .NET target and matching SQL client/Azure authentication packages.

## Run locally (optional)

### Local SQL Express alternative (no Azure required)

For local testing on your own computer, use `.\SQLEXPRESS` with Windows
authentication instead of Azure SQL. Run [sql/setup-local.sql](./sql/setup-local.sql)
in SSMS using an account authorized to create the separate `LbsTraining` database.
It creates the sample table and row without changing existing application databases.
Replace the instance name if your installation differs; the instructor's
`<instructor-computer>\SQLEXPRESS` name applies only to that computer.

Save the local connection string and run:

```powershell
dotnet user-secrets set "ConnectionStrings:TrainingDatabase" "Server=.\SQLEXPRESS;Initial Catalog=LbsTraining;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;"
dotnet run --launch-profile AppServiceSqlSample
```

Remove any `ConnectionStrings__TrainingDatabase` environment variable override
before running if it still points to Azure SQL. Open
`http://localhost:5080/api/items`, or use **Debug AppServiceSqlSample** in VS Code.
`TrustServerCertificate=True` is for this local development instance only.
This setup does not need `az login` or managed identity. A deployed Azure App
Service cannot use your computer's Windows identity to connect to this instance;
keep the Azure configuration below for the cloud lab.

### Azure SQL from your local machine

Prerequisites: .NET 10 SDK, Azure CLI, access to the training database, and an Entra database user with permission to read `dbo.TrainingItems`. Local runs authenticate as the signed-in developer; this is not the App Service managed identity.

In PowerShell, open the `AppServiceSqlSample` project folder, then replace the server and database placeholders with the instructor-provided training values:

```powershell
# Open your copied AppServiceSqlSample folder in VS Code first.
# Run these commands from its integrated PowerShell terminal.
az login
$env:ConnectionStrings__TrainingDatabase = "Server=tcp:<server-name>.database.windows.net,1433;Initial Catalog=<database-name>;Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
dotnet run --project .\AppServiceSqlSample.csproj
```

Open the local URL printed by `dotnet run` and visit `/api/items`. The response should contain the seeded training row. If local database access is not pre-authorized, inspect the code and have the instructor demonstrate the deployed app instead.

### Debug in VS Code

Terminal environment variables are not automatically inherited by a VS Code instance
that is already running. The SQL Express user secret above works for debugging too;
do not replace it with an Azure string unless switching to the Azure lab.
For Azure SQL only, save the connection string using .NET user secrets (replace
both placeholders with existing resource names):

```powershell
dotnet user-secrets set "ConnectionStrings:TrainingDatabase" "Server=tcp:<server-name>.database.windows.net,1433;Initial Catalog=<database-name>;Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
az login
```

In Run and Debug, select **Debug AppServiceSqlSample** and press **F5**.
The supplied debug configuration and project launch profile set both
`DOTNET_ENVIRONMENT=Development` and `ASPNETCORE_ENVIRONMENT=Development`,
which enables user secrets, and listen at `http://localhost:5080`.
The project profile also applies to `dotnet run` and project-based C# debugging.
If the debugger is already paused on a startup exception, stop it and start a new
debug session; continuing the paused session does not reload its environment.
Stop any existing app using that port first. Do not put credentials in the debug
configuration or commit them to source files.

## App Service configuration

Publish with `dotnet publish -c Release -o publish`. Deploy only to the instructor-designated training App Service.

In that App Service's configuration, add the application setting `ConnectionStrings__TrainingDatabase` with a value in this shape:

```text
Server=tcp:<server-name>.database.windows.net,1433;Initial Catalog=<database-name>;Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;
```

This setting contains the server and database names, not credentials. Confirm the App Service's system-assigned identity is enabled and the database setup script has been applied by an authorized instructor. Restart the app after approved configuration changes.

## Expected API response

`GET /api/items` returns JSON similar to:

```json
[
  {
    "id": 1,
    "name": "Welcome to the lab",
    "description": "This sample row is read by the App Service managed identity."
  }
]
```

The app is intentionally read-only. SQL errors are logged server-side with a trace ID; the HTTP response does not expose database details.

## Reference

- [Securely connect .NET apps to Azure SQL Database using Managed Identity](https://learn.microsoft.com/en-us/azure/app-service/tutorial-connect-msi-sql-database)
- [Managed identities for App Service and Azure Functions](https://learn.microsoft.com/en-us/azure/app-service/overview-managed-identity)

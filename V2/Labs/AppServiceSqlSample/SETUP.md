# Set up the App Service + Azure SQL lab environment

This guide prepares a Windows development environment for the sample in this folder. Choose one path before starting:

- **Local SQL Express (recommended first):** run everything on your own computer. No Azure subscription, Azure CLI, Azure SQL database, or managed identity is needed. Follow the attendee walkthrough below.
- **Azure lab:** requires an existing Azure SQL server/database and an assigned App Service, provided or explicitly assigned for creation by the instructor. Follow the numbered Azure sections after the walkthrough. A placeholder connection string does not create these resources.

Signing in to C# Dev Kit does not install the .NET SDK.

## Attendee walkthrough: local SQL Express from setup to debugging

### A. Install the prerequisites

1. Install the **.NET 10 SDK**, **VS Code**, and Microsoft's **C# Dev Kit** extension. See sections 1 and 2 below for details. Restart VS Code after installing the SDK and verify `dotnet --version` prints `10.0.xxx`.
2. Install [SQL Server Express](https://www.microsoft.com/sql-server/sql-server-downloads) using a local named instance such as `SQLEXPRESS`. Record the instance name shown by the installer. This guide assumes the database engine runs on your own Windows computer.
3. Install [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/ssms/install/install). SSMS is the management tool; installing it alone does not install the SQL Server database engine.
4. In Windows **Services**, confirm **SQL Server (SQLEXPRESS)** is running, or start the service for your chosen instance.

### B. Connect and create the training database

1. Open SSMS and connect using **Windows Authentication**.
2. For the default local Express instance, use `.\SQLEXPRESS` as the server name. If you chose another instance, use its actual name. The instructor's example `<instructor-computer>\SQLEXPRESS` is specific to that computer; do not copy it onto another attendee's computer.
3. If SSMS asks about the local development server's untrusted certificate, enable **Trust server certificate** for this local connection only.
4. Open [sql/setup-local.sql](./sql/setup-local.sql) using **File > Open > File** and execute it against your local instance. Your Windows account needs permission to create the lab database; if denied, ask the instructor rather than changing permissions on an unrelated database.
5. Refresh **Databases** in Object Explorer. You should see **LbsTraining** with **Tables > dbo.TrainingItems**. The script creates a separate database and one training row; it does not modify existing application databases. It can be rerun without duplicating the seeded row.
6. Verify in a new SSMS query:

   ```sql
   USE LbsTraining;
   SELECT Id, Name, Description FROM dbo.TrainingItems;
   ```

Do not run [sql/setup.sql](./sql/setup.sql) for this path: that script contains Azure managed-identity permissions.

### C. Configure the project once

Open **File > Open Folder** in VS Code and select the folder containing `AppServiceSqlSample.csproj`, not a parent folder or just the source file.

Open **Terminal > New Terminal** using PowerShell. The following commands must run from that project folder:

```powershell
dotnet restore .\AppServiceSqlSample.csproj
dotnet build .\AppServiceSqlSample.csproj
dotnet user-secrets set "ConnectionStrings:TrainingDatabase" "Server=.\SQLEXPRESS;Initial Catalog=LbsTraining;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;"
```

Replace `.\SQLEXPRESS` if your instance has a different name. Windows authentication uses the Windows account running the app; no SQL password or `az login` is needed. `TrustServerCertificate=True` is only for this local development setup, not the Azure connection.

The project already has a `UserSecretsId`; do not generate a different one. User secrets save the setting outside the project for your Windows user. Do not copy secrets into source control or debug configuration files.

If you previously set an Azure connection string in this terminal, remove the override before starting:

```powershell
if (Test-Path Env:\ConnectionStrings__TrainingDatabase) {
    Remove-Item Env:\ConnectionStrings__TrainingDatabase
}
```

Environment variables override user secrets. If you saved an override in Windows **Environment Variables**, remove that obsolete setting there too and restart VS Code.

### D. Run and verify (one instance only)

```powershell
dotnet run --project .\AppServiceSqlSample.csproj --launch-profile AppServiceSqlSample
```

Wait for **Now listening on: http://localhost:5080** and **Hosting environment: Development**. Keep the terminal running.

- [Home page](http://localhost:5080/): links to the endpoints. Its Azure-oriented heading does not determine which database is used; your connection string does.
- [Health check](http://localhost:5080/health): returns `{"status":"ok"}`; this checks the web process, not SQL.
- [Data endpoint](http://localhost:5080/api/items): queries SQL Express and returns:

  ```json
  [{"id":1,"name":"Welcome to the lab","description":"This sample row is read from local SQL Express using Windows authentication."}]
  ```

JSON is the expected API output, not an error or a missing web page. The data endpoint is the end-to-end success check.

### E. Stop, restart, or debug

- **Terminal run:** press **Ctrl+C** in the terminal that started the app.
- **Debug run:** press **Shift+F5** to stop it.
- To debug, first stop any terminal/background copy on port 5080. In **Run and Debug** (**Ctrl+Shift+D**), select **Debug AppServiceSqlSample**, then press **F5**. Set a breakpoint in the `/api/items` handler and refresh the data endpoint.
- Use the named project configuration, not a file-level Run/Debug action on `Program.cs`. Both [.vscode/launch.json](./.vscode/launch.json) and [Properties/launchSettings.json](./Properties/launchSettings.json) select Development mode and port 5080. Development mode enables user secrets.
- After changing a connection string or launch environment, stop the app and start a new session. Continuing an exception-paused session does not reload its startup configuration.
- Alternatively, attach without starting a second copy: open the Command Palette (**Ctrl+Shift+P**), choose **Debug: Attach to a .NET 5+ or .NET Core process**, and select the process for this project's `AppServiceSqlSample.exe` or `dotnet.exe` running `AppServiceSqlSample.dll`. Match the process ID using the port lookup below if several instances exist.

The vsdbg license banner is informational, not an application error.

### F. Resolve "address already in use" safely

This means another process already owns the port. Do not repeatedly launch more copies or change ports without checking what is running.

In PowerShell, identify the current owner:

```powershell
$listeners = Get-NetTCPConnection -State Listen |
    Where-Object { $_.LocalPort -in 5000, 5001, 5080 }
$listeners | Select-Object LocalPort, OwningProcess -Unique
$listeners.OwningProcess | Sort-Object -Unique |
    ForEach-Object { Get-Process -Id $_ | Select-Object Id, ProcessName, Path }
```

Prefer **Ctrl+C** in the owning terminal or **Shift+F5** in its debug session. If that is not possible, confirm the process is your unwanted sample instance, then replace the example ID below with its **current** `OwningProcess` value:

```powershell
Stop-Process -Id 12345
```

`12345` is a placeholder, not an ID to copy unchanged. Process IDs change; do not reuse IDs from earlier screenshots or terminate all `dotnet` processes. Never stop an unfamiliar process just to free a port.

Start exactly one app instance afterward. If an error still mentions **5000**, the launch is not using the supplied profile (or an old session is still running); stop it, open the project folder itself, and select **Debug AppServiceSqlSample**.

### G. Troubleshoot the local path

| Symptom | Action |
|---|---|
| `Configure the ConnectionStrings:TrainingDatabase setting` | Run the user-secrets command in the project folder as the same Windows user, remove obsolete environment overrides, and use the Development launch profile. Stop and restart the session. |
| Browser shows `ERR_CONNECTION_REFUSED` | Nothing is accepting connections at that address. Start the app, wait for **Now listening**, and browse to the exact printed URL. Do not stop the app before browsing. |
| `Failed to bind ... address already in use` | Follow the port-owner steps above. Run only one instance on a given port. |
| Error 11001 / `No such host is known` | Check the configured server name. For this path use your actual local SQL instance, not an Azure placeholder or the instructor's computer name. |
| SQL Express connection fails | Verify that SSMS can connect with Windows authentication using the same server name and Windows account, and that the SQL Server service is running. |
| Login denied or cannot open `LbsTraining` | Confirm the database exists on that instance and your Windows account has access. Ask the instructor if permissions are missing. |
| `Invalid object name 'dbo.TrainingItems'` | Run the local setup script and confirm the connection string selects `LbsTraining`. |
| `/health` succeeds but `/api/items` returns 500 | Read the app terminal/debug output for the SQL error; check instance name, database, table, and Windows permissions. The browser intentionally hides database error details. |

**Local setup is complete only when `/api/items` returns the training row.** Azure resources are needed only if you choose the separate cloud lab below; creating a local SQL Express database does not create an Azure SQL database.

## What you need

The requirements and numbered sections below describe the **Azure path**. Local-only attendees can stop after the walkthrough above; sections 1, 2, and 4 also provide shared tooling details.

### On your Windows computer

- Windows 10 or 11 supported by [.NET 10](https://learn.microsoft.com/dotnet/core/install/windows).
- The .NET 10 SDK (not only the runtime).
- Visual Studio Code.
- Azure CLI, for local Microsoft Entra sign-in.
- Internet access to download NuGet packages and to reach the training database, if local database access is enabled.

### Provided by the instructor

- The approved training subscription and resource group.
- An Azure SQL server and database containing the `dbo.TrainingItems` table.
- An assigned App Service with its system-assigned identity enabled.
- A tested network route from the App Service to Azure SQL.
- Database access for the App Service identity. For optional local runs, database access for the learner's Entra account is needed separately.
- The server name, database name, app URL, and an approved deployment method, if deploying is part of the exercise.

Do not create an Azure subscription or resources, change SQL firewall rules, or grant permissions yourself unless the instructor explicitly assigned that step in the isolated training environment.

## 1. Install the .NET 10 SDK

Use the official [.NET on Windows installation page](https://learn.microsoft.com/dotnet/core/install/windows) and install the **.NET 10 SDK** for this computer's processor architecture. The .NET SDK includes the runtime required to run this web application.

You can also use VS Code's **Install .NET SDK System-Wide** command if the .NET Install Tool extension is installed. Accept any Windows administrator prompt if required. An extension-managed runtime alone is not sufficient for `dotnet run` in the terminal.

After installation:

1. Close and reopen VS Code so it refreshes the system `PATH`.
2. Open a new VS Code terminal.
3. Verify the SDK is available:

   ```powershell
   dotnet --list-sdks
   dotnet --version
   ```

   The output should include a `10.0.xxx` SDK (the screenshot shows `10.0.403`). If PowerShell says that `dotnet` is not recognized, restart VS Code or Windows and check the installation before continuing.

The C# extension may report that it cannot load `Program.cs` until an SDK is installed. After installing, use **Developer: Reload Window** from the VS Code Command Palette if the warning remains.

### About the .NET telemetry notice

The telemetry notice is informational; it is not an installation error. .NET CLI telemetry is enabled by default. Read Microsoft's [.NET SDK and CLI telemetry documentation](https://learn.microsoft.com/dotnet/core/tools/telemetry) for what is collected and how it is used.

To opt out for the current PowerShell window before running .NET commands:

```powershell
$env:DOTNET_CLI_TELEMETRY_OPTOUT = "1"
```

To opt out for your Windows user in future terminals:

```powershell
[Environment]::SetEnvironmentVariable("DOTNET_CLI_TELEMETRY_OPTOUT", "1", "User")
```

After setting the user variable, open a new terminal. This setting is optional and does not change the application's behavior.

## 2. Install VS Code extensions

In VS Code, open **Extensions** and install:

- **C# Dev Kit** by Microsoft (recommended for C# project support, IntelliSense, and debugging).

Optional extensions:

- **Azure App Service** by Microsoft, if the instructor wants you to deploy or inspect the training app from VS Code.
- **SQL Server (mssql)** by Microsoft, only if the instructor wants you to inspect SQL using VS Code and has authorized your database access.

C# Dev Kit subscription-benefit sign-in is optional for this sample. It is separate from Azure sign-in and does not replace the .NET SDK.

## 3. Install Azure CLI and sign in

Install Azure CLI using the official [Azure CLI for Windows instructions](https://learn.microsoft.com/cli/azure/install-azure-cli-windows). If WinGet is available, open PowerShell and run:

```powershell
winget install --exact --id Microsoft.AzureCLI
```

Close and reopen the terminal, then verify and sign in:

```powershell
az version
az login
az account show --output table
```

Confirm that the displayed tenant and subscription are the ones approved for training. If your instructor gives you a subscription ID, select it explicitly:

```powershell
az account set --subscription "<training-subscription-id>"
az account show --output table
```

Do not select or make changes in a personal or production subscription.

## 4. Open the project folder

In VS Code, choose **File > Open Folder** and open the `AppServiceSqlSample` folder (the folder containing `AppServiceSqlSample.csproj`), not just `Program.cs`.

The project targets .NET 10 and uses matching `Microsoft.Data.SqlClient` and `Microsoft.Data.SqlClient.Extensions.Azure` packages. The Azure extension enables the `Authentication=Active Directory Default` connection-string mode. It also pins the transitive `System.Security.Cryptography.Pkcs` dependency to a patched version because earlier builds reported security advisories for the older transitive version. Restore/build requires access to NuGet.

Use the project file when building or running. Do **not** run `dotnet build Program.cs`: that asks the C# tooling to compile the source file as a standalone project and can produce errors such as `Microsoft.AspNetCore` or `Microsoft.Data` not found. Those framework and package references come from `AppServiceSqlSample.csproj`.

```powershell
# Open your copied AppServiceSqlSample folder in VS Code first.
# Run these commands from its integrated PowerShell terminal.
dotnet restore .\AppServiceSqlSample.csproj
dotnet build .\AppServiceSqlSample.csproj
```

If these commands succeed, the code compiles. The database connection still needs the correct training server, network access, and database permissions.

### Build from VS Code without selecting the wrong file

This folder includes a default project build task in [.vscode/tasks.json](./.vscode/tasks.json).

1. Open **File > Open Folder** and select `AppServiceSqlSample` as the workspace folder.
2. Press **Ctrl+Shift+B** to run **Build AppServiceSqlSample project**. Alternatively, use **Tasks: Run Task** from the Command Palette and select that task.
3. The build output must reference `AppServiceSqlSample.csproj`, not `Program.cs` as the build target. Do not use a file-level Run/Debug action on `Program.cs`.
4. To run the app, use `dotnet run --project .\AppServiceSqlSample.csproj` in the same PowerShell window where you set `ConnectionStrings__TrainingDatabase`.

If you keep the larger training folder open instead, use the explicit terminal commands above. The nested project's task applies when `AppServiceSqlSample` itself is opened as the workspace folder.

## 5. Prepare the database and identity

An authorized instructor or administrator prepares Azure SQL and App Service before learners run the deployed sample:

1. Enable the App Service's **system-assigned managed identity**.
2. In the target training database, run the provided [sql/setup.sql](./sql/setup.sql) as an authorized Microsoft Entra SQL administrator, after replacing `<APP_SERVICE_NAME>` with the exact identity name.
3. The script creates the sample table and row, creates a database user for the App Service identity, and grants only `SELECT` on `dbo.TrainingItems`.
4. Confirm the App Service can reach the SQL endpoint through the approved network configuration. Do not use a broad public firewall rule as a shortcut.
5. Configure the App Service setting in the next section.

Do not run the setup script against production or a shared database. The script is intended for the designated training database only.

## 6. Run locally (optional)

Local execution is optional. It uses the signed-in developer's Entra identity, **not** the App Service managed identity. The instructor must separately authorize your Entra account in the training database and provide network access before this step can work.

From the project folder, substitute the instructor-provided server and database names:

```powershell
az login
$env:ConnectionStrings__TrainingDatabase = "Server=tcp:<server-name>.database.windows.net,1433;Initial Catalog=<database-name>;Authentication=Active Directory Default;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
dotnet run
```

Keep this terminal open. Open the local address printed by `dotnet run` (usually `http://localhost:5xxx`) and test:

- `/health` should return `{"status":"ok"}`. This checks the web process only; it does not test SQL.
- `/api/items` should return the seeded `Welcome to the lab` row. This confirms local authentication, network access, and SQL authorization all work.

The connection string contains endpoint and database names only; do not add a SQL username or password. The environment variable exists only in that PowerShell process. Close the terminal when finished.

If you do not have approved local database access, do not change firewall or permission settings. Build the sample and have the instructor demonstrate the deployed application instead.

## 7. Configure the training App Service

An instructor or authorized deployer should configure only the designated training App Service:

1. Set `ConnectionStrings__TrainingDatabase` to the same endpoint/database shape shown above, using the correct training resource names. App Service maps the double underscore to the `ConnectionStrings:TrainingDatabase` configuration key.
2. Confirm the system-assigned identity is enabled and the SQL database user/grant matches that identity.
3. Publish/deploy using the instructor-approved method. Do not deploy over a shared or production app.
4. Restart the app after an approved configuration change if requested.
5. Browse to `/health`, then `/api/items`. The first checks that the site responds; the second proves the SQL path works.

The configuration setting must not contain SQL credentials. Do not paste the full setting into chat, screenshots, or source files.

## 8. Common setup problems

| Symptom | Check |
|---|---|
| “No installed .NET SDK was found” | Install the .NET 10 SDK system-wide, start a new terminal, and confirm `dotnet --list-sdks` includes version `10.0.xxx`. |
| Installer reports success, but `dotnet` is not recognized | Open a new terminal (or restart VS Code/Windows), then run `dotnet --version`. If needed, use the .NET Install Tool's **Find .NET** command to check the system installation path. |
| VS Code cannot load `Program.cs` | Open the folder containing the `.csproj`, install the SDK, then reload the VS Code window. |
| Build reports `Microsoft.AspNetCore` or `Microsoft.Data` is missing and output shows `Program.cs.csproj` | VS Code built the `.cs` file as a standalone project. Open the `AppServiceSqlSample` folder and run `dotnet build .\AppServiceSqlSample.csproj` in the terminal. |
| `dotnet` is not recognized | Restart VS Code/terminal after SDK installation; check that the SDK installer completed and added `dotnet` to `PATH`. |
| NuGet restore fails | Check internet/proxy access to NuGet and retry `dotnet restore .\AppServiceSqlSample.csproj`. |
| `/health` works but `/api/items` returns 500 | The web app runs, but SQL did not. Ask the instructor to check the server/database names, Entra sign-in or App Service identity, database user/grant, network path, and server-side logs. |
| Local sign-in works but SQL denies access | The local developer identity needs its own database user and least-privilege grant; the App Service identity's grant does not authorize your local account. |
| App Service cannot access SQL | Verify that its system-assigned identity is enabled, the database user refers to the current identity, the grant is present in the correct database, and the approved network path is available. |
| C# Dev Kit asks you to sign in | That sign-in is optional for this sample. It does not resolve a missing SDK or SQL authorization problem. |

## Reference links

- [.NET installation on Windows](https://learn.microsoft.com/dotnet/core/install/windows)
- [.NET SDK and CLI telemetry](https://learn.microsoft.com/dotnet/core/tools/telemetry)
- [Install Azure CLI on Windows](https://learn.microsoft.com/cli/azure/install-azure-cli-windows)
- [Securely connect .NET apps to Azure SQL Database using Managed Identity](https://learn.microsoft.com/azure/app-service/tutorial-connect-msi-sql-database)
- [Microsoft Entra authentication with Microsoft.Data.SqlClient](https://learn.microsoft.com/sql/connect/ado-net/sql/azure-active-directory-authentication)
- [Microsoft.Data.SqlClient package](https://www.nuget.org/packages/Microsoft.Data.SqlClient)
- [Microsoft.Data.SqlClient.Extensions.Azure package](https://www.nuget.org/packages/Microsoft.Data.SqlClient.Extensions.Azure)

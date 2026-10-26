# LBS Document Tracker

A fictional, local-only starter for submitting and tracking small text documents through an in-memory background-processing demo.

## Local-only boundary

This is not an Azure-integrated or production application. It runs only when `ASPNETCORE_ENVIRONMENT=Development`; startup deliberately fails in every other environment. It has no authentication, so the development-only access bypass is prominently shown in the UI. Do not expose it to a network or upload real, personal, confidential, or sensitive documents.

Documents are held in memory only. Upload bytes are limited to 32 KiB, decoded as strict UTF-8, and passed to a bounded in-memory queue; neither uploaded content nor the fictional reference is written to logs. The worker validates fictional application headers and reports Completed or Needs review. All records and queued data disappear when the process stops. The application contains no Azure clients, cloud credentials, or claims of Azure integration. Before any hosting, replace the local store/queue with approved persistent storage, implement and enforce tenant-only authentication, and complete the required review and deployment preparation. This starter must not be hosted as-is.

## Run locally

Requires the .NET 10 SDK. From this directory in PowerShell:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:DOTNET_ENVIRONMENT = 'Development'
$env:PORT = '5193'
dotnet run --urls "http://localhost:$env:PORT"
```

Open `http://localhost:5193`. Submit a fictional reference such as `LBS-DEMO-1234` and a UTF-8 `.txt` file no larger than 32 KiB. The dashboard and detail page show its queued/processing/completed status. ASP.NET Core's configured console logger records processing events, never document contents.

## Attendee package

This project is now included in `Labs\LbsDocumentTracker` in the attendee pack.
The Word guide `LBS_Document_Tracker_Setup_And_Testing_V_3.docx` is at the
pack root. Copy this project to your own computer before running it.
It remains a local-only training sample, not a production-ready application.

## Real-world training scenario

The first scenario is a fictional building-society application document check.
The local implementation accepts an upload, queues a job, validates its training
header and displays the outcome. This is format validation, not credit scoring,
identity verification or application approval.

Use the four files in [SampleData](SampleData), with the form references and
expected statuses in [cases.json](SampleData/cases.json):

| File | Form reference | Expected outcome |
|---|---|---|
| 01-valid-application.txt | LBS-DEMO-1001 | Completed |
| 02-missing-document-type.txt | LBS-DEMO-1002 | Needs review |
| 03-reference-mismatch.txt | LBS-DEMO-1003 | Needs review |
| 04-unsupported-document-type.txt | LBS-DEMO-1004 | Needs review |

The header contains `FictionalData: true`, `Reference`, `Applicant` and
`DocumentType`, followed by a blank line and non-empty body. Supported types are
`ApplicationSummary` and `SupportingStatement`. Missing, duplicate or unsupported
fields produce Needs review. Upload format errors (extension, size, UTF-8 or
control characters) are rejected before queueing. The fictional marker is a
training declaration, not a detector for real personal data. Never submit real data.

Open [Architecture.svg](Architecture.svg) for the diagram. Solid arrows show the
implemented local path; dashed arrows show the Azure target, not working integrations.

### Existing Azure resources: read-only inventory, 26 October 2026

Resource group `Training` contains:

- SQL `firstsqlserver2026/firstdb` in Central US; public network access disabled.
  An approved network route and SQL data permissions are prerequisites.
- Function App `fristAzureFunctionApp`, UK South, .NET 8 isolated. No managed
  identity was enabled at inspection. Do not deploy over existing code without
  confirming its owner and obtaining approval. The local website targets .NET 10;
  the future Function project must match an approved host/runtime.
- Service Bus `fristAzureServiceBus`, Basic tier, UK South; no queues existed
  at inspection. Basic supports queues, not topics.
- Storage `stordemo2026` in East US and `traininga8a2` in UK South.
  Confirm which account is Function host storage before choosing document storage.
- Application Insights `fristAzureFunctionApp`, linked to an existing workspace
  in `DefaultResourceGroup-SUK`.
- App Service plan `ASP-Training-b795`, but no separate website/web app.

This change does not create queues, containers, SQL objects, identities, roles,
settings or deployments. Resource existence does not establish application access.
Cross-region calls can add latency and transfer costs.

### Next Azure milestone

Replace memory with SQL/private Blob Storage, durable dispatch and Service Bus.
Add an idempotent Function worker, tenant sign-in, per-user authorisation,
managed identities, scoped data access and correlated telemetry. Durable dispatch
must recover after storing an upload but failing to enqueue its job. Business
validation outcomes belong in application status; dead-letter queues are for
delivery/processing problems, not every invalid application.

### Run scenario checks

From this project directory, with the .NET 10 SDK:

```powershell
dotnet run --project .\Checks\LbsDocumentTracker.Checks.csproj -- "$PWD\SampleData"
```

This package-free executable checks all four cases through the background worker,
validation edge cases and shutdown. It throws and exits unsuccessfully on failure.
It does not connect to Azure. Checks are excluded from the website's source build
and published content.

`PORT` is documented as the local port setting; pass it through `--urls` as shown because the framework's standard ASP.NET Core hosting configuration uses URLs rather than a built-in `PORT` variable.

## Framework-only smoke-check (not run during starter generation)

With the Development server running, use a browser or `curl.exe`:

```powershell
curl.exe -i http://localhost:5193/healthz
curl.exe -i http://localhost:5193/readyz
```

Expected: both return HTTP 200 with JSON (`{"status":"ok"}` and `{"status":"ready","mode":"local-memory"}` respectively). `/readyz` returns HTTP 503 if the in-memory store or processing worker is unavailable. Also check that the dashboard loads, the upload page rejects a wrong extension/oversized or invalid UTF-8 file, and a valid fictitious upload appears on its status page. POST forms use Razor Pages antiforgery protection; no cross-origin API is enabled.

## Validation and remaining work

Verified on 26 October 2026: .NET build with zero warnings/errors; health,
readiness and dashboard responses; an antiforgery-protected valid upload reaching
Completed; rejection of an invalid reference, invalid UTF-8 and a file exceeding
32 KiB. The temporary smoke-check process was stopped afterward.

Scenario update verified the same day: all four sample cases passed both worker
checks and antiforgery-protected HTTP uploads; nine validator checks and worker
shutdown passed. The architecture SVG passed XML validation and was opened in a
browser. The temporary HTTP validation server was stopped. These are local checks,
not Azure integration evidence.

This is the first development milestone, not the requested finished Azure app.
Remaining work: replace memory with Azure SQL and private Blob Storage; implement
durable Service Bus publishing/processing and a Function worker; enforce tenant
sign-in and per-user access; configure telemetry; estimate cloud costs and validate
permissions/quotas; generate approved infrastructure; deploy and run end-to-end
cloud verification. No Azure resources have been created for this project.
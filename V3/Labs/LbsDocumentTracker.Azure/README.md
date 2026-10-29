# LBS Document Tracker - separate Azure training version

This sibling project does not modify or depend on the original local-only demo.
It uses Azure SQL, Azure Service Bus and .NET 10 isolated Azure Functions.
The Razor Pages web UI stays on `http://127.0.0.1:5293` in Development only.
It has no sign-in: do not host it, proxy it to a network, or use real/sensitive data.
This is a training workflow, not financial approval, identity verification or a production system.

For the detailed architecture diagram, per-component identity/permission matrix,
Portal setup, SQL firewall and trusted-services explanation, test plan and recovery
runbook, see [Architecture and operations](ARCHITECTURE-AND-OPERATIONS.md).
For inviting external attendees and granting scoped resource/data access, see
[External user access](EXTERNAL-USER-ACCESS.md).

## Existing resources discovered (read-only)

Subscription: Visual Studio Enterprise Subscription (`fbd069b7-a20d-4619-9fbe-a7d8b2ab3367`).
Resource group: `Training`.

| Component | Existing resource | Region |
|---|---|---|
| SQL server/database | `firstsqlserver2026` / `firstdb` | Central US |
| Service Bus namespace | `fristAzureServiceBus` | UK South |
| Function App | `fristAzureFunctionApp` | UK South |
| Functions hosting plan | `ASP-Training-b795` (Flex Consumption) | UK South |

No Service Bus queues were returned during initial discovery.
After explicit approval on 2026-10-29, `document-processing` was created and verified
Active with maximum delivery count 10 and a one-minute lock duration.
The `lbs_training` schema and Documents/Outbox tables were also created in `firstdb`.
All eleven columns, five expected indexes, foreign key and status check were verified;
both tables were empty immediately after schema setup. No application data was seeded;
later live tests created persistent fictional records.
Following separate owner approval, the Functions package was deployed to the existing
app, replacing `ApplicationStatus` with `DispatchTrainingDocuments` and
`ProcessTrainingDocument`. Both triggers were verified as indexed.
The previous deployment packages were backed up locally before replacement; the temporary
container-scoped backup permission was removed afterward.
**Future publishing replaces the functions in this app. Obtain owner approval and
retain a recovery package before publishing.**
Read-only inspection confirmed the app is Running with a `dotnet-isolated` 10.0 runtime
and a system-assigned identity. Existing host/deployment storage settings were preserved.
The identity received Sender/Receiver roles on only `document-processing`, and SELECT/UPDATE
on only the two training tables via [Database/function-access.sql](Database/function-access.sql).
No SQL password or Service Bus SAS secret is used by this application.
Resource existence does not establish data access, network access or deployment permission.
Cross-region SQL calls can add latency and transfer costs.

## Workflow

1. The local web app accepts a strict UTF-8 `.txt` upload, at most 32 KiB.
2. One SQL transaction stores a `Queued` document and an outbox entry.
3. `DispatchTrainingDocuments` runs every 15 seconds, reads up to 20 pending entries,
   sends document identifiers to Service Bus, then marks those entries dispatched.
4. `ProcessTrainingDocument` loads the text from SQL, validates the fictional header,
   and conditionally finalizes the record as `Completed` or `Needs review`.
5. The processor clears the uploaded text in the same SQL update. Metadata and results persist.
   SQL backups can retain earlier content; clearing a column is not secure erasure.

There is deliberately no in-memory fallback or simulated cloud-success mode.
Outbox entries survive web restarts and broker outages. A crash after sending but before
marking dispatched can send a duplicate. Stable message IDs and a conditional SQL completion
make retries safe; broker duplicate detection is not required.
Multiple dispatchers can also send duplicates, which the same completion guard handles.
There is no intermediate Processing state: a single final SQL update avoids stuck claims.

Malformed queue messages are dead-lettered explicitly. Business validation failures become
Needs review and are completed normally. Technical failures propagate for host retries;
after the queue's configured maximum delivery count they go to its dead-letter queue.
Such records remain Queued until an operator fixes and replays the job.
Do not delete SQL rows while messages may still reference them.
Outbox and metadata retention, dead-letter alerting and cleanup need an operational policy.

## Build and offline checks

Requires .NET 10 SDK. Open this folder in a separate VS Code window.

```powershell
dotnet build .\LbsDocumentTracker.Azure.slnx
dotnet run --project .\Checks\LbsDocumentTracker.Azure.Checks.csproj -- "$PWD\SampleData"
```

Offline checks use test doubles, not cloud resources. They cover all ten sample outcomes,
idempotent redelivery, broker failure, failure after send/before acknowledgement, message
validation, upload-page validation, strict UTF-8, and the exact 32 KiB upload limit.
They do not prove SQL transactions, managed identities, Function bindings or Azure delivery work.
The web upload page was also smoke-tested locally for HTTP 200, antiforgery protection
and CSS delivery, and Production startup was verified to fail. The Functions project was also published for Linux and deployed to the existing Flex app.

## Azure setup status and remaining approvals

No setup command below should be run without approval for the specific resource changes.
Do not create a replacement database, namespace, Function App or hosting plan.

1. **Completed after approval:** [Database/schema.sql](Database/schema.sql) was applied to
   **firstdb**, not master. Only `lbs_training` and its tables, constraints and indexes
   were created. Existing application tables were not changed.
2. **Completed after approval:** `document-processing` was created in the existing
   namespace with `maxDeliveryCount=10` and a lock duration of one minute.
3. **Completed:** confirmed the Function App's .NET 10 isolated runtime and Flex deployment configuration,
   host-storage configuration, and identity. Reuse its approved host storage; do not
   replace AzureWebJobsStorage or deployment-storage settings blindly.
4. **Configured:** Microsoft Entra authentication (no SQL passwords or Service Bus SAS secrets).
   Grant the Function identity Azure Service Bus Data Sender and Data Receiver on the queue.
   Grant the local developer those roles only if running the Functions host locally.
   The web UI itself needs no Service Bus access.
5. **Configured for the Function identity:** an Entra SQL administrator resolved and created
   the contained user using `FROM EXTERNAL PROVIDER`, then granted table permissions.
   Do not guess its SQL SID from the managed identity object ID.
   For a separate, least-privileged developer identity, grant only:
   - Web/developer identity: SELECT and INSERT on Documents; INSERT on Outbox.
   - Function identity: SELECT and UPDATE on Documents and Outbox.
   Neither runtime identity needs schema-owner, db_owner, CREATE TABLE or DELETE privileges.
   SQL permissions are separate from Azure subscription/resource-group roles.
   **Training limitation:** the current CLI login is already the SQL Entra administrator.
   Its existing elevated access was not changed. Do not use this identity for a hosted app.
6. **Training networking:** after approval, 31 exact-address firewall rules were added for
   reported Function outbound addresses, plus separately approved observed addresses
   `4.250.220.175`, `4.158.142.191`, `172.167.222.36`, `4.250.59.122` and `172.167.65.37`.
   Flex workers used addresses absent from the reported list. These rules are not stable
   production egress and may stop working as workers change.
   After subsequent failures, the owner reported enabling SQL's **Allow Azure services
   and resources to access this server** exception in Portal, choosing no VNet.
   Four later sample-01 uploads were shown as Completed. This broad exception allows
   Azure-origin connection attempts from other customers' subscriptions too; SQL
   authentication and table permissions remain required. It does not cover the
   laptop's normal internet connection. See the architecture guide for the trade-off.
7. **Configured:** the following non-secret Function settings:

   | Setting | Value |
   |---|---|
   | `Sql__Server` | `firstsqlserver2026.database.windows.net` |
   | `Sql__Database` | `firstdb` |
   | `ServiceBusConnection__fullyQualifiedNamespace` | `fristazureservicebus.servicebus.windows.net` |
   | `ServiceBusConnection__credential` | `managedidentity` (Azure host only) |
   | `QueueName` | `document-processing` |

   Flex runtime selection remains in `functionAppConfig.runtime`, not a new
   FUNCTIONS_WORKER_RUNTIME app setting.
   Hosted Functions explicitly use the system-assigned `ManagedIdentityCredential`;
   the Functions trigger uses its identity-based ServiceBusConnection settings.
   The local UI explicitly uses `AzureCliCredential`, avoiding unexpected credential
   selection and slow developer-tool probing during SQL connection establishment.
8. Use [Deploy-Functions.ps1](Deploy-Functions.ps1) to build a Linux-ready package.
   It includes hidden extension files and uses portable ZIP entry separators.
   The first Windows-generated archive failed Linux validation; the script corrects
   that issue and checks the package before deployment.

   ```powershell
   .\Deploy-Functions.ps1
   # Only after approval to replace the current hosted functions:
   .\Deploy-Functions.ps1 -Deploy
   ```

   Azure CLI routes this Flex deployment through its package deployment endpoint.
   Do not set WEBSITE_RUN_FROM_PACKAGE or publish the local Web project to Azure.

## Run after approved setup

Authenticate locally with `az login` using the intended tenant/account.
The local UI uses AzureCliCredential with that CLI login. It does not silently select
an unrelated Visual Studio or environment identity.

From this directory:

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:DOTNET_ENVIRONMENT = 'Development'
dotnet run --project .\Web\LbsDocumentTracker.Azure.Web.csproj
```

Open `http://127.0.0.1:5293`. The fixed loopback port avoids the original demo's 5193.
If 5293 is occupied, stop only the known process using it; do not expose the app externally.
Uploads will not complete until the Functions host is running and authorized.

### Optional local Functions host

Requires Azure Functions Core Tools v4 with .NET 10 support and an approved local
Azurite instance for timer-trigger host storage. These tools are not installed by this project.
Copy the settings example locally:

```powershell
Copy-Item .\Functions\local.settings.example.json .\Functions\local.settings.json
Set-Location .\Functions
func start
```

Start Azurite separately first. The example uses its development-storage connection,
but SQL and Service Bus are REAL Azure services, not emulated.
Do not set the Azure-only `managedidentity` credential override when using a local CLI login.
Do not run local Functions against a shared queue while an uncoordinated hosted processor
is consuming it. Real Azure use may incur charges.

## Live acceptance checks after approval

With the local web UI running, the repeatable live test submits ten fictional uploads
over HTTP with real antiforgery tokens, then waits up to three minutes for the hosted
Functions to produce the expected results:

```powershell
.\Test-Live.ps1 -SubmitTrainingData
```

This writes ten persistent records and uses REAL Azure services on each run.
It does not emulate SQL or Service Bus.

### Verified live results (2026-10-29)

**Latest owner-observed smoke test: successful after the SQL network change.**
Screenshots around 22:05 UTC show four sample-01 uploads as Completed, including
the previously Queued 22:01:42 upload. The owner reported enabling SQL's Allow Azure
services exception. This is not a new ten-case automated run, and current dead-letter
counts/recovery were not verified. See [the detailed evidence and runbook](ARCHITECTURE-AND-OPERATIONS.md).

**Earlier automated retest: FAILED (after the initial successful run below).** Offline checks still
pass, and ten new HTTP submissions were accepted, but the live test timed out.
Processing workers subsequently used `172.167.55.244`, `172.167.55.175` and
`172.167.29.68`, which SQL denied. The queue reported ten dead-letter messages.
No additional firewall or permission changes were made during this retest.
The later successful screenshots supersede that failure for those four fresh jobs,
but do not establish recovery of the older failed jobs.
Verify access and obtain approval before replaying the affected jobs;
do not repeatedly submit the samples to work around this failure.

Initial successful run (historical):

- All ten fresh HTTP uploads reached their expected statuses through the hosted Functions.
- The original ten uploads survived the initial SQL/network outage and were recovered
  from dead-letter storage after repairing access. Recovery replay was separately approved.
- SQL contained 20 finalized records: 6 Completed and 14 Needs review, with all 20
  document-content columns cleared, all 20 outbox entries dispatched, and no document
  lacking an outbox record.
- Service Bus reported zero active and zero dead-letter messages after recovery/testing.
- Temporary backup and replay roles were removed. The Function retains its approved queue roles.
- After stopping and restarting the local web UI, the completed document details and
  SQL-backed dashboard were still available. Temporary test web processes were then stopped.
- Application Insights recorded 20 successful document-processing events. SCM basic
  publishing authentication remained disabled.

The initial dead-lettering was caused by denied SQL access, not business validation.
Never replay a queue blindly: inspect the error, repair access, select only intended
document identifiers, send the replacement job, and complete its dead-letter message
only after broker acceptance. This application does not automatically replay dead letters.

### Additional operational checks

1. Upload each sample with the matching reference from [SampleData/cases.json](SampleData/cases.json).
   Samples 01, 05 and 06 should be Completed; the other seven should be Needs review.
2. Stop/restart only the new local web UI: previously submitted records must remain.
3. Pause the new dispatcher in an isolated test window, submit a sample, confirm it stays
   Queued, resume dispatch, and confirm eventual completion. Do not pause shared functions
   without owner approval.
4. Replay the same identifier and verify its completion timestamp/result do not change.
5. Check Functions logs and queue dead-letter counts. Missing/invalid identifiers are
   technical errors, not business review outcomes. Fix the root cause before replay.
6. Check that completed documents have DocumentText NULL and retain their status metadata.

Sequential duplicate delivery is covered by offline tests; live concurrency, sustained
load, stable networking, and production disaster recovery have not been certified.
Before real hosting,
add tenant sign-in and per-user authorization, security/compliance review, retention and
audit policies, alerts, operational retry tools and approved network isolation.

## Stopping and cleanup

Press Ctrl+C to stop the local UI. The hosted Functions and Azure resources remain
deployed and may incur charges. This is a shared existing resource group: do not delete
Training or its SQL database, Service Bus namespace, storage accounts or hosting plan.
Removing this version requires a reviewed rollback of its Function package, settings,
queue-specific roles, named LbsFunction firewall rules and lbs_training objects.
Preserve pending jobs and obtain explicit approval before removing any cloud data or resources.

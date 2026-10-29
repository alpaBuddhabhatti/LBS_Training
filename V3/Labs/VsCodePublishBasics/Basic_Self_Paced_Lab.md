# Function and local dummy tracker exercise

The full student instructions are in the
[Function and dummy tracker handout](../../Day%201/Basic_Lab_VS_Code_Azure_Function_And_Dummy_Tracker.docx).

- Orientation: Parts 1-2 only; defer Function tools until the Function exercise.
- Standalone Function: Parts 3-5, then relevant evidence/cleanup in Part 8.
- Optional local UI: Parts 6-8, only when assigned.
- Website publishing is an optional extension requiring a separate approved
  App Service web app. A Function host is not a website deployment target.

Follow [Day 1 order](../../Day%201/00_DAY_1_ORDER.docx). These independent
samples do not connect SQL, Blob Storage or Service Bus.


---

## Day 1 tested walkthrough - student support

Updated 28 October 2026.

### 4. Function: build, local invocation and cloud invocation

![function local cloud](<../../Day 1/Diagrams/function-local-cloud.png>)

Open Labs\VsCodePublishBasics\HelloAzureFunction. Confirm the .csproj, ApplicationStatus.cs and host.json are present. Use the approved .NET 10 SDK, .NET-10-compatible Core Tools v4 and Azurite. If func is not found, approved tools must be installed/enabled in this terminal. The instructor's portable Enable-LocalTrainingTools.ps1 helper is not a universal attendee prerequisite; use it only if supplied and approved.

```powershell
dotnet --list-sdks
func --version
if (-not (Test-Path '.\local.settings.json')) {
    Copy-Item '.\local.settings.example.json' '.\local.settings.json'
}
dotnet build
```

Do not overwrite existing local settings. The example uses local Azurite storage, not Azure account keys. Never publish local.settings.json. A successful build proves compilation only, not a running HTTP endpoint.

Press Ctrl+Shift+P > Azurite: Start. In terminal 1 run the host; keep it running until local tests finish. Open terminal 2 for the request.

```powershell
# Terminal 1
func start --port 7071

# Terminal 2
curl.exe --noproxy "*" --fail --max-time 20 http://127.0.0.1:7071/api/ApplicationStatus
```

Wait for the host to list ApplicationStatus: [GET] before testing. The explicit IPv4 loopback command helped during rehearsal; its proxy bypass is limited to this local request, not corporate networking globally. If the port is occupied, do not kill someone else's process. Use an approved alternative port and update both the host and request.

```json
{"reference":"LBS-DEMO-1001","status":"Received","fictionalData":true,"message":"Hello from the VS Code publishing exercise."}
```

### Why press Ctrl+C?

Click inside terminal 1 running func start and press Ctrl+C after local testing. Wait for Host shutdown completed and the PowerShell prompt. This stops only your local server, frees port 7071 and local resources, and helps avoid build file locks. It does not delete code, undo the build or stop the Azure Function App. localhost stops responding; the cloud URL remains independent. Restart with Azurite running and func start --port 7071. Logs/KQL does not require the local host.

### Shared cloud Function: test-only route

Do not redeploy the shared app to repeat this checkpoint. Publishing replaces deployed content and requires a separately approved compatible .NET 10 isolated target. Record publishing as Not run / test-only if not authorised; a build or cloud invocation does not prove VS Code publishing.

In the approved Function App select Functions > ApplicationStatus > Get Function URL. Choose default (Function key), not default (Host key) or _master (Host key). Click the copy icon and paste the full URL directly into your browser address bar, never chat. Expect the same four-field JSON. The portal Code + Test > Test/Run route can separately show 200 OK; a browser JSON body alone does not display its exact HTTP status.

In a separate browser tab remove ?code= and everything after it. Expected: HTTP 401 Unauthorized. The browser's error page is a passing negative access test, not a broken app. Local Core Tools normally does not enforce keys, so a local keyless response does not replace this cloud check.

### If a key is accidentally shared

Stop using it. Coordinate with the authorised owner to regenerate the exact exposed key: Function Keys for a function key, App keys for a host key. Update affected callers and obtain a fresh private URL. Confirm old-key invalidation through the approved private process; do not assume renewing a different key fixes it. Never paste the replacement into evidence. Function keys are shared access secrets, not individual user sign-in.

### 6. Finish safely and record your own evidence

Record Passed, Blocked or Not run separately for each item: resource discovery; SQL portal connect/create/read; SQL MSSQL read/alter/update; portal recheck; Blob transfer and hashes in both directions; local Function build/invocation; publishing approval/result or test-only; cloud keyed response and keyless rejection; portal/terminal KQL match.

Capture the target, time, command and expected/actual non-secret output. An instructor screenshot is an example, not your pass. SQL rows and schema do not prove which tool wrote them. A green deployment is not an invocation. A reported key rotation is not independent proof the exposed key was invalidated.

Record your own results for each assigned exercise. Instructor examples and earlier rehearsals do not establish attendee access or a successful run on your machine.

Stop your own local Function with Ctrl+C. Close Azurite only if you started it and no other work needs it. Disconnect the SQL client and sign out on shared machines. Local shutdown does not stop cloud billing. Do not delete shared resources or restore network/role changes without owner approval; temporary permission cleanup requires checking dependencies and verifying the narrower approved access first.

Day 1 is ready for sign-off only when required checks have evidence or approved exceptions. Record your own results, review access cleanup with the owner, and leave the optional UI Not run if not assigned. Continue to Day 2 after your instructor reviews the checkpoints.

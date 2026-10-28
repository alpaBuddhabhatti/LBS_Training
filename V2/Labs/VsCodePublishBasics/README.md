# Standalone Function and local dummy website

The [illustrated Day 1 quick reference](<../../Day 1/Day_1_Illustrated_Quick_Reference.pdf>)
includes the local/cloud diagram, why to press Ctrl+C, key-safe testing and
the course-wide completion checklist. Use it alongside the full Word handouts.

Follow the [full student handout](../../Day%201/Basic_Lab_VS_Code_Azure_Function_And_Dummy_Tracker.docx)
and [Day 1 order](../../Day%201/00_DAY_1_ORDER.docx).

## Separate VS Code projects

For the Function code, read [Program.cs](HelloAzureFunction/Program.cs), then
[ApplicationStatus.cs](HelloAzureFunction/ApplicationStatus.cs).
For the dummy website, read [Program.cs](DummyDocumentTracker/Program.cs),
[index.html](DummyDocumentTracker/wwwroot/index.html), then
[site.js](DummyDocumentTracker/wwwroot/site.js). Inline comments explain the
local/cloud boundary, static data and safe rendering. Neither example connects
the services together.

- [HelloAzureFunction](HelloAzureFunction): a fixed fictional HTTP response.
  Open this folder for the Function exercise. The guide covers .NET 10,
  Core Tools, Azurite, local-only settings, publishing approval and keyed tests.
- [DummyDocumentTracker](DummyDocumentTracker): a local-only UI showing three
  fixed fictional records. It does not call the Function or any Azure service.

For the optional website exercise, open `DummyDocumentTracker` and run:

```powershell
dotnet run --project .\DummyDocumentTracker.csproj --urls "http://localhost:5203"
```

Keep that terminal running. Open `http://localhost:5203`, check all three rows
and their detail buttons. From a second terminal in `VsCodePublishBasics`:

```powershell
.\Test-PublishBasics.ps1 -WebsiteUrl "http://localhost:5203"
```

The script checks HTTP/data responses, not browser button behaviour. Stop your
own server with Ctrl+C afterward.

## Cloud safety

Use only your assigned Azure target and obtain approval before deploying.
Publishing replaces existing content. Do not deploy the website to a Function
App or assume an existing Function plan can host it. A cloud Function test
requires a privately handled key; never put key-bearing URLs in shared evidence.

For browser testing, choose **default (Function key)** in **Get Function URL**,
not either Host key option. Copy the full URL directly into your browser, never
into chat. With the key, expect the fixed fictional JSON; without `?code=...`,
expect HTTP 401. A keyless 401 is the expected access check, not a broken app.
If a key is exposed, ask the authorised owner to regenerate the matching key
and update affected callers before testing with a fresh private URL.

## Next after the Function checkpoint

After recording the local response, keyed cloud response and keyless rejection,
stop your local Function with Ctrl+C. On the shared-app test-only route, record
publishing as **Not run**; do not redeploy just to complete the checklist.

**Why Ctrl+C?** `func start` keeps a server running on your computer. Click
inside the terminal running it and press Ctrl+C, then wait for the PowerShell
prompt to return. This stops the local server and releases port 7071 and its
resources. It does not delete your code, undo the build or stop the Azure
Function App. The localhost URL stops responding; the cloud URL is independent.
Logs/KQL does not require the local server. To restart it later, ensure Azurite
is running and run `func start --port 7071` in the Function project folder.

Continue to [Logs/KQL](../PortalAndVsCodeBasics/03_Log_Analytics_Portal_And_VS_Code.md),
Day 1 exercise 5. Run the supplied synthetic query in portal Logs and from the
VS Code terminal against the same assigned workspace. Expect three matching
rows; these are not live Function telemetry. The dummy UI remains optional.

The realistic Day 2 local scenario is a different project:
[LbsDocumentTracker](../LbsDocumentTracker). It must not be hosted as-is.

# Logs and KQL exercise

Follow the [complete Logs/KQL handout](../../Day%201/Basic_Lab_03_Log_Analytics_Portal_And_VS_Code.docx).
Use [Samples/Logs](Samples/Logs) for the supplied query and PowerShell runner.

The main exercise returns three synthetic rows. Run it in the portal and
the VS Code terminal against the same approved workspace. These rows do not
prove that application telemetry has been collected. Query access and any
optional workspace changes require approval.

## Where to copy the Workspace ID

In Azure portal, open **Log Analytics workspaces > your assigned workspace >
Overview**. In the **Essentials** section, find **Workspace ID** and click its
copy icon. If it is not shown in that layout, check **Properties**.

Use that GUID for the supplied script's `-WorkspaceId` argument. Do not copy
Workspace Name, Subscription ID or the Azure resource ID instead. No workspace
shared key is needed. Use your own assigned workspace value in commands.

## Queries hub and KQL mode

When opening **Logs**, close the welcome panel. **Queries hub** is a catalogue
of ready-made queries grouped by category, not the query editor. Close it using
the **X at its top-right**. Optionally turn off **Always show Queries hub** first
to stop it opening automatically. This lab uses the supplied fictional query,
not the **Run** buttons on the hub's example cards.

If the **Agent** toggle is on at the top-right, turn it off. Use the mode
dropdown above the editor on the right and select **KQL mode**. KQL means
Kusto Query Language; this mode lets you write and run query text directly.
Paste the supplied query into the editor, not the Observability Agent chat,
then select **Run** on the editor toolbar.


---

## Day 1 tested walkthrough - student support

Updated 28 October 2026.

### 5. Logs/KQL: same synthetic query in two clients

![logs query](<../../Day 1/Diagrams/logs-query.png>)

In Azure portal open the assigned Log Analytics workspace, not just the Function App. It can be in a different resource group. Overview > Essentials > Workspace ID > copy icon gives the GUID needed by the runner. If it is not shown in that layout, check Properties. Do not copy Workspace Name, Subscription ID or the Azure resource ID. No shared key is needed.

Open Logs. Close Welcome to Log Analytics with its top-right X. Queries hub is a catalogue of ready-made examples, not this lab's editor. Optionally turn off Always show Queries hub, then close the hub with X. Do not use the Run buttons on its example cards for this checkpoint.

If the top-right Agent toggle is on, turn it off to leave Observability Agent chat. Select KQL mode from the dropdown above the editor on the right. KQL means Kusto Query Language. Paste into the query editor, not Agent chat, then click Run on the editor toolbar.

```kql
datatable(Reference:string, Status:string)
[
    "LBS-DEMO-1001", "Completed",
    "LBS-DEMO-1002", "Needs review",
    "LBS-DEMO-1003", "Received"
]
| order by Reference asc
```

Expect exactly three rows in Reference order: Completed, Needs review, Received. datatable makes an in-memory fictional result; it does not create/ingest a table or read Function logs. The pipe sends the result to the next operator. KQL identifiers are case-sensitive; do not paste SQL SELECT syntax here.

In the existing VS Code terminal change to Labs\PortalAndVsCodeBasics. No new window is required. Confirm CLI sign-in, then run the supplied script with the same workspace's GUID.

```powershell
.\Samples\Logs\Invoke-LabQuery.ps1 -WorkspaceId "YOUR-WORKSPACE-ID"
```

Replace the placeholder. Expect the same three Reference/Status rows and PASS Azure Monitor query: exactly three expected fictional rows. The script checks the KQL and companion request JSON agree, submits a remote Azure Monitor query and validates exact values. Keep both query copies unchanged for the first comparison. A local .kql file is just saved text until submitted. If query access is denied, ask for scoped workspace access; do not grant yourself broader roles.

Optional real telemetry is a separate exercise requiring configured collection, approved access and ingestion time. A successful synthetic query does not prove Function telemetry exists. Inspect actual table names/schema before using AppRequests or resource-scoped requests.

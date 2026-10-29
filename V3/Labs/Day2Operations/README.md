# Day 2 operations exercises

V3: these are supporting/optional operations activities. The current Day 2 core
agenda prioritises Service Bus in the morning and Azure AI services/practical
in the afternoon. Do not treat the older 75-minute operations blocks as
additional mandatory sessions.

Use the [Day 2 order](<../../Day 2/00_DAY_2_ORDER.docx>) and the
[KQL handout](<../../Day 2/03_KQL_Beginner_Hands_On.docx>).

## Choose one mode before starting

- **Workspace live:** `Queries\Workspace` uses AppRequests/AppDependencies,
  TimeGenerated, DurationMs and OperationId. Replace the Application Insights
  resource ID placeholder with the approved component's full resource ID.
- **Application Insights live:** `Queries\ApplicationInsights` uses
  requests/dependencies, timestamp, duration and operation_Id, normalised for
  comparison. Confirm the selected Application Insights component.
- **Prepared query:** `Queries\Prepared` defines fictional in-memory datatables.
  Paste one whole file into an authorised KQL editor. No ingestion is required.
  Select a time range including 28 October 2026, 10:00-10:05 UTC.
- **Offline review:** read the CSVs and predict the result if query access is
  unavailable. This is not a KQL execution or a passed live access test.

Run files 01, 02, 03 in order. Each file is self-contained; do not paste all
three files into one editor selection. Live result counts vary. The prepared
exercise has five requests, two failures and two dependencies over 1000 ms.
Use actual correlation IDs when investigating live data. Correlation is
evidence of related operations, not automatic proof of root cause.

## Cost exercise

`fictional-costs.csv` contains eight **invented GBP amounts**, not Azure prices,
a quote, a billing export or evidence of any real subscription. Compare equal
seven-day periods, group by service and identify the biggest increase. Use a
spreadsheet or a read-only CSV viewer. Do not add two different periods and
call the result a single-period bill. Costs are illustrative and omit tax.

## Incident exercise

A fictional applicant reports a slow/failed submission and the team sees a
higher bill. Establish scope/time/source, find failed requests, correlate a
slow dependency, distinguish observation from hypothesis, and propose the
next safe read-only check. Then explain the cost driver, an optimisation and
its risk. The datasets do not prove that the local tracker or any Azure app
generated these events. No fault injection or real configuration change is
required.

## Boundaries

Stored row counts can differ from traffic totals when sampling is active.
Do not capture tokens, connection strings or request/document bodies.
Budgets alert; they do not automatically stop spending. Do not delete shared
resources, widen permissions or change retention to complete an exercise.

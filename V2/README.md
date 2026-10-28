# Azure developer training - student pack V2

This is the updated attendee edition. The older repository-root materials are
retained for reference; use **this V2 pack** for the current two-day route.

Download or clone the whole repository and keep the V2 folder structure intact.
Download Word, PowerPoint and PDF files before opening them; GitHub's file viewer
is not a substitute for Word/PowerPoint and may not follow local document links.

Both days run **09:45-17:30**, including lunch and breaks.
See the [two-day timetable](00_Two_Day_Timetable.docx).

## Start here

- [Course introduction](00_START_HERE.docx)
- [Day 1 order](<Day 1/00_DAY_1_ORDER.docx>)
- [Day 1 illustrated quick reference](<Day 1/Day_1_Illustrated_Quick_Reference.pdf>)
- [Day 1 student slides](<Day 1/LBS_Azure_Day_1_Attendee.pptx>)
  / [PDF](<Day 1/LBS_Azure_Day_1_Attendee.pdf>)
- [Day 2 order](<Day 2/00_DAY_2_ORDER.docx>)
- [Day 2 student slides](<Day 2/LBS_Azure_Day_2_Attendee.pptx>)
  / [PDF](<Day 2/LBS_Azure_Day_2_Attendee.pdf>)

## Supplied solutions

- [SQL, Blob and Logs/KQL](Labs/PortalAndVsCodeBasics/00_START_HERE.md)
- [Standalone Function and optional dummy website](Labs/VsCodePublishBasics/README.md)
- [App Service / SQL sample](Labs/AppServiceSqlSample/README.md)
- [Local document-processing tracker](Labs/LbsDocumentTracker/README.md)
- [Day 2 operations: fictional cost/telemetry data and schema-specific KQL](Labs/Day2Operations/README.md)

## Day 2 priorities

Cost Management is a mandatory 75-minute investigation, followed by protected
75-minute Application Insights and 75-minute KQL blocks. These use a realistic
failed/slow-submission scenario, then a short standalone queue exercise, the
local tracker and a team incident/cost handover.

- [Mandatory cost lab](<Day 2/06_Cost_Management_Portal_Lab.docx>)
- [Application Insights lab](<Day 2/02_Application_Insights_Portal_Lab.docx>)
- [Three-level KQL lab](<Day 2/03_KQL_Beginner_Hands_On.docx>)
- [Team incident and cost exercise](<Day 2/04_Incident_And_Cost_Team_Exercise.docx>)

Each revised core lab includes prerequisites, permissions, duration, created
resources, cost considerations, navigation, expected results, recovery, checks
and cleanup. Budget **design** is mandatory; real creation needs approval.
CI/CD and IaC remain awareness-only unless a separate tested runbook is supplied.

Use the query directory matching the selected schema: workspace
`AppRequests/AppDependencies`, or Application Insights `requests/dependencies`.
Prepared queries contain fictional in-memory data, and offline CSV review is
not KQL execution. Label live, prepared-query and offline evidence separately.
Expected fixture arithmetic and result shapes have been checked locally; that
does not establish Azure query execution, live collection or attendee access.

The source includes beginner-friendly comments and explanations. The labs are
separate component exercises unless a guide explicitly says otherwise. The
local tracker is a Development-only, in-memory sample, not a production-ready
or Azure-integrated application.

## Safety and completion

- Use fictional data and instructor-assigned targets. Example resource names
  and table suffixes are not permission to use another person's resources.
- Confirm identity, subscription, network path and data permissions separately.
- Never publish over a shared Function App without explicit replacement approval.
  The approved test-only route can leave publishing **Not run**.
- Keep credentials, tokens, Function keys, keyed URLs and real documents private.
  Copy the provided local-settings example only when your local file is absent.
- Do not commit generated output, local settings, private environment files or
  downloaded data. This pack's ignore rules exclude common local artifacts.
- Record your own **Passed / Blocked / Not run** results. Preparation-required
  demos are not claims of a completed cloud implementation.
- Cost tips describe cost drivers, not fixed quotations or spending caps.
  Budgets alert; stopping a local process does not stop cloud billing.
- Change permissions, tiers, networking or shared resources only with owner
  approval. Delete only your authorised temporary data.

Instructor delivery scripts, answer keys, cue sheets and internal rehearsal
records are intentionally not included.

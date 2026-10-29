# Azure developer training - student pack V3

Use **branch `v3`, folder `V3`** for the updated course. The preserved `V2`
folder is historical; do not mix its timetable or slides with this version.
Download the whole branch and keep this folder structure intact.

Both days run **09:45-17:30**. The Day 2 timetable follows the newly supplied
Service Bus / Azure AI presentation, not V2's operations-first afternoon.

## Start here

- [Course introduction](00_START_HERE.docx)
- [Two-day timetable](00_Two_Day_Timetable.docx)
- [Prerequisites](02_Pre_Course_Setup_And_Prerequisites_Guide.md)
- [Day 1 order](<Day 1/00_DAY_1_ORDER.docx>)
- [Day 1 student slides](<Day 1/LBS_Azure_Day_1_Attendee.pptx>) /
  [PDF](<Day 1/LBS_Azure_Day_1_Attendee.pdf>)
- [Day 2 order](<Day 2/00_DAY_2_ORDER.docx>)
- [Updated Day 2 student slides](<Day 2/LBS_Azure_Day_2_Attendee.pptx>) /
  [PDF](<Day 2/LBS_Azure_Day_2_Attendee.pdf>)

Download Word, PowerPoint and PDF files before opening them. GitHub previews
do not replace the desktop applications or guarantee relative-link navigation.

## Day 2 core agenda

| Start | Topic |
|---|---|
| 09:45 | Day 1 recap and architecture |
| 10:25 | Service Bus concepts and delivery semantics |
| 11:45 | Isolated queue exercise and short document-tracker demonstration |
| 13:30 | Azure AI services and service selection |
| 14:30 | Azure AI practical: approved Document Intelligence walkthrough |
| 16:00 | Real-world design discussion |
| 17:00 | Knowledge check and safe cleanup |
| 17:30 | Finish |

Lunch/break arrangements are in the timetable. Protect the afternoon AI
sessions. Student slides **1-33** form the core route; **34-42** are an optional
appendix containing SQL/identity, networking, testing, recovery, guest access
and hosting homework. The instructor edition has additional speaker notes.

Monitoring, KQL, cost and networking handouts are retained as supporting/
extension activities, not additional mandatory 75-minute blocks in this agenda.

## Demonstrations and supplied solutions

- [SQL, Blob and Logs basics](Labs/PortalAndVsCodeBasics/00_START_HERE.md)
- [Standalone Function and dummy website](Labs/VsCodePublishBasics/README.md)
- [App Service / SQL sample](Labs/AppServiceSqlSample/README.md)
- [Azure AI practical and fictional PDF](Labs/AzureAiDemo/README.md)
- [Day 2 AI step-by-step handout](<Day 2/08_AI_Document_Intelligence_Practical.docx>)
- [Original local tracker](Labs/LbsDocumentTracker/README.md): port 5193,
  in-memory store/worker; ten fictional samples.
- [Separate Azure tracker](Labs/LbsDocumentTracker.Azure/README.md): local UI
  on 5293, durable SQL/outbox, Service Bus and hosted Azure Functions.
- [Architecture, identity, SQL external user, network and testing guide](Labs/LbsDocumentTracker.Azure/ARCHITECTURE-AND-OPERATIONS.md)
- [Invite external users and grant scoped Training RG access](Labs/LbsDocumentTracker.Azure/EXTERNAL-USER-ACCESS.md)
- [Supporting operations fixtures and KQL](Labs/Day2Operations/README.md)

The Azure tracker performs deterministic text-format validation, **not AI**.
The Document Intelligence practical and illustrative AI architecture are separate
from that deployed demo. AI processing of tracker uploads is future work.

The latest owner screenshots show four sample-01 jobs Completed after enabling
SQL's broad Azure-services exception. They are not a fresh full ten-case cloud
regression, proof of an empty DLQ or production availability.

## Safety and sharing

- Confirm tenant, subscription, resource, identity, network path and data
  permissions separately. Guest invitation alone grants no Azure resource access.
- Use fictional data and assigned resources. Example names are not permission.
- Keep secrets, Function keys, tokens and real documents private.
- Both tracker UIs are local-only; do not publish them unchanged. Azure website
  hosting requires authentication, authorization and a separate workload identity.
- SQL's Allow Azure services exception includes Azure-origin attempts from
  other customers' subscriptions. It does not grant SQL data permissions.
- Queue Explorer exercises need an isolated queue; do not consume shared demo jobs.
- Do not redeploy the standalone Function over the shared tracker Function App.
  Existing `ApplicationStatus` code was replaced during the approved tracker demo.
- Record Passed / Blocked / Not run honestly. No new cloud changes or live
  acceptance tests are implied by this release.
- Changes to shared resources, IAM, tier, networking and replay require approval.
- Budgets alert, not cap spending. Stop only your own processes; never delete
  the shared Training resource group.

Archives, rehearsal records, local secrets and build outputs are not part of V3.
See [V3 release notes](RELEASE-NOTES.md) for validation and limitations.

# Azure AI classroom demonstration

This is a separate Document Intelligence practical, not an implemented
integration with the document tracker. Use the instructor-assigned existing
resource; do not create or upgrade services without approval.

## Input

[Fictional application PDF](Samples/fictional-application.pdf) is an original
one-page training sample. It has no real applicant data. It contains reference
`LBS-DEMO-2001`, applicant `Training Person`, document type `ApplicationSummary`
and a small training checklist table.

Use the PDF in Document Intelligence, not the tracker's `.txt` upload form.
Conversely, do not assume the tracker's strict `.txt` samples are valid inputs
to the selected Document Intelligence model.

## Exercise

1. Confirm the assigned tenant, resource, region, network path, permissions,
   pricing and supported Studio experience with the instructor.
2. Open that approved Document Intelligence resource in the supported Studio.
3. Use **Read** (`prebuilt-read`) for text extraction, or **Layout**
   (`prebuilt-layout`) for document structure/table exploration.
4. Upload the supplied fictional PDF and run analysis.
5. Locate the reference and applicant in the extracted text. For Layout, inspect
   the detected checklist structure and compare it with the original PDF.
6. Record the model, document, actual extracted result, missing/incorrect values
   and what requires human review. Keep keys, tokens and endpoints with secrets
   out of screenshots.

These are expected observations, not a claim that Azure analysis has been run
for this release. Results can vary by model/version. Read does not automatically
assign business meaning to every label; key-value/semantic fields require a
model and features that explicitly support them.

No automatic identity verification, affordability check or financial approval
is performed. Analysis can incur charges. Reuse an approved resource and do
not delete it as cleanup.

## Authentication

Management visibility is not data-plane access. An operator must prepare the
appropriate supported data role and network access for the selected Studio/API
scenario. Prefer Entra authentication where supported; if a scenario requires a
resource key, handle it privately through approved processes and never commit it.
Do not grant Owner simply to make the walkthrough work.

## Follow-up architecture

Adding AI to the tracker would require approved storage/input-format decisions,
an authenticated AI client, scoped permissions, resilient calls, result
persistence, human review and additional tests. It is future work, not a switch
enabled by sending an identifier through Service Bus.

References:

- [Document Intelligence Read](https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/prebuilt/read)
- [Document Intelligence Layout](https://learn.microsoft.com/en-us/azure/ai-services/document-intelligence/prebuilt/layout)

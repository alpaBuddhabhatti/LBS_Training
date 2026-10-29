# Student SQL exercise - portal, VS Code and optional SSMS

Consolidated 29 October 2026. Fictional data only.

Word edition: [student SQL handout](<../../Day 1/Basic_Lab_01_Azure_SQL_Portal_And_VS_Code.docx>).

## Before you start - choose your route

Allow 60-75 minutes including explanation and verification. Use fictional data only. You will connect, read rows and compare the same database through portal and VS Code. SSMS is an optional additional client. No database solution or SQL Database Project is required.

Required: an assigned Azure SQL hostname/database, approved authentication, network route and SELECT permission. For the VS Code route, install VS Code and Microsoft SQL Server (mssql). Local SQL Server/Express, Visual Studio, .NET SDK and the App Service sample are not required for this exercise.

READ-ONLY ROUTE: use the instructor-approved existing table. Do not execute CREATE, INSERT, ALTER, UPDATE or DELETE. The table dbo.VsCodeItems_instructor20261028 is the current retained instructor example, ONLY when the instructor assigns it. Other deliveries may assign a different table. Existing contents can change; record what you actually observe.

WRITE ROUTE: only with explicit approval, use your own disposable table and replace VsCodeItems_ab01 everywhere with your unique assigned name. ab01 is a placeholder, not a table guaranteed to exist. Never substitute the retained instructor table into a write script.

Resources created: none on the read-only route; one small table on the approved write route. Reuse the existing database. Its compute, storage and backups can cost money independently of your client. No new database, tier change or firewall/role change is required without owner approval.

## 1. Install VS Code first, then the correct SQL extension

1. Install Visual Studio Code from https://code.visualstudio.com/download using your approved company process, then open it.

2. Press Ctrl+Shift+X, search mssql, choose SQL Server (mssql) by Microsoft, extension ID ms-mssql.mssql, and click Install. Reload if prompted. Do not choose SQL Server Client (mssql) from another publisher. SQL Database Projects is not needed.

3. Disable and Uninstall buttons show the extension is already installed and enabled in this context. A gear icon indicates installation; check enablement if commands are absent.

![Microsoft SQL Server (mssql) extension](<../../Day 1/Diagrams/vscode-mssql-extension.png>)

## 2. Understand the connection before opening a query

The Azure SQL logical server supplies the hostname and server-level connection configuration. The database contains your tables. A firewall/private network route controls reachability; authentication identifies the caller; database permissions authorise queries. These are separate.

Confirm tenant, subscription, hostname, database and assigned table with the tutor. A .sql file supplies statements; it does not select your connection automatically. Azure Resources sign-in alone is not an MSSQL connection. Keep passwords and sign-in codes private.

## 3. Test the portal connection

Azure portal > SQL databases > assigned database > Query editor. Use the approved authentication. Confirm the database and execute the complete query below. Saving a query does not run it.

```sql
-- Run in each client before writes: the file does not choose or change your connection.
-- Confirm the assigned database and identity; CheckedAtUtc records the server's UTC time.
SELECT DB_NAME() AS DatabaseName,
       SUSER_SNAME() AS LoginName,
       SYSUTCDATETIME() AS CheckedAtUtc;
```

Expected: one row containing your assigned DatabaseName, connected LoginName and server UTC time. This proves this query ran, not permission to change tables. A blank portal blade is not automatically a firewall error.

## 4. Test the same database from VS Code

Open Samples\Sql\01-connection-check.sql in VS Code. Press Ctrl+Shift+P > MS SQL: Connect, select/create the assigned server/database profile and use the approved authentication. Keep encryption enabled and Trust server certificate = False for Azure.

Execute Query / Ctrl+Shift+E runs the file or selection. Clear an unintended selection first. Run the connection query above. Expected: the same assigned database. Login identities may differ if the approved authentication contexts differ; confirm this with the tutor. Timestamps naturally differ.

## 5. Optional: run the same checks in SSMS

Open SSMS and connect to the assigned Azure SQL server using approved authentication and secure connection settings. Choose the assigned database in the query dropdown, click New Query, paste the connection query and press Execute / F5.

Expected: a row from the assigned database, not master. SSMS is an additional client, not the database server. A successful SSMS query does not prove that VS Code or the portal also worked; record each separately.

## 6. Find the correct table - do not guess the example name

If the sample table name is not available, run this read-only table-list query in the assigned database:

```sql
SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_TYPE = 'BASE TABLE'
ORDER BY TABLE_SCHEMA, TABLE_NAME;
```

Expected: tables visible to your identity. Confirm which one the tutor assigned before opening its rows. An empty list can mean no tables or restricted metadata visibility; it is not permission to create a replacement. In SSMS you can also expand Databases > assigned database > Tables and choose Refresh.

## 7A. Read-only exercise - existing instructor-approved table

If the tutor assigns dbo.VsCodeItems_instructor20261028, run this exact read-only query in portal, VS Code and optionally SSMS. Otherwise replace schema/table with the actual approved target and confirm its columns.

```sql
SELECT Id, Name, Status
FROM dbo.VsCodeItems_instructor20261028
ORDER BY Id;
```

Expected for the retained example at preparation: ID 1 Fictional application / Completed; ID 2 Fictional statement / Needs review. These are example existing values, not your insertion evidence. If actual values differ, record them and ask the tutor; do not update a shared table to force the expected result.

After confirming Notes exists, use this fuller query:

```sql
SELECT Id, Name, Status, Notes
FROM dbo.VsCodeItems_instructor20261028
ORDER BY Id;
```

Example Notes: ID 1 Updated from VS Code; ID 2 NULL unless an approved later exercise changed it. NULL means no stored value. The text Updated from VS Code does not prove which client wrote it.

Compare results from new executions in each client. The rows match because the clients read the same remote database, not because VS Code synchronises a local copy. Concurrent changes can alter results; note query time. READ-ONLY PARTICIPANTS STOP HERE and continue to verification, troubleshooting and cleanup below.

## 7B. Approved write exercise - your own disposable table only

Skip this section without explicit write approval. Confirm your assigned database and unique table name before every script. Replace VsCodeItems_ab01 everywhere, including object checks and metadata filters. Do not replace it with VsCodeItems_instructor20261028.

Step 1 - IN THE PORTAL: run Samples\Sql\02-create-and-read.sql, reproduced below. CREATE defines columns; INSERT adds rows. Existing-row guards make reruns leave existing data intact.

```sql
-- Use only an approved disposable target; replace the attendee suffix everywhere.
-- These existence checks make repeat runs leave existing tables/rows intact.
IF OBJECT_ID(N'dbo.VsCodeItems_ab01', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.VsCodeItems_ab01
    (
        Id int NOT NULL PRIMARY KEY,
        Name nvarchar(100) NOT NULL,
        Status nvarchar(30) NOT NULL
    );
END;

-- N-prefixed strings store Unicode text in the nvarchar columns.
IF NOT EXISTS (SELECT 1 FROM dbo.VsCodeItems_ab01 WHERE Id = 1)
    INSERT INTO dbo.VsCodeItems_ab01 (Id, Name, Status)
    VALUES (1, N'Fictional application', N'Received');

IF NOT EXISTS (SELECT 1 FROM dbo.VsCodeItems_ab01 WHERE Id = 2)
    INSERT INTO dbo.VsCodeItems_ab01 (Id, Name, Status)
    VALUES (2, N'Fictional statement', N'Needs review');

-- ORDER BY makes the portal and MSSQL results easy to compare.
SELECT Id, Name, Status
FROM dbo.VsCodeItems_ab01
ORDER BY Id;
```

Expected on a fresh assigned table: ID 1 Fictional application / Received; ID 2 Fictional statement / Needs review. If this table already exists with different contents, stop and confirm ownership instead of dropping or overwriting it.

Step 2 - IN VS CODE: read those same rows using SELECT Id, Name, Status FROM your assigned table ORDER BY Id. Then execute Samples\Sql\03-modify-and-check.sql below. ALTER changes structure; UPDATE changes row values. Keep GO so later statements compile after Notes exists. Keep WHERE Id = 1.

```sql
-- Use only your approved lab table; replace the attendee suffix everywhere.
IF OBJECT_ID(N'dbo.VsCodeItems_ab01', N'U') IS NULL
    THROW 50001, 'Lab table missing. Run 02-create-and-read.sql on the approved database first.', 1;

-- Add the column only once. NULL lets existing rows have no Notes value.
IF COL_LENGTH(N'dbo.VsCodeItems_ab01', N'Notes') IS NULL
    ALTER TABLE dbo.VsCodeItems_ab01 ADD Notes nvarchar(200) NULL;
-- GO starts a new client batch so later statements compile after Notes exists.
GO

-- WHERE limits this repeatable update to ID 1; ID 2 must remain unchanged.
UPDATE dbo.VsCodeItems_ab01
SET Status = N'Completed',
    Notes = N'Updated from VS Code'
WHERE Id = 1;

SELECT Id, Name, Status, Notes
FROM dbo.VsCodeItems_ab01
ORDER BY Id;

-- Inspect schema separately from row values to confirm the ALTER took effect.
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = N'dbo'
  AND TABLE_NAME = N'VsCodeItems_ab01'
ORDER BY ORDINAL_POSITION;
```

Expected: Notes is nullable nvarchar(200); ID 1 becomes Completed / Updated from VS Code; ID 2 stays Needs review / NULL. The WHERE condition restricts the changed row. Without WHERE, UPDATE can change every row.

Step 3 - BACK IN THE PORTAL: replace the example table name below, run this new SELECT and compare with VS Code:

```sql
SELECT Id, Name, Status, Notes
FROM dbo.VsCodeItems_ab01
ORDER BY Id;
```

Expected: both clients show the same final values. Optional SSMS can read these too. If Notes is invalid, check the database/table and whether the ALTER batch executed before rerunning anything.

## 8. Verify and record what you actually completed

Record each client separately: portal connection/read; VS Code connection/read; optional SSMS connection/read. Record writes separately as Passed / Blocked / Not run, with target, time and non-secret query/result evidence.

Read-only completion means you identified and read the approved existing table from the assigned clients. It does not mean you created or updated it. Approved write completion additionally needs the fresh create/read, VS Code ALTER/UPDATE and portal recheck evidence. Screenshot only fictional rows, never credentials.

## 9. If your public IP changes: Azure portal recovery

Moving between home Wi-Fi, the training venue, a hotspot or a VPN can change your public outbound IP. A firewall rule for the old IP may no longer admit your connection. This is a network check, not a reason to reset passwords.

1. Open Azure portal > SQL databases > your assigned database > Overview.    Click the Server name link, then Security > Networking. 2. Check Public network access. If it is Disabled/private-only, stop:    use the approved VPN/private route and ask IT to check DNS/routing.    Do not enable public access. Continue only for an owner-approved    Selected networks route. 3. Have the authorised owner select Add your client IPv4 address, or update    your approved temporary rule. Use a recognisable rule name.    Set Start IPv4 address = End IPv4 address = the approved current public    outbound IPv4. Do not use the laptop's private 192.168.x.x address from    ipconfig. VPNs/proxies can affect the outbound address; if browser and SQL    traffic use different routes, ask IT to confirm the address Azure SQL sees. 4. Click Save, allow a few minutes, reopen the page and verify the saved    rule. If Add/Save is unavailable, ask the owner; do not request broad    administrative access or enable "Allow Azure services and resources to    access this server" as a shortcut. 5. Return to the database's Query editor, authenticate using the approved    method, and execute the read-only check below. Reconnect in VS Code MSSQL    and run the same check. Both should report the assigned database.

```sql
SELECT DB_NAME() AS DatabaseName;
```

6. Ask the owner to remove an obsolete temporary rule only when confirmed no    longer needed. Leave other attendees' and shared rules unchanged.    Record the actual result as Passed / Blocked / Not run without secrets.

An IP change normally needs no code, connection-string, password or Function key change. Firewall access does not grant SQL data permissions.

For Blob transfers, an owner-approved selected-public-networks route may also need a rule update under Storage account > Networking. Private-endpoint access instead requires the approved private route. Do not enable anonymous container access or open all networks to fix it.

## 10. Troubleshooting without risky shortcuts

MS SQL: Connect absent: enable the correct Microsoft extension, open a .sql file, wait for activation and reload if prompted. Ask IT if installation is blocked.

Invalid object name: confirm DB_NAME(), actual schema/table and metadata list. ab01 is an example. Do not create a replacement on a read-only route.

Invalid column Notes: the assigned table may not have it. Read Id/Name/Status first; inspect metadata. Only the approved own-table write route may add it.

Timeout / client IP denied: owner checks approved network route and current public IP. Private-only means approved VPN/DNS, not enabling public access.

Login or SELECT denied: confirm actual authentication and database permissions. Firewall access does not grant data rights; ask the owner.

Portal blank/loading: confirm directory, refresh once and use a supported browser. Record Blocked if persistent; do not open the firewall to fix a UI symptom.

Unexpected row changes: check statement selection, correct database/table and concurrent users. Stop; do not rerun writes blindly.

## 11. Knowledge checks and safe cleanup

Before: does saving a .sql file execute it? Does Azure sign-in grant SELECT? Do you need local SQL Server to query Azure SQL?

After: explain table structure versus row values; what does WHERE protect; why does the same database appear in three clients; what did your evidence prove and leave untested? Explain why a stored note is not proof of the client that wrote it.

Cost decision: identify a compute/storage/backup cost driver, one potential saving and its risk. Do not change the shared tier, pause policy or backup retention. Closing SSMS/VS Code does not stop cloud billing.

Disconnect clients when finished, especially on a shared machine. Delete no shared table, database, server or resource group. Only remove your disposable table if its owner explicitly approves and you have checked the exact name; no automatic deletion script is supplied. Temporary firewall cleanup belongs to the authorised owner. Keep only non-secret evidence.

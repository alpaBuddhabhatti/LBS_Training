# SQL exercise

Follow the [complete SQL handout](../../Day%201/Basic_Lab_01_Azure_SQL_Portal_And_VS_Code.docx).
It includes identity and current-IP preparation, portal/MSSQL connection steps,
creating rows, adding a column, updating data, verification and safe cleanup.

Use [Samples/Sql](Samples/Sql) and your own assigned table suffix. Connect to
the same approved server/database in both clients. Do not modify the instructor
verification table or shared network settings.


---

## Day 1 tested walkthrough - student support

Updated 28 October 2026.

### 2. SQL: two clients, one Azure database

![sql clients](<../../Day 1/Diagrams/sql-clients.png>)

Open the assigned database in portal > Query editor. In VS Code, install the approved Microsoft SQL Server (mssql) extension, open a .sql file and choose MS SQL: Connect. Use the exact server hostname and database, approved authentication, encryption enabled and Trust server certificate = False for Azure. Azure Resources sign-in alone is not a SQL connection.

Run Samples\Sql\01-connection-check.sql in both clients. Confirm DatabaseName is the assigned database, not master. Clear accidental text selections: the client may run only selected SQL. Click Run in the portal or Execute Query / Ctrl+Shift+E in MSSQL. Saving a .sql file does not run it.

Approved write route: replace VsCodeItems_ab01 everywhere with your assigned disposable table name. In the portal run 02-create-and-read.sql; expect two fictional rows. In VS Code read those rows, then run 03-modify-and-check.sql. Keep GO between ALTER and later statements. Expect a nullable Notes column and only ID 1 changed to Completed. Rerun SELECT in the portal to verify; no synchronisation or deployment is needed.

```sql
SELECT DB_NAME() AS DatabaseName;

SELECT Id, Name, Status, Notes
FROM dbo.VsCodeItems_ab01
ORDER BY Id;
```

Use this Notes query only after the column exists, and replace ab01. Expected final rows: ID 1 / Fictional application / Completed / Updated from VS Code; ID 2 / Fictional statement / Needs review / NULL. NULL means no stored value. The optional reverse update changes ID 2 Notes to Checked in portal; verify that from VS Code.

Read-only route: if writes are not authorised, use the instructor-approved table and supplied 04-read-only-verification.sql. Replace its example table name with the approved name. Do not run creation or modification scripts against a shared instructor table. Mark write steps Blocked or Not run. A stored Notes value saying 'Updated from VS Code' does not prove which client performed the update.

### SQL first-run troubleshooting

A blank/loading portal editor is not proof of a firewall problem. Refresh once, check the correct directory and use the supported browser; ask the instructor if it persists. A paused serverless database may need time to resume. Connection timeouts require approved network-path checks. Do not enable public access, trust all certificates or allow all Azure services. The owner alone approves any narrowly scoped temporary firewall change.

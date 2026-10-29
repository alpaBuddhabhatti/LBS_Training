-- Read-only verification of the retained instructor rehearsal, not an attendee table.
-- Connect to firstsqlserver2026.database.windows.net and database firstdb.
-- Run the same SELECTs in portal and MSSQL; matching rows do not prove which client wrote them.
SELECT DB_NAME() AS DatabaseName,
       SUSER_SNAME() AS LoginName;

SELECT Id, Name, Status, Notes
FROM dbo.VsCodeItems_instructor20261028
ORDER BY Id;

SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = N'dbo'
  AND TABLE_NAME = N'VsCodeItems_instructor20261028'
ORDER BY ORDINAL_POSITION;

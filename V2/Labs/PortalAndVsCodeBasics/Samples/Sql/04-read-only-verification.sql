-- Read-only route: replace VsCodeItems_ab01 with the table approved by your instructor.
-- Connect to your assigned server and database; this script does not change data.
-- Run the same SELECTs in portal and MSSQL; matching rows do not prove which client wrote them.
SELECT DB_NAME() AS DatabaseName,
       SUSER_SNAME() AS LoginName;

SELECT Id, Name, Status
FROM dbo.VsCodeItems_ab01
ORDER BY Id;

SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = N'dbo'
  AND TABLE_NAME = N'VsCodeItems_ab01'
ORDER BY ORDINAL_POSITION;

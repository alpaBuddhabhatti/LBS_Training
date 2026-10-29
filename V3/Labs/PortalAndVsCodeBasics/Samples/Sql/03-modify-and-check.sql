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

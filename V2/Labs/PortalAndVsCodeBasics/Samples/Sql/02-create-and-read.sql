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

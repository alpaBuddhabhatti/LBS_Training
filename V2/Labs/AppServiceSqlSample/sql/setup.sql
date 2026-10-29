-- Run this script only in the designated training database using an
-- authorized Microsoft Entra SQL administrator. Replace the identity
-- placeholder before running the permissions section.

-- Create only the lab table if absent; rerunning does not drop existing data.
IF OBJECT_ID(N'dbo.TrainingItems', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TrainingItems
    (
        Id int IDENTITY(1, 1) NOT NULL
            CONSTRAINT PK_TrainingItems PRIMARY KEY,
        Name nvarchar(80) NOT NULL,
        Description nvarchar(200) NOT NULL
    );
END;
GO

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.TrainingItems
    WHERE Name = N'Welcome to the lab'
)
BEGIN
    INSERT INTO dbo.TrainingItems (Name, Description)
    VALUES
    (
        N'Welcome to the lab',
        N'This sample row is read by the App Service managed identity.'
    );
END;
GO

-- Replace <APP_SERVICE_NAME> with the exact system-assigned identity name.
-- This creates a database identity, not an Azure resource or network/firewall rule.
IF DATABASE_PRINCIPAL_ID(N'<APP_SERVICE_NAME>') IS NULL
BEGIN
    CREATE USER [<APP_SERVICE_NAME>] FROM EXTERNAL PROVIDER;
END;
GO

-- The sample only reads rows, so it receives SELECT on this table only.
GRANT SELECT ON OBJECT::dbo.TrainingItems TO [<APP_SERVICE_NAME>];
GO

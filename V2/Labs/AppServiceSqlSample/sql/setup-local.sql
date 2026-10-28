-- Run against the local SQL Express instance with Windows authentication.
-- Keep the local sample separate from existing application databases.
USE master;
GO

-- This separate local database is not the Azure SQL database from the cloud exercise.
IF DB_ID(N'LbsTraining') IS NULL
BEGIN
    CREATE DATABASE LbsTraining;
END;
GO

USE LbsTraining;
GO

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

-- Seed the fictional row only if absent so a repeat run does not duplicate it.
IF NOT EXISTS
(
    SELECT 1 FROM dbo.TrainingItems WHERE Name = N'Welcome to the lab'
)
BEGIN
    INSERT INTO dbo.TrainingItems (Name, Description)
    VALUES
    (
        N'Welcome to the lab',
        N'This sample row is read from local SQL Express using Windows authentication.'
    );
END;
GO

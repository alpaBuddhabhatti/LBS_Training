-- Apply only after approval, connected to firstdb as an authorized schema administrator.
IF DB_NAME() <> N'firstdb'
    THROW 50001, 'Schema setup must target firstdb.', 1;
SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF SCHEMA_ID(N'lbs_training') IS NULL
    EXEC(N'CREATE SCHEMA lbs_training AUTHORIZATION dbo;');

IF OBJECT_ID(N'lbs_training.Documents', N'U') IS NULL
BEGIN
    CREATE TABLE lbs_training.Documents
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_lbs_training_Documents PRIMARY KEY,
        Reference nvarchar(13) NOT NULL,
        Status nvarchar(20) NOT NULL CONSTRAINT CK_lbs_training_Status
            CHECK (Status IN (N'Queued', N'Completed', N'Needs review')),
        CreatedAt datetimeoffset NOT NULL,
        CompletedAt datetimeoffset NULL,
        ResultSummary nvarchar(500) NULL,
        DocumentText nvarchar(max) NULL
    );
    CREATE INDEX IX_lbs_training_Documents_CreatedAt ON lbs_training.Documents (CreatedAt DESC);
END;

IF OBJECT_ID(N'lbs_training.Outbox', N'U') IS NULL
BEGIN
    CREATE TABLE lbs_training.Outbox
    (
        Id uniqueidentifier NOT NULL CONSTRAINT PK_lbs_training_Outbox PRIMARY KEY,
        DocumentId uniqueidentifier NOT NULL
            CONSTRAINT FK_lbs_training_Outbox_Documents REFERENCES lbs_training.Documents(Id),
        CreatedAt datetimeoffset NOT NULL,
        DispatchedAt datetimeoffset NULL,
        CONSTRAINT UQ_lbs_training_Outbox_Document UNIQUE (DocumentId)
    );
    CREATE INDEX IX_lbs_training_Outbox_Pending ON lbs_training.Outbox (CreatedAt)
        INCLUDE (Id, DocumentId) WHERE DispatchedAt IS NULL;
END;

COMMIT TRANSACTION;

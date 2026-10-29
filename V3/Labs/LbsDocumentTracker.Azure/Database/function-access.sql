-- Run in firstdb using an authorized Microsoft Entra administrator connection.
IF DB_NAME() <> N'firstdb'
    THROW 50001, 'Function access must target firstdb.', 1;

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'fristAzureFunctionApp')
    CREATE USER [fristAzureFunctionApp] FROM EXTERNAL PROVIDER;

-- Resolve the directory identity instead of guessing its SQL SID from the object ID.
IF NOT EXISTS (
    SELECT 1 FROM sys.database_principals
    WHERE name = N'fristAzureFunctionApp'
      AND sid = CONVERT(binary(16), CONVERT(uniqueidentifier, N'e358bc51-ed73-4d5b-a834-fc6bbc3acd20'))
)
    THROW 50002, 'The SQL user does not match the approved Function identity.', 1;

GRANT SELECT, UPDATE ON OBJECT::lbs_training.Documents TO [fristAzureFunctionApp];
GRANT SELECT, UPDATE ON OBJECT::lbs_training.Outbox TO [fristAzureFunctionApp];

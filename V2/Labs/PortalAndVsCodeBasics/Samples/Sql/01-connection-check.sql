-- Run in each client before writes: the file does not choose or change your connection.
-- Confirm the assigned database and identity; CheckedAtUtc records the server's UTC time.
SELECT DB_NAME() AS DatabaseName,
       SUSER_SNAME() AS LoginName,
       SYSUTCDATETIME() AS CheckedAtUtc;

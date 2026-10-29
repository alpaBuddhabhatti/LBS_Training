using System.Data;
using Azure.Core;
using Microsoft.Data.SqlClient;

namespace LbsDocumentTracker.Azure.Core;

public sealed class SqlDocumentRepository(string server, string database, TokenCredential credential)
    : IDocumentRepository
{
    private readonly string _connectionString = new SqlConnectionStringBuilder
    {
        DataSource = $"tcp:{server},1433",
        InitialCatalog = database,
        Encrypt = SqlConnectionEncryptOption.Mandatory,
        TrustServerCertificate = false,
        ConnectTimeout = 15
    }.ConnectionString;

    // Reuse the callback instance so connection pooling and token refresh work together.
    private readonly Func<SqlAuthenticationParameters, CancellationToken, Task<SqlAuthenticationToken>> _tokenCallback =
        async (_, cancellationToken) =>
        {
            var token = await credential.GetTokenAsync(
                new TokenRequestContext(["https://database.windows.net/.default"]), cancellationToken);
            return new SqlAuthenticationToken(token.Token, token.ExpiresOn);
        };

    private async Task<SqlConnection> OpenAsync(CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(_connectionString) { AccessTokenCallback = _tokenCallback };
        try
        {
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
        catch
        {
            await connection.DisposeAsync();
            throw;
        }
    }

    public async Task<Guid> SubmitAsync(string reference, string text, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        await using var connection = await OpenAsync(cancellationToken);
        using var command = connection.CreateCommand();
        // One SQL transaction commits both the upload and its durable dispatch intent.
        command.CommandText = """
            SET XACT_ABORT ON;
            BEGIN TRANSACTION;
            INSERT INTO lbs_training.Documents (Id, Reference, Status, CreatedAt, DocumentText)
            VALUES (@Id, @Reference, N'Queued', SYSDATETIMEOFFSET(), @Text);
            INSERT INTO lbs_training.Outbox (Id, DocumentId, CreatedAt)
            VALUES (@OutboxId, @Id, SYSDATETIMEOFFSET());
            COMMIT TRANSACTION;
            """;
        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
        command.Parameters.Add("@OutboxId", SqlDbType.UniqueIdentifier).Value = Guid.NewGuid();
        command.Parameters.Add("@Reference", SqlDbType.NVarChar, 13).Value = reference;
        command.Parameters.Add("@Text", SqlDbType.NVarChar, -1).Value = text;
        await command.ExecuteNonQueryAsync(cancellationToken);
        return id;
    }

    public async Task<IReadOnlyList<DocumentRecord>> GetRecentAsync(CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT TOP (100) Id, Reference, Status, CreatedAt, CompletedAt, ResultSummary
            FROM lbs_training.Documents ORDER BY CreatedAt DESC;
            """;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var documents = new List<DocumentRecord>();
        while (await reader.ReadAsync(cancellationToken))
        {
            documents.Add(ReadDocument(reader));
        }

        return documents;
    }

    public async Task<DocumentRecord?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Reference, Status, CreatedAt, CompletedAt, ResultSummary
            FROM lbs_training.Documents WHERE Id = @Id;
            """;
        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? ReadDocument(reader) : null;
    }

    public async Task<PendingDocument?> GetPendingAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Reference, DocumentText, CompletedAt FROM lbs_training.Documents WHERE Id = @Id;
            """;
        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new(reader.GetGuid(0), reader.GetString(1),
            reader.IsDBNull(2) ? string.Empty : reader.GetString(2), !reader.IsDBNull(3));
    }

    public async Task CompleteAsync(Guid id, bool accepted, string summary, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE lbs_training.Documents
            SET Status = @Status, CompletedAt = SYSDATETIMEOFFSET(), ResultSummary = @Summary, DocumentText = NULL
            WHERE Id = @Id AND CompletedAt IS NULL;
            """;
        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = id;
        command.Parameters.Add("@Status", SqlDbType.NVarChar, 20).Value = accepted ? "Completed" : "Needs review";
        command.Parameters.Add("@Summary", SqlDbType.NVarChar, 500).Value = summary;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<OutboxEntry>> GetOutboxAsync(CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT TOP (20) Id, DocumentId FROM lbs_training.Outbox
            WHERE DispatchedAt IS NULL ORDER BY CreatedAt;
            """;
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var entries = new List<OutboxEntry>();
        while (await reader.ReadAsync(cancellationToken))
        {
            entries.Add(new(reader.GetGuid(0), reader.GetGuid(1)));
        }

        return entries;
    }

    public async Task MarkDispatchedAsync(Guid outboxId, CancellationToken cancellationToken)
    {
        await using var connection = await OpenAsync(cancellationToken);
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE lbs_training.Outbox SET DispatchedAt = SYSDATETIMEOFFSET()
            WHERE Id = @Id AND DispatchedAt IS NULL;
            """;
        command.Parameters.Add("@Id", SqlDbType.UniqueIdentifier).Value = outboxId;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    private static DocumentRecord ReadDocument(SqlDataReader reader) => new(
        reader.GetGuid(0), reader.GetString(1), reader.GetString(2), reader.GetDateTimeOffset(3),
        reader.IsDBNull(4) ? null : reader.GetDateTimeOffset(4),
        reader.IsDBNull(5) ? null : reader.GetString(5));
}

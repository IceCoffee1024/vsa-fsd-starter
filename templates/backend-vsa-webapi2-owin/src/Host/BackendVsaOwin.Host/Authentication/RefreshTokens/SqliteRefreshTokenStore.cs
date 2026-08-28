using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using BackendVsaOwin.BuildingBlocks.Persistence.Sqlite;
using Dapper;

namespace BackendVsaOwin.Host.Authentication.RefreshTokens;

internal sealed class SqliteRefreshTokenStore : IRefreshTokenStore
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteRefreshTokenStore(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<bool> StoreAsync(
        RefreshTokenEntry entry,
        CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        var affectedRows = await connection.ExecuteAsync(
            new CommandDefinition(
                "INSERT INTO oauth_refresh_tokens "
                    + "(token_hash, family_id, client_id, protected_ticket, created_utc, expires_utc) "
                    + "SELECT @TokenHash, @FamilyId, @ClientId, @ProtectedTicket, @CreatedUtc, @ExpiresUtc "
                    + "WHERE NOT EXISTS ("
                    + "SELECT 1 FROM oauth_refresh_tokens "
                    + "WHERE family_id = @FamilyId AND revoked_utc IS NOT NULL);",
                new
                {
                    entry.TokenHash,
                    entry.FamilyId,
                    entry.ClientId,
                    entry.ProtectedTicket,
                    CreatedUtc = FormatTimestamp(entry.CreatedUtc),
                    ExpiresUtc = FormatTimestamp(entry.ExpiresUtc),
                },
                cancellationToken: cancellationToken));
        return affectedRows == 1;
    }

    public async Task<string?> RedeemAsync(
        string tokenHash,
        DateTimeOffset redeemedUtc,
        CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();
        var timestamp = FormatTimestamp(redeemedUtc);
        var parameters = new
        {
            TokenHash = tokenHash,
            RedeemedUtc = timestamp,
        };
        var affectedRows = await connection.ExecuteAsync(
            new CommandDefinition(
                "UPDATE oauth_refresh_tokens SET consumed_utc = @RedeemedUtc "
                    + "WHERE token_hash = @TokenHash "
                    + "AND consumed_utc IS NULL "
                    + "AND revoked_utc IS NULL "
                    + "AND expires_utc > @RedeemedUtc;",
                parameters,
                transaction,
                cancellationToken: cancellationToken));

        if (affectedRows == 1)
        {
            var protectedTicket = await connection.QuerySingleAsync<string>(
                new CommandDefinition(
                    "SELECT protected_ticket FROM oauth_refresh_tokens "
                        + "WHERE token_hash = @TokenHash;",
                    parameters,
                    transaction,
                    cancellationToken: cancellationToken));
            transaction.Commit();
            return protectedTicket;
        }

        var replayedFamilyId = await connection.QuerySingleOrDefaultAsync<string>(
            new CommandDefinition(
                "SELECT family_id FROM oauth_refresh_tokens "
                    + "WHERE token_hash = @TokenHash "
                    + "AND consumed_utc IS NOT NULL "
                    + "AND revoked_utc IS NULL "
                    + "AND expires_utc > @RedeemedUtc;",
                parameters,
                transaction,
                cancellationToken: cancellationToken));
        if (replayedFamilyId is not null)
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    "UPDATE oauth_refresh_tokens SET revoked_utc = @RedeemedUtc "
                        + "WHERE family_id = @FamilyId AND revoked_utc IS NULL;",
                    new
                    {
                        FamilyId = replayedFamilyId,
                        RedeemedUtc = timestamp,
                    },
                    transaction,
                    cancellationToken: cancellationToken));
        }

        transaction.Commit();
        return null;
    }

    private static string FormatTimestamp(DateTimeOffset value)
    {
        return value.UtcDateTime.ToString("O", CultureInfo.InvariantCulture);
    }
}

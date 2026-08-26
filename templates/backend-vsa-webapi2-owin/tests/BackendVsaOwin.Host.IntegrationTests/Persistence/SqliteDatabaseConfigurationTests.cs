using System;
using System.IO;
using System.Threading.Tasks;
using BackendVsaOwin.BuildingBlocks.Persistence.Sqlite;
using BackendVsaOwin.Host.IntegrationTests.Support;
using Microsoft.Data.Sqlite;
using Xunit;

namespace BackendVsaOwin.Host.IntegrationTests.Persistence;

public sealed class SqliteDatabaseConfigurationTests
{
    [Fact]
    public async Task Host_enables_wal_before_serving_requests()
    {
        var directory = CreateTemporaryDirectory();
        var databasePath = Path.Combine(directory, "wal.db");

        try
        {
            using (TestServerFactory.Create(databasePath))
            {
            }

            var factory = new SqliteConnectionFactory(databasePath, pooling: false);
            using var connection = await factory.OpenConnectionAsync(
                TestContext.Current.CancellationToken);
            using var command = connection.CreateCommand();
            command.CommandText = "PRAGMA journal_mode;";

            Assert.Equal(
                "wal",
                Convert.ToString(await command.ExecuteScalarAsync(
                    TestContext.Current.CancellationToken)));
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Theory]
    [InlineData(SqliteConnectionFactory.DefaultBusyTimeoutSeconds)]
    [InlineData(12)]
    public void Connection_factory_applies_the_configured_busy_timeout(int busyTimeoutSeconds)
    {
        var factory = new SqliteConnectionFactory(
            "configuration.db",
            pooling: false,
            busyTimeoutSeconds);
        var builder = new SqliteConnectionStringBuilder(factory.ConnectionString);

        Assert.Equal(busyTimeoutSeconds, builder.DefaultTimeout);
    }

    private static string CreateTemporaryDirectory()
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "BackendVsaOwin.Tests",
            Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        return directory;
    }
}

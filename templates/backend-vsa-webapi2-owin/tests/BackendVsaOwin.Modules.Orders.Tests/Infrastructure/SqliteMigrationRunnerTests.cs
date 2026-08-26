using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Xunit;

namespace BackendVsaOwin.Modules.Orders.Tests.Infrastructure;

public sealed class SqliteMigrationRunnerTests
{
    [Fact]
    public async System.Threading.Tasks.Task Applies_module_migrations_in_one_database_idempotently()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        using var database = new OrderSqliteDatabase(migrate: false);

        database.Migrate();
        var firstRunScripts = await database.GetAppliedScriptsAsync(cancellationToken);
        database.Migrate();
        var secondRunScripts = await database.GetAppliedScriptsAsync(cancellationToken);

        Assert.True(await database.TableExistsAsync("customers", cancellationToken));
        Assert.True(await database.TableExistsAsync("orders", cancellationToken));
        Assert.Equal(2, firstRunScripts.Length);
        Assert.Contains(firstRunScripts, name => name.EndsWith("001_CreateCustomers.sql"));
        Assert.Contains(firstRunScripts, name => name.EndsWith("001_CreateOrders.sql"));
        Assert.True(firstRunScripts.SequenceEqual(secondRunScripts));
    }

    [Fact]
    public void Writes_migration_events_to_the_supplied_logger_factory()
    {
        using var database = new OrderSqliteDatabase(migrate: false);
        using var loggerFactory = new RecordingLoggerFactory();

        database.Migrate(loggerFactory);

        Assert.NotEmpty(loggerFactory.Messages);
    }

    private sealed class RecordingLoggerFactory : ILoggerFactory
    {
        public List<string> Messages { get; } = new();

        public void AddProvider(ILoggerProvider provider)
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new RecordingLogger(Messages);
        }

        public void Dispose()
        {
        }
    }

    private sealed class RecordingLogger : ILogger
    {
        private readonly ICollection<string> _messages;

        public RecordingLogger(ICollection<string> messages)
        {
            _messages = messages;
        }

        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            _messages.Add(formatter(state, exception));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendVsaOwin.BuildingBlocks.Persistence.Sqlite;
using BackendVsaOwin.Modules.Customers.Domain;
using Dapper;
using Microsoft.Data.Sqlite;

namespace BackendVsaOwin.Modules.Customers.Infrastructure;

internal sealed class SqliteCustomerStore : ICustomerStore
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteCustomerStore(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task AddAsync(
        Customer customer,
        CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

        try
        {
            await connection.ExecuteAsync(
                new CommandDefinition(
                    "INSERT INTO customers (id, display_name) "
                        + "VALUES (@Id, @DisplayName);",
                    new
                    {
                        Id = customer.Id.ToString("D"),
                        customer.DisplayName,
                    },
                    cancellationToken: cancellationToken));
        }
        catch (SqliteException exception) when (IsUniqueConstraint(exception))
        {
            throw new InvalidOperationException(
                $"Customer '{customer.Id}' already exists.",
                exception);
        }
    }

    public async Task<Customer?> GetAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<CustomerRow>(
            new CommandDefinition(
                "SELECT id AS Id, display_name AS DisplayName "
                    + "FROM customers WHERE id = @Id;",
                new { Id = id.ToString("D") },
                cancellationToken: cancellationToken));
        return row?.ToDomain();
    }

    public async Task<IReadOnlyCollection<Customer>> GetManyAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<Customer>();
        }

        var distinctIds = ids.Distinct().ToArray();
        using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<CustomerRow>(
            new CommandDefinition(
                "SELECT id AS Id, display_name AS DisplayName "
                    + "FROM customers WHERE id IN @Ids;",
                new
                {
                    Ids = distinctIds.Select(id => id.ToString("D")).ToArray(),
                },
                cancellationToken: cancellationToken));
        var customersById = rows
            .Select(row => row.ToDomain())
            .ToDictionary(customer => customer.Id);

        return distinctIds
            .Where(customersById.ContainsKey)
            .Select(id => customersById[id])
            .ToArray();
    }

    private sealed class CustomerRow
    {
        public string Id { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public Customer ToDomain()
        {
            return new Customer(Guid.Parse(Id), DisplayName);
        }
    }

    private static bool IsUniqueConstraint(SqliteException exception)
    {
        const int primaryKeyConstraint = 1555;
        const int uniqueConstraint = 2067;

        return exception.SqliteExtendedErrorCode is primaryKeyConstraint or uniqueConstraint;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackendVsaOwin.BuildingBlocks.Persistence.Sqlite;
using BackendVsaOwin.Modules.Orders.Domain;
using Dapper;
using Microsoft.Data.Sqlite;

namespace BackendVsaOwin.Modules.Orders.Infrastructure;

internal sealed class SqliteOrderStore : IOrderStore
{
    private readonly SqliteConnectionFactory _connectionFactory;

    public SqliteOrderStore(SqliteConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task AddAsync(
        Order order,
        CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);

        try
        {
            await ExecuteInsertAsync(
                connection,
                transaction: null,
                order,
                cancellationToken);
        }
        catch (SqliteException exception) when (IsUniqueConstraint(exception))
        {
            throw DuplicateOrder(order.Id, exception);
        }
    }

    public async Task AddManyAsync(
        IReadOnlyCollection<Order> orders,
        CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        try
        {
            foreach (var order in orders)
            {
                await ExecuteInsertAsync(
                    connection,
                    transaction,
                    order,
                    cancellationToken);
            }

            transaction.Commit();
        }
        catch (SqliteException exception) when (IsUniqueConstraint(exception))
        {
            throw new InvalidOperationException(
                "The batch contains an order ID that already exists or is duplicated.",
                exception);
        }
    }

    public async Task<Order?> GetAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await GetAsync(connection, id, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Order>> ListAsync(
        CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<OrderRow>(
            new CommandDefinition(
                "SELECT id AS Id, customer_id AS CustomerId, "
                    + "customer_name AS CustomerName, total_amount AS TotalAmount "
                    + "FROM orders ORDER BY id;",
                cancellationToken: cancellationToken));
        return rows.Select(row => row.ToDomain()).ToArray();
    }

    public async Task<Order?> UpdateAsync(
        Guid id,
        decimal totalAmount,
        CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        var affectedRows = await connection.ExecuteAsync(
            new CommandDefinition(
                "UPDATE orders SET total_amount = @TotalAmount WHERE id = @Id;",
                new
                {
                    Id = id.ToString("D"),
                    TotalAmount = totalAmount,
                },
                cancellationToken: cancellationToken));
        return affectedRows == 0
            ? null
            : await GetAsync(connection, id, cancellationToken);
    }

    public async Task<bool> DeleteAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        return await connection.ExecuteAsync(
            new CommandDefinition(
                "DELETE FROM orders WHERE id = @Id;",
                new { Id = id.ToString("D") },
                cancellationToken: cancellationToken)) > 0;
    }

    public async Task<IReadOnlyCollection<Guid>> DeleteManyAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.OpenConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();
        var deletedIds = new List<Guid>(ids.Count);

        foreach (var id in ids)
        {
            if (await connection.ExecuteAsync(
                    new CommandDefinition(
                        "DELETE FROM orders WHERE id = @Id;",
                        new { Id = id.ToString("D") },
                        transaction,
                        cancellationToken: cancellationToken)) > 0)
            {
                deletedIds.Add(id);
            }
        }

        transaction.Commit();
        return deletedIds;
    }

    private static Task<int> ExecuteInsertAsync(
        SqliteConnection connection,
        SqliteTransaction? transaction,
        Order order,
        CancellationToken cancellationToken)
    {
        return connection.ExecuteAsync(
            new CommandDefinition(
                "INSERT INTO orders "
                    + "(id, customer_id, customer_name, total_amount) "
                    + "VALUES (@Id, @CustomerId, @CustomerName, @TotalAmount);",
                new
                {
                    Id = order.Id.ToString("D"),
                    CustomerId = order.CustomerId.ToString("D"),
                    order.CustomerName,
                    order.TotalAmount,
                },
                transaction,
                cancellationToken: cancellationToken));
    }

    private static async Task<Order?> GetAsync(
        SqliteConnection connection,
        Guid id,
        CancellationToken cancellationToken)
    {
        var row = await connection.QuerySingleOrDefaultAsync<OrderRow>(
            new CommandDefinition(
                "SELECT id AS Id, customer_id AS CustomerId, "
                    + "customer_name AS CustomerName, total_amount AS TotalAmount "
                    + "FROM orders WHERE id = @Id;",
                new { Id = id.ToString("D") },
                cancellationToken: cancellationToken));
        return row?.ToDomain();
    }

    private sealed class OrderRow
    {
        public string Id { get; set; } = string.Empty;

        public string CustomerId { get; set; } = string.Empty;

        public string CustomerName { get; set; } = string.Empty;

        public decimal TotalAmount { get; set; }

        public Order ToDomain()
        {
            return new Order(
                Guid.Parse(Id),
                Guid.Parse(CustomerId),
                CustomerName,
                TotalAmount);
        }
    }

    private static bool IsUniqueConstraint(SqliteException exception)
    {
        const int primaryKeyConstraint = 1555;
        const int uniqueConstraint = 2067;

        return exception.SqliteExtendedErrorCode is primaryKeyConstraint or uniqueConstraint;
    }

    private static InvalidOperationException DuplicateOrder(
        Guid id,
        SqliteException exception)
    {
        return new InvalidOperationException($"Order '{id}' already exists.", exception);
    }
}

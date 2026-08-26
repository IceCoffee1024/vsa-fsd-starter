using System;
using System.Configuration;
using System.IO;

namespace BackendVsaOwin.Host.Persistence;

internal sealed class DatabaseOptions
{
    private const string DatabasePathSettingName = "DatabasePath";

    private DatabaseOptions(string databasePath, bool pooling)
    {
        DatabasePath = databasePath;
        Pooling = pooling;
    }

    public string DatabasePath { get; }

    public bool Pooling { get; }

    public static DatabaseOptions FromConfiguration()
    {
        var configuredPath = ConfigurationManager.AppSettings[DatabasePathSettingName];
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            throw new ConfigurationErrorsException(
                $"The '{DatabasePathSettingName}' app setting is required.");
        }

        var databasePath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configuredPath);
        return Create(databasePath, pooling: true);
    }

    internal static DatabaseOptions Create(string databasePath, bool pooling)
    {
        if (string.IsNullOrWhiteSpace(databasePath))
        {
            throw new ArgumentException("A database path is required.", nameof(databasePath));
        }

        var fullPath = Path.GetFullPath(databasePath);
        var directory = Path.GetDirectoryName(fullPath);
        if (string.IsNullOrEmpty(directory))
        {
            throw new ArgumentException(
                "The database path must include a parent directory.",
                nameof(databasePath));
        }

        Directory.CreateDirectory(directory);
        return new DatabaseOptions(fullPath, pooling);
    }
}

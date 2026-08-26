using System;
using System.IO;
using System.Threading;
using BackendVsaOwin.Host.Persistence;
using Owin;

namespace BackendVsaOwin.Host.IntegrationTests.Support;

internal static class TemporaryDatabase
{
    public static DatabaseOptions CreateOptions(IAppBuilder app)
    {
        var directory = Path.Combine(
            Path.GetTempPath(),
            "BackendVsaOwin.Tests",
            Guid.NewGuid().ToString("N"));
        var databasePath = Path.Combine(directory, "test.db");
        var options = DatabaseOptions.Create(databasePath, pooling: false);

        if (app.Properties.TryGetValue("host.OnAppDisposing", out var value)
            && value is CancellationToken cancellationToken)
        {
            cancellationToken.Register(() => Delete(directory));
        }

        return options;
    }

    private static void Delete(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, recursive: true);
            }
        }
        catch (IOException)
        {
            // A failed cleanup must not hide the test result.
        }
        catch (UnauthorizedAccessException)
        {
            // A failed cleanup must not hide the test result.
        }
    }
}

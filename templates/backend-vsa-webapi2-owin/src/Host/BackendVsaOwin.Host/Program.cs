using System;
using System.Configuration;
using System.Threading;
using HostApplicationIdentity = BackendVsaOwin.Host.Composition.ApplicationIdentity;
using Microsoft.Owin.Hosting;

namespace BackendVsaOwin.Host;

internal static class Program
{
    private const string DefaultHostUrl = "http://localhost:5088/";

    private static void Main(string[] args)
    {
        var hostUrl = args.Length > 0
            ? args[0]
            : ConfigurationManager.AppSettings["HostUrl"] ?? DefaultHostUrl;

        using (var shutdown = new ManualResetEventSlim())
        using (WebApp.Start<Startup>(hostUrl))
        {
            Console.CancelKeyPress += (_, eventArgs) =>
            {
                eventArgs.Cancel = true;
                shutdown.Set();
            };

            Console.WriteLine(
                $"{HostApplicationIdentity.ApplicationName} "
                + $"is listening on {hostUrl}");
            Console.WriteLine("Press Ctrl+C to stop.");
            shutdown.Wait();
        }
    }
}

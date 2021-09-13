﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using UiPath.CoreIpc.NamedPipe;

namespace UiPath.CoreIpc.Tests
{
    class Server
    {
        //private static readonly Timer _timer = new Timer(_ =>
        //{
        //    Console.WriteLine("GC.Collect");
        //    GC.Collect();
        //    GC.WaitForPendingFinalizers();
        //    GC.Collect();
        //}, null, 0, 3000);
        static async Task Main()
        {
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
            //GuiLikeSyncContext.Install();
            Console.WriteLine(SynchronizationContext.Current);
            var serviceProvider = ConfigureServices();
            // build and run service host
            var host = new ServiceHostBuilder(serviceProvider)
                .UseNamedPipes(new NamedPipeSettings("test")
                {
                    RequestTimeout = TimeSpan.FromSeconds(2),
                    //AccessControl = security => security.AllowCurrentUser(),
                })
                .AddEndpoint<IComputingService, IComputingCallback>()
                .AddEndpoint<ISystemService>()
                .ValidateAndBuild();

            await await Task.WhenAny(host.RunAsync(), Task.Run(() =>
            {
                Console.WriteLine(typeof(int).Assembly);
                Console.ReadLine();
                host.Dispose();
            }));

            Console.WriteLine("Server stopped.");
        }

        private static IServiceProvider ConfigureServices() =>
            new ServiceCollection()
                .AddIpcWithLogging()
                .AddSingleton<IComputingService, ComputingService>()
                .AddSingleton<ISystemService, SystemService>()
                .BuildServiceProvider();
    }
}
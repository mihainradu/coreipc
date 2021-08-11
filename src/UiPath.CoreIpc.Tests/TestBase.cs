﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;

namespace UiPath.CoreIpc.Tests
{
    public abstract class TestBase : IDisposable
    {
        protected const int MaxReceivedMessageSizeInMegabytes = 1;
        protected static int Count = -1;
        protected static readonly TimeSpan RequestTimeout =
#if CI
            TimeSpan.FromSeconds(2) +
#endif
            TimeSpan.FromSeconds(2);
        protected readonly IServiceProvider _serviceProvider;
        protected readonly AsyncContext _guiThread = new AsyncContextThread().Context;

        public TestBase()
        {
            _guiThread.SynchronizationContext.Send(() => Thread.CurrentThread.Name = "GuiThread");
            _serviceProvider = IpcHelpers.ConfigureServices();
        }

        protected static int GetCount() => Interlocked.Increment(ref Count);

        protected TaskScheduler GuiScheduler => _guiThread.Scheduler;

        public virtual void Dispose() => _guiThread.Dispose();
        protected virtual TSettings Configure<TSettings>(TSettings listenerSettings) where TSettings : ListenerSettings
        {
            listenerSettings.RequestTimeout = RequestTimeout;
            listenerSettings.MaxReceivedMessageSizeInMegabytes = MaxReceivedMessageSizeInMegabytes;
            return listenerSettings;
        }
        protected abstract ServiceHostBuilder Configure(ServiceHostBuilder serviceHostBuilder);
    }
}
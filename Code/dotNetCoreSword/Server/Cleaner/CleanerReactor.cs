using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sword.Server.Cleaner
{
    class CleanerReactor
    {
        private OfflineConnectionCleanWorker offlineConnectionCleanWorker;
        private int CleanTick = 200;

        public CleanerReactor(double timeoutSeconds)
        {
            this.offlineConnectionCleanWorker= new OfflineConnectionCleanWorker(TimeSpan.FromSeconds(timeoutSeconds));
        }

        public void Start()
        {
            Task.Factory.StartNew(() =>
            {
                StartCleanWorker();
            }, TaskCreationOptions.LongRunning);
        }

        private void StartCleanWorker()
        {
            while (true)
            {
                offlineConnectionCleanWorker.DetectAndTagInactiveConnectionWorkers();

                offlineConnectionCleanWorker.CleanTaggedConnectionWorkers();

                Thread.Sleep(CleanTick);
            }
        }
    }
}

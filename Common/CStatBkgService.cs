using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace CStat.Common
{
    public class CStatBkgService : BackgroundService
    {
        private readonly IWorker worker;
        public CStatBkgService(IWorker worker)
        {
            this.worker = worker;
        }

        protected override async Task ExecuteAsync(CancellationToken cancelToken)
        {
            await worker.DoWork(cancelToken);
        }
    }
}

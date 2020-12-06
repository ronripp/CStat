using System.Threading;
using System.Threading.Tasks;

namespace CStat.Common
{
    public interface IWorker
    {
        Task DoWork(CancellationToken cancelToken);
    }
}
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface IItfCmdDataRepository
    {
        Task<int> EnqueueAsync(EIfCmd cmd,
            string data1, string data2, string data3, string data4,
            string data5, string data6, string data7, string data8, string data9, string data10,
            string requestSystem, CancellationToken cancellationToken = default(CancellationToken));
        Task<int> AcknowledgeAsync(EIfCmd cmd, string data1, CancellationToken cancellationToken = default(CancellationToken));
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Application.UseCases
{
    public class EnqueueCommandUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly IItfCmdDataRepository _repo;
        public EnqueueCommandUseCase(IUnitOfWork uow, IItfCmdDataRepository repo)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<bool> EnqueueAsync(EIfCmd cmd, string data1, string data2, string data3, string data4, string requestSystem, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var affected = await _repo.EnqueueAsync(cmd, data1, data2, data3, data4, requestSystem, ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch
            {
                throw;
            }
        }
    }
}

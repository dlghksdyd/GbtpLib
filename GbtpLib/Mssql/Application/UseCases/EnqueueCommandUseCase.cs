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
        private readonly IItfCmdDataRepository _cmdRepo;

        public EnqueueCommandUseCase(IUnitOfWork uow, IItfCmdDataRepository cmdRepo)
        {
            _uow = uow ?? throw new System.ArgumentNullException(nameof(uow));
            _cmdRepo = cmdRepo ?? throw new System.ArgumentNullException(nameof(cmdRepo));
        }

        public async Task<bool> EnqueueAsync(IfCmd cmd, string data1, string data2, string data3, string data4, string requestSystem, CancellationToken ct = default(CancellationToken))
        {
            await _uow.BeginAsync(ct).ConfigureAwait(false);
            try
            {
                var affected = await _cmdRepo.EnqueueAsync(cmd, data1, data2, data3, data4, requestSystem, ct).ConfigureAwait(false);
                await _uow.CommitAsync(ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch
            {
                await _uow.RollbackAsync(ct).ConfigureAwait(false);
                throw;
            }
        }
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Application.UseCases
{
    public class AcknowledgeCommandUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly IItfCmdDataRepository _cmdRepo;

        public AcknowledgeCommandUseCase(IUnitOfWork uow, IItfCmdDataRepository cmdRepo)
        {
            _uow = uow ?? throw new System.ArgumentNullException(nameof(uow));
            _cmdRepo = cmdRepo ?? throw new System.ArgumentNullException(nameof(cmdRepo));
        }

        public async Task<bool> AcknowledgeAsync(IfCmd cmd, string data1, CancellationToken ct = default(CancellationToken))
        {
            await _uow.BeginAsync(ct).ConfigureAwait(false);
            try
            {
                var affected = await _cmdRepo.AcknowledgeAsync(cmd, data1, ct).ConfigureAwait(false);
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

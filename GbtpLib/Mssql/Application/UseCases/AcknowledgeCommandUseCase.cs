using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Mssql.Domain;

namespace GbtpLib.Mssql.Application.UseCases
{
    public class AcknowledgeCommandUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly IItfCmdDataRepository _repo;
        public AcknowledgeCommandUseCase(IUnitOfWork uow, IItfCmdDataRepository repo)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<bool> AcknowledgeAsync(EIfCmd cmd, string data1, CancellationToken ct = default(CancellationToken))
        {
            await _uow.BeginAsync(ct).ConfigureAwait(false);
            try
            {
                var affected = await _repo.AcknowledgeAsync(cmd, data1, ct).ConfigureAwait(false);
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

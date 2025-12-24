using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Mssql.Domain;

namespace GbtpLib.Mssql.Application.UseCases
{
    public class CommandPollingUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly IItfCmdDataQueries _queries;
        private readonly IItfCmdDataRepository _repo;
        public CommandPollingUseCase(IUnitOfWork uow, IItfCmdDataQueries queries, IItfCmdDataRepository repo)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<bool> WaitForAndAcknowledgeAsync(EIfCmd cmdToWait, string data1, CancellationToken ct = default(CancellationToken))
        {
            await _uow.BeginAsync(ct).ConfigureAwait(false);
            try
            {
                var list = await _queries.GetPendingAsync(cmdToWait.ToString(), data1, ct).ConfigureAwait(false);
                if (list.Count == 0) { await _uow.RollbackAsync(ct).ConfigureAwait(false); return false; }
                var affected = await _repo.AcknowledgeAsync(cmdToWait, data1, ct).ConfigureAwait(false);
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

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

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

        public async Task<bool> WaitForAndAcknowledgeAsync(IfCmd cmdToWait, string data1, CancellationToken ct = default(CancellationToken))
        {
            // Single-shot: check once; caller can loop externally
            var pending = await _queries.GetPendingAsync(cmdToWait.ToString(), data1, ct).ConfigureAwait(false);
            if (pending.Count > 0)
            {
                await _uow.BeginAsync(ct).ConfigureAwait(false);
                try
                {
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
            return false;
        }
    }
}

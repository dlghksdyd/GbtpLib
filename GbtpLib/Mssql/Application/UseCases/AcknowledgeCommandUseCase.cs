using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Mssql.Domain;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Acknowledges queued interface commands by updating their status.
    /// <para>
    /// Return semantics:
    /// - <c>true</c>: At least one matching row was acknowledged (affected &gt; 0).
    /// - <c>false</c>: No matching rows found or updated.
    /// - Exceptions are propagated.
    /// </para>
    /// </summary>
    public class AcknowledgeCommandUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly IItfCmdDataRepository _repo;
        public AcknowledgeCommandUseCase(IUnitOfWork uow, IItfCmdDataRepository repo)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        /// <summary>
        /// Acknowledges a command for the given key.
        /// </summary>
        public async Task<bool> AcknowledgeAsync(EIfCmd cmd, string data1, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var affected = await _repo.AcknowledgeAsync(cmd, data1, ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch
            {
                throw;
            }
        }
    }
}

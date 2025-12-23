using System;
using System.Threading;
using System.Threading.Tasks;

namespace GbtpLib.Mssql.Application.Abstractions
{
    /// <summary>
    /// Application Layer transaction boundary abstraction.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Starts a new transaction scope. Nested calls may join the same underlying transaction depending on implementation.
        /// </summary>
        void Begin();

        /// <summary>
        /// Starts a new transaction scope asynchronously (may wrap a sync begin under .NET Framework).
        /// </summary>
        Task BeginAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Commits the current transaction.
        /// </summary>
        void Commit();

        /// <summary>
        /// Rolls back the current transaction.
        /// </summary>
        void Rollback();

        /// <summary>
        /// Async commit for implementations that support it.
        /// </summary>
        Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken));

        /// <summary>
        /// Async rollback for implementations that support it.
        /// </summary>
        Task RollbackAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}

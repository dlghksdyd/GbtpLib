using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Persistence.Abstractions;

namespace GbtpLib.Mssql.Persistence.UnitOfWork
{
    // Simple UoW that delegates to IAppDbContext transaction APIs
    public sealed class EfUnitOfWork : IUnitOfWork
    {
        private readonly IAppDbContext _db;
        private bool _active;
        private bool _disposed;

        public EfUnitOfWork(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(EfUnitOfWork));
        }

        public void Begin()
        {
            EnsureNotDisposed();
            if (_active) return;
            _db.BeginTransaction();
            _active = true;
        }

        public void Commit()
        {
            EnsureNotDisposed();
            if (!_active) return;
            _db.Commit();
            _active = false;
        }

        public void Rollback()
        {
            EnsureNotDisposed();
            if (!_active) return;
            _db.Rollback();
            _active = false;
        }

        public Task BeginAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            EnsureNotDisposed();
            if (cancellationToken.IsCancellationRequested)
            {
                var tcs = new TaskCompletionSource<object>();
                tcs.SetCanceled();
                return tcs.Task;
            }

            Begin();
            return Task.CompletedTask;
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            EnsureNotDisposed();
            if (!_active) return;
            await _db.CommitAsync(cancellationToken).ConfigureAwait(false);
            _active = false;
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            EnsureNotDisposed();
            if (!_active) return;
            await _db.RollbackAsync(cancellationToken).ConfigureAwait(false);
            _active = false;
        }

        public void Dispose()
        {
            if (_disposed) return;
            try
            {
                if (_active)
                {
                    try { _db.Rollback(); }
                    catch { /* swallow */ }
                    _active = false;
                }
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}

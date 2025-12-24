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
        private bool _ownsTransaction;

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

            // Reuse existing transaction if already started elsewhere
            if (_db.HasActiveTransaction)
            {
                _ownsTransaction = false;
                _active = true;
                return;
            }

            _db.BeginTransaction();
            _ownsTransaction = true;
            _active = true;
        }

        public void Commit()
        {
            EnsureNotDisposed();
            if (!_active) return;

            if (_ownsTransaction)
            {
                _db.Commit();
            }

            _active = false;
            _ownsTransaction = false;
        }

        public void Rollback()
        {
            EnsureNotDisposed();
            if (!_active) return;

            if (_ownsTransaction)
            {
                _db.Rollback();
            }

            _active = false;
            _ownsTransaction = false;
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

            if (_ownsTransaction)
            {
                await _db.CommitAsync(cancellationToken).ConfigureAwait(false);
            }

            _active = false;
            _ownsTransaction = false;
        }

        public async Task RollbackAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            EnsureNotDisposed();
            if (!_active) return;

            if (_ownsTransaction)
            {
                await _db.RollbackAsync(cancellationToken).ConfigureAwait(false);
            }

            _active = false;
            _ownsTransaction = false;
        }

        public void Dispose()
        {
            if (_disposed) return;
            try
            {
                if (_active)
                {
                    try
                    {
                        if (_ownsTransaction)
                        {
                            _db.Rollback();
                        }
                    }
                    catch { /* swallow */ }
                    finally
                    {
                        _active = false;
                        _ownsTransaction = false;
                    }
                }
            }
            finally
            {
                _disposed = true;
            }
        }
    }
}

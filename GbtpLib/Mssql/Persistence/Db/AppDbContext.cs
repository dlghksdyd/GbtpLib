using System;
using System.Data;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Abstractions;

namespace GbtpLib.Mssql.Persistence.Db
{
    // EF6 DbContext wrapper for .NET Framework 4.8
    public class AppDbContext : IAppDbContext, IDisposable
    {
        private readonly DbContext _context;
        private DbContextTransaction _transaction;

        public AppDbContext(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public DbContext Context { get { return _context; } }
        public DbContextTransaction CurrentTransaction { get { return _transaction; } }
        public bool HasActiveTransaction { get { return _transaction != null; } }

        public void BeginTransaction()
        {
            if (_transaction != null) return;
            _transaction = _context.Database.BeginTransaction();
        }

        public void BeginTransaction(IsolationLevel isolationLevel)
        {
            if (_transaction != null) return;
            _transaction = _context.Database.BeginTransaction(isolationLevel);
        }

        public void Commit()
        {
            if (_transaction == null) return;
            _context.SaveChanges();
            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;
        }

        public void Rollback()
        {
            if (_transaction == null) return;
            _transaction.Rollback();
            _transaction.Dispose();
            _transaction = null;
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_transaction == null) return;

            cancellationToken.ThrowIfCancellationRequested();
            await _context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            // Commit is synchronous in EF6 and not cancellable
            _transaction.Commit();
            _transaction.Dispose();
            _transaction = null;
        }

        public Task RollbackAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            Rollback();
            return Task.CompletedTask;
        }

        public int SaveChanges()
        {
            return _context.SaveChanges();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _context.SaveChangesAsync(cancellationToken);
        }

        public int ExecuteSqlCommand(string sql, params object[] parameters)
        {
            return _context.Database.ExecuteSqlCommand(sql, parameters);
        }

        public Task<int> ExecuteSqlCommandAsync(string sql, CancellationToken cancellationToken = default(CancellationToken), params object[] parameters)
        {
            return _context.Database.ExecuteSqlCommandAsync(sql, cancellationToken, parameters);
        }

        public DbSet<TEntity> Set<TEntity>() where TEntity : class
        {
            return _context.Set<TEntity>();
        }

        public int? CommandTimeout
        {
            get { return _context.Database.CommandTimeout; }
            set { _context.Database.CommandTimeout = value; }
        }

        public void Dispose()
        {
            if (_transaction != null)
            {
                try { _transaction.Dispose(); } catch { }
                _transaction = null;
            }
            if (_context != null)
            {
                try { _context.Dispose(); } catch { }
            }
        }
    }
}

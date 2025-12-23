using System;
using System.Data;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;

namespace GbtpLib.Mssql.Persistence.Abstractions
{
    public interface IAppDbContext : IDisposable
    {
        DbContext Context { get; }
        DbContextTransaction CurrentTransaction { get; }

        // Transaction APIs
        void BeginTransaction();
        void BeginTransaction(IsolationLevel isolationLevel);
        bool HasActiveTransaction { get; }
        void Commit();
        void Rollback();
        Task CommitAsync(CancellationToken cancellationToken = default(CancellationToken));
        Task RollbackAsync(CancellationToken cancellationToken = default(CancellationToken));

        // SaveChanges APIs
        int SaveChanges();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));

        // Raw SQL execution
        int ExecuteSqlCommand(string sql, params object[] parameters);
        Task<int> ExecuteSqlCommandAsync(string sql, CancellationToken cancellationToken = default(CancellationToken), params object[] parameters);

        // DbSet accessor
        DbSet<TEntity> Set<TEntity>() where TEntity : class;

        // Command timeout forwarding to Database.CommandTimeout
        int? CommandTimeout { get; set; }
    }
}

using System;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Logging;
using GbtpLib.Mssql.Persistence.Abstractions;
using GbtpLib.Mssql.Persistence.Entities;

namespace GbtpLib.Mssql.Application.UseCases
{
    // Use cases for system connection check commands using ITF_SYS_CON_CHECK
    public class SystemConnectionUseCases
    {
        private readonly IAppDbContextFactory _dbFactory;

        public SystemConnectionUseCases(IAppDbContextFactory dbFactory)
        {
            _dbFactory = dbFactory ?? throw new ArgumentNullException(nameof(dbFactory));
        }

        // Toggle system connection flag (0 <-> 1) and update response time
        public async Task<bool> ToggleConnectionAsync(string systemCode, CancellationToken ct = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(systemCode)) throw new ArgumentException("systemCode is required", nameof(systemCode));

            try
            {
                using (var uow = _dbFactory.Create())
                {
                    var set = uow.Set<ItfSysConCheckEntity>();
                    var entity = await set.FindAsync(systemCode).ConfigureAwait(false);
                    if (entity == null)
                    {
                        // If no record, initialize with 1
                        entity = new ItfSysConCheckEntity
                        {
                            SystemCode = systemCode,
                            ConnectionFlag = "1",
                            ResponseTime = DateTime.Now
                        };
                        set.Add(entity);
                    }
                    else
                    {
                        entity.ConnectionFlag = entity.ConnectionFlag == "1" ? "0" : "1";
                        entity.ResponseTime = DateTime.Now;
                    }

                    var affected = await uow.SaveChangesAsync(ct).ConfigureAwait(false);
                    return affected > 0;
                }
            }
            catch (Exception ex)
            {
                AppLog.Error($"SystemConnectionUseCases.ToggleConnectionAsync failed. systemCode={systemCode}", ex);
                throw;
            }
        }
    }
}

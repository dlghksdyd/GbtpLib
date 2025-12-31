using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Income-specific command helpers. Stored procedure calls have been centralized into StoredProcCommandUseCases.
    /// </summary>
    public class IncomeCommandUseCases
    {
        private readonly IStoredProcedureExecutor _sp; // kept for backward-compat constructor but unused

        public IncomeCommandUseCases(IStoredProcedureExecutor sp)
        {
            _sp = sp ?? throw new ArgumentNullException(nameof(sp));
        }

        // No SP methods here anymore. Use StoredProcCommandUseCases instead.
    }
}

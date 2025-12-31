using System;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Provides outcome-related interface commands not covered elsewhere.
    /// Stored procedure calls have been centralized into StoredProcCommandUseCases.
    /// </summary>
    public class OutcomeCommandUseCases
    {
        private readonly IStoredProcedureExecutor _sp; // kept for backward-compat constructor but unused
        public OutcomeCommandUseCases(IStoredProcedureExecutor sp)
        {
            _sp = sp ?? throw new ArgumentNullException(nameof(sp));
        }

        // No SP methods here anymore. Use StoredProcCommandUseCases instead.
    }
}

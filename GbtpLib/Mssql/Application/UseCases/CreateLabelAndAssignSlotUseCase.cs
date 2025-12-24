using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Mssql.Persistence.Entities;
using GbtpLib.Mssql.Domain;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Creates a label in MST_BTR and assigns it to a warehouse slot.
    /// <para>
    /// Steps:
    /// 1) Insert label into MST_BTR.
    /// 2) Update target INV_WAREHOUSE slot with label and grade.
    /// If step 2 fails, the created label is deleted as compensation.
    /// Return semantics:
    /// - <c>true</c>: Both steps succeeded (affected &gt; 0 in each).
    /// - <c>false</c>: Insert failed or slot update failed; compensation delete may occur.
    /// - Exceptions are propagated.
    /// </para>
    /// </summary>
    public class CreateLabelAndAssignSlotUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMstBtrRepository _btrRepo;
        private readonly IInvWarehouseRepository _whRepo;

        public CreateLabelAndAssignSlotUseCase(IUnitOfWork uow, IMstBtrRepository btrRepo, IInvWarehouseRepository whRepo)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _btrRepo = btrRepo ?? throw new ArgumentNullException(nameof(btrRepo));
            _whRepo = whRepo ?? throw new ArgumentNullException(nameof(whRepo));
        }

        /// <summary>
        /// Executes the create-and-assign flow.
        /// </summary>
        /// <param name="label">Label entity to insert.</param>
        /// <param name="slot">Target slot update parameters.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns><c>true</c> if both insert and update affected &gt; 0; otherwise <c>false</c>.</returns>
        public async Task<bool> ExecuteAsync(MstBtrEntity label, WarehouseSlotUpdateDto slot, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var ins = await _btrRepo.InsertAsync(label, ct).ConfigureAwait(false);
                if (ins < 1)
                {
                    return false;
                }

                var upd = await _whRepo.UpdateLabelAndGradeAsync(slot, ct).ConfigureAwait(false);
                if (upd < 1)
                {
                    // rollback label creation if slot update failed (logical compensation; outer tx will rollback too)
                    await _btrRepo.DeleteAsync(label.LabelId, ct).ConfigureAwait(false);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                AppLog.Error("CreateLabelAndAssignSlotUseCase.ExecuteAsync failed.", ex);
                throw;
            }
        }
    }
}

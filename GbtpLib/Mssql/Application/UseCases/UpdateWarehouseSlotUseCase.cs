using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Manages updates to warehouse slots by assigning or clearing labels and grades.
    /// <para>
    /// Return semantics for methods:
    /// - <c>true</c>: The underlying repository updated at least one record (affected &gt; 0).
    /// - <c>false</c>: No records were updated (affected == 0).
    /// - Exceptions are propagated; callers should wrap with transaction/compensation if needed.
    /// </para>
    /// </summary>
    public class UpdateWarehouseSlotUseCase
    {
        private readonly IInvWarehouseRepository _warehouseRepo;

        public UpdateWarehouseSlotUseCase(IInvWarehouseRepository warehouseRepo)
        {
            _warehouseRepo = warehouseRepo ?? throw new ArgumentNullException(nameof(warehouseRepo));
        }

        /// <summary>
        /// Assigns a label and grade to a warehouse slot.
        /// </summary>
        /// <param name="dto">Slot identification and new label/grade values.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns><c>true</c> if affected rows &gt; 0; otherwise <c>false</c>.</returns>
        public async Task<bool> SetLabelAsync(WarehouseSlotUpdateDto dto, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var affected = await _warehouseRepo.UpdateLabelAndGradeAsync(dto, ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch (Exception ex)
            {
                AppLog.Error("UpdateWarehouseSlotUseCase.SetLabelAsync failed.", ex);
                throw;
            }
        }

        /// <summary>
        /// Clears a label from a warehouse slot.
        /// </summary>
        /// <param name="key">Slot key information.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns><c>true</c> if affected rows &gt; 0; otherwise <c>false</c>.</returns>
        public async Task<bool> ClearLabelAsync(WarehouseSlotKeyDto key, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var affected = await _warehouseRepo.ClearLabelAsync(key, ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch (Exception ex)
            {
                AppLog.Error("UpdateWarehouseSlotUseCase.ClearLabelAsync failed.", ex);
                throw;
            }
        }
    }
}

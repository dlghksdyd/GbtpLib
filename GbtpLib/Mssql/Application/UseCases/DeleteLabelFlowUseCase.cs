using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Flow to delete a label and clear related warehouse slots.
    /// <para>
    /// Steps:
    /// 1) Delete label from MST_BTR.
    /// 2) Clear matching INV_WAREHOUSE slots referencing the label.
    /// Return semantics:
    /// - <c>true</c>: Label was deleted (affected &gt; 0) and clear operation executed.
    /// - <c>false</c>: Label did not exist or delete affected 0 rows; clear is skipped.
    /// - Exceptions are propagated.
    /// </para>
    /// </summary>
    public class DeleteLabelFlowUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMstBtrRepository _btrRepo;
        private readonly IInvWarehouseRepository _whRepo;

        public DeleteLabelFlowUseCase(IUnitOfWork uow, IMstBtrRepository btrRepo, IInvWarehouseRepository whRepo)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _btrRepo = btrRepo ?? throw new ArgumentNullException(nameof(btrRepo));
            _whRepo = whRepo ?? throw new ArgumentNullException(nameof(whRepo));
        }

        /// <summary>
        /// Executes the delete-and-clear flow.
        /// </summary>
        /// <param name="labelId">Label identifier to remove.</param>
        /// <param name="siteCode">Optional site code filter when clearing slots.</param>
        /// <param name="factoryCode">Optional factory code filter when clearing slots.</param>
        /// <param name="warehouseCode">Optional warehouse code filter when clearing slots.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <returns><c>true</c> when the label delete affected &gt; 0; otherwise <c>false</c>.</returns>
        public async Task<bool> ExecuteAsync(string labelId, string siteCode = null, string factoryCode = null, string warehouseCode = null, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var del = await _btrRepo.DeleteAsync(labelId, ct).ConfigureAwait(false);
                if (del < 1)
                {
                    return false;
                }

                await _whRepo.ClearLabelByLabelIdAsync(labelId, siteCode, factoryCode, warehouseCode, ct).ConfigureAwait(false);
                return true;
            }
            catch
            {
                throw;
            }
        }
    }
}

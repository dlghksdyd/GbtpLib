using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Application.Abstractions;
using GbtpLib.Mssql.Persistence.Entities;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Creates a label record in MST_BTR.
    /// <para>
    /// Return semantics: <c>true</c> when insert affected &gt; 0; otherwise <c>false</c>. Exceptions are propagated.
    /// </para>
    /// </summary>
    public class CreateLabelUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMstBtrRepository _btrRepo;

        public CreateLabelUseCase(IUnitOfWork uow, IMstBtrRepository btrRepo)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _btrRepo = btrRepo ?? throw new ArgumentNullException(nameof(btrRepo));
        }

        /// <summary>
        /// Creates the label.
        /// </summary>
        public async Task<bool> CreateAsync(MstBtrEntity entity, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var affected = await _btrRepo.InsertAsync(entity, ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch
            {
                throw;
            }
        }
    }

    /// <summary>
    /// Deletes a label record in MST_BTR.
    /// <para>
    /// Return semantics: <c>true</c> when delete affected &gt; 0; otherwise <c>false</c>. Exceptions are propagated.
    /// </para>
    /// </summary>
    public class DeleteLabelUseCase
    {
        private readonly IUnitOfWork _uow;
        private readonly IMstBtrRepository _btrRepo;

        public DeleteLabelUseCase(IUnitOfWork uow, IMstBtrRepository btrRepo)
        {
            _uow = uow ?? throw new ArgumentNullException(nameof(uow));
            _btrRepo = btrRepo ?? throw new ArgumentNullException(nameof(btrRepo));
        }

        /// <summary>
        /// Deletes the label.
        /// </summary>
        public async Task<bool> DeleteAsync(string labelId, CancellationToken ct = default(CancellationToken))
        {
            try
            {
                var affected = await _btrRepo.DeleteAsync(labelId, ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch
            {
                throw;
            }
        }
    }
}

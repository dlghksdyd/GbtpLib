using System;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Entities;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;
using GbtpLib.Logging;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Saves income/outcome inspection results into QLT_BTR_INOUT_INSP table.
    /// </summary>
    public class InoutInspectionSaveUseCases
    {
        private readonly IQltBtrInoutInspRepository _repo;
        public InoutInspectionSaveUseCases(IQltBtrInoutInspRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<bool> SaveAsync(InoutInspectionSaveDto dto, CancellationToken ct = default(CancellationToken))
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));
            try
            {
                var entity = new QltBtrInoutInspEntity
                {
                    SiteCode = dto.SiteCode,
                    FactoryCode = dto.FactoryCode,
                    ProcessCode = dto.ProcessCode,
                    MachineCode = dto.MachineCode,
                    InspKindGroupCode = dto.InspKindGroupCode,
                    InspKindCode = dto.InspKindCode,
                    LabelId = dto.LabelId,
                    InspectSeq = dto.InspectSeq.ToString(),
                    InspectValue = dto.InspectValueJson,
                    InspectResult = dto.InspectResultEnum.ToString(),
                    InspectStart = dto.InspectStart,
                    InspectEnd = dto.InspectEnd,
                    Note = dto.Note,
                    RegId = dto.RegId,
                    RegDateTime = dto.RegDateTime,
                };

                var affected = await _repo.InsertAsync(entity, ct).ConfigureAwait(false);
                return affected > 0;
            }
            catch (Exception ex)
            {
                AppLog.Error("InoutInspectionSaveUseCases.SaveAsync failed.", ex);
                throw;
            }
        }
    }
}

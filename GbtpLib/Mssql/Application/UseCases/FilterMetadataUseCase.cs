using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Logging;
using GbtpLib.Mssql.Domain;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Application.UseCases
{
    /// <summary>
    /// Provides dropdown lists for defect/outcome filter UIs.
    /// Methods return read-only lists; empty lists indicate no data; exceptions are propagated.
    /// </summary>
    public class FilterMetadataUseCase
    {
        private readonly IMetadataQueries _queries;

        public FilterMetadataUseCase(IMetadataQueries queries)
        {
            _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        }

        public async Task<IReadOnlyList<string>> GetGradeNamesAsync(CancellationToken ct = default)
        {
            try
            {
                var list = await _queries.GetGradeNamesAsync(ct).ConfigureAwait(false);
                return list;
            }
            catch (Exception ex)
            {
                AppLog.Error("FilterMetadataUseCase.GetGradeNamesAsync failed.", ex);
                throw;
            }
        }

        public async Task<IReadOnlyList<string>> GetCarMakeNamesAsync(CancellationToken ct = default)
        {
            try
            {
                var list = await _queries.GetCarMakeNamesAsync(ct).ConfigureAwait(false);
                return list;
            }
            catch (Exception ex)
            {
                AppLog.Error("FilterMetadataUseCase.GetCarMakeNamesAsync failed.", ex);
                throw;
            }
        }

        public async Task<IReadOnlyList<string>> GetCarNamesAsync(CancellationToken ct = default)
        {
            try
            {
                var list = await _queries.GetCarNamesAsync(ct).ConfigureAwait(false);
                return list;
            }
            catch (Exception ex)
            {
                AppLog.Error("FilterMetadataUseCase.GetCarNamesAsync failed.", ex);
                throw;
            }
        }

        public async Task<IReadOnlyList<string>> GetBatteryMakeNamesAsync(CancellationToken ct = default)
        {
            try
            {
                var list = await _queries.GetBatteryMakeNamesAsync(ct).ConfigureAwait(false);
                return list;
            }
            catch (Exception ex)
            {
                AppLog.Error("FilterMetadataUseCase.GetBatteryMakeNamesAsync failed.", ex);
                throw;
            }
        }

        public async Task<IReadOnlyList<string>> GetBatteryTypeNamesAsync(CancellationToken ct = default)
        {
            try
            {
                var list = await _queries.GetBatteryTypeNamesAsync(ct).ConfigureAwait(false);
                return list;
            }
            catch (Exception ex)
            {
                AppLog.Error("FilterMetadataUseCase.GetBatteryTypeNamesAsync failed.", ex);
                throw;
            }
        }

        public async Task<IReadOnlyList<string>> GetReleaseYearsAsync(CancellationToken ct = default)
        {
            try
            {
                var list = await _queries.GetReleaseYearsAsync(ct).ConfigureAwait(false);
                return list;
            }
            catch (Exception ex)
            {
                AppLog.Error("FilterMetadataUseCase.GetReleaseYearsAsync failed.", ex);
                throw;
            }
        }

        public async Task<IReadOnlyList<BatteryTypeYearDto>> GetBatteryTypesWithYearAsync(CancellationToken ct = default)
        {
            try
            {
                var list = await _queries.GetBatteryTypesWithYearAsync(ct).ConfigureAwait(false);
                return list;
            }
            catch (Exception ex)
            {
                AppLog.Error("FilterMetadataUseCase.GetBatteryTypesWithYearAsync failed.", ex);
                throw;
            }
        }
    }
}

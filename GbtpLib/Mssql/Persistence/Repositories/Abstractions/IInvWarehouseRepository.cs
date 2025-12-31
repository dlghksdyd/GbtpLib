using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Domain;

namespace GbtpLib.Mssql.Persistence.Repositories.Abstractions
{
    public interface IInvWarehouseRepository
    {
        Task<int> UpdateLabelAndGradeAsync(WarehouseSlotUpdateDto dto, CancellationToken cancellationToken = default(CancellationToken));
        Task<int> ClearLabelAsync(WarehouseSlotKeyDto key, CancellationToken cancellationToken = default(CancellationToken));
        Task<int> ClearLabelByLabelIdAsync(string labelId, string siteCode = null, string factoryCode = null, string warehouseCode = null, CancellationToken cancellationToken = default(CancellationToken));
        Task<int> UpdateStoreDivAsync(int row, int col, int lvl, string warehouseCode, string storeDiv, CancellationToken cancellationToken = default(CancellationToken));
    }
}

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GbtpLib.Mssql.Persistence.Abstractions;
using GbtpLib.Mssql.Persistence.Entities;
using GbtpLib.Mssql.Persistence.Repositories.Abstractions;

namespace GbtpLib.Mssql.Persistence.Repositories
{
    public class MstCodeRepository : IMstCodeRepository
    {
        private readonly IAppDbContext _db;
        public MstCodeRepository(IAppDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<string> GetCodeAsync(string group, string codeName, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var code = await _db.Set<MstCodeEntity>()
                .Where(x => x.CodeGroup == group && x.CodeName == codeName)
                .Select(x => x.Code)
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);

            return code ?? string.Empty;
        }

        public async Task<IReadOnlyList<string>> GetNamesAsync(string group, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var names = await _db.Set<MstCodeEntity>()
                .Where(x => x.CodeGroup == group && x.UseYn == "Y")
                .OrderBy(x => x.ListOrder)
                .Select(x => x.CodeName)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return names;
        }
    }
}

namespace GbtpLib.Mssql.Persistence.Abstractions
{
    public interface IAppDbContextFactory
    {
        IAppDbContext Create();
    }
}

namespace Chinook.IServices
{
    public interface IDBContextService
    {
        Task<ChinookContext> GetContextAsync();
    }
}

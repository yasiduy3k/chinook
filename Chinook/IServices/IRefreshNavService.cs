namespace Chinook.IServices
{
    public interface IRefreshNavService
    {
        event Action RefreshRequested;
        void CallRequestRefresh();
    }
}

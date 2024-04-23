using Chinook.IServices;

namespace Chinook.Services
{
    public class RefreshNavService : IRefreshNavService
    {
        public event Action RefreshRequested;
        /// <summary>
        /// Method for invoking the parent event of subscriber.
        /// </summary>
        public void CallRequestRefresh()
        {
            RefreshRequested?.Invoke();
        }
    }
}

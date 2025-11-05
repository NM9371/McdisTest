using McdisTest.Models;

namespace McdisTest.Data
{
    public interface IUserEventStatsStorage
    {
        Task SaveStatsAsync(UserEvent userEvent);
        Task CreateStatsTableAsync();
    }
}

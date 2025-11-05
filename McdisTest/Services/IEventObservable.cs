using McdisTest.Models;

namespace McdisTest.Services
{
    public interface IEventObservable : IObservable<UserEvent>
    {
        void Publish(UserEvent userEvent);
        void Complete();
    }
}

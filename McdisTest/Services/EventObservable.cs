using McdisTest.Data;
using System.Reactive.Subjects;

namespace McdisTest.Services
{
    public class EventObservable : IObservable<UserEvent>
    {
        private readonly Subject<UserEvent> _subject = new Subject<UserEvent>();

        public IDisposable Subscribe(IObserver<UserEvent> observer)
        {
            return _subject.Subscribe(observer);
        }

        public void Publish(UserEvent userEvent)
        {
            _subject.OnNext(userEvent);
        }

        public void Complete()
        {
            _subject.OnCompleted();
        }
    }
}

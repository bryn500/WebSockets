using System.Reactive.Subjects;

namespace WebSockets.Hubs
{
    public class TimerBackgroundService : BackgroundService
    {
        private readonly Subject<string> _subject = new Subject<string>();

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _subject.OnNext(DateTime.Now.ToString("o"));
                await Task.Delay(1000);
            }
        }

        public IObservable<string> StreamDate()
        {
            return _subject;
        }
    }
}

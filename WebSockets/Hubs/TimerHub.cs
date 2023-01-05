using Microsoft.AspNetCore.SignalR;
using System.Threading.Channels;
using WebSockets.Extensions;

namespace WebSockets.Hubs
{
    public class TimerHub : Hub
    {
        private readonly TimerBackgroundService _timerService;
        public TimerHub(TimerBackgroundService weatherBackgroundService)
        {
            _timerService = weatherBackgroundService;
        }

        public ChannelReader<string> StreamDate(CancellationToken cancellationToken)
        {
            return _timerService.StreamDate().AsChannelReader(cancellationToken);
        }
    }
}

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChannelSample
{
    public class ConsumerService : BackgroundService
    {
        private readonly MessageQueue _queue;
        private readonly Watch _watch;
        public ConsumerService(Watch watch, MessageQueue queue)
        {
            _watch = watch;
            _queue = queue;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            await foreach (var message in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                Console.WriteLine($"Event: {message.Id} => {message.Value}");
            }
            
            _watch.Stop();

            var totalMicroseconds = _watch.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
            var avgOneItemDelivery = totalMicroseconds / 1000000;

            Console.WriteLine($"totalMicroseconds = {totalMicroseconds} and avgOneItemDelivery = {avgOneItemDelivery}");

            Console.ReadLine();
        }
    }
}
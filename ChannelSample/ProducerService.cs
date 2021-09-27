using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ChannelSample
{
    public class ProducerService : BackgroundService
    {
        private readonly MessageQueue _messageQueue;
        private readonly Watch _watch;

        public ProducerService(Watch watch, MessageQueue queue )
        {
            _messageQueue = queue;
            _watch = watch;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Yield();

            var memory = new Memory<byte>(new byte[12]);
            _watch.Start();

            var iterationCounts = 1000001;
            for (var i = 0; i < iterationCounts; i++)
            {
                MemoryMarshal.Write(memory.Span, ref i);
                var sampleEvent = new SampleEvent()
                {
                    Id = MemoryMarshal.Read<int>(memory.Span),
                    Value = MemoryMarshal.Read<double>(memory.Span.Slice(4))
                };

                await _messageQueue.WriteMessagesAsync(new List<SampleEvent>() { sampleEvent }, stoppingToken);
            }

            _messageQueue.TryCompleteWriter();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ChannelSample
{
    public class MessageQueue 
    {
        private const int MaxMessagesInChannel = 1024;

        private readonly Channel<SampleEvent> _channel;
        public MessageQueue()
        {
            _channel = Channel.CreateBounded<SampleEvent>(new BoundedChannelOptions(MaxMessagesInChannel)
            {
                SingleWriter = true,
                SingleReader = true
            });
        }

        public ChannelReader<SampleEvent> Reader => _channel.Reader;

        public async Task WriteMessagesAsync(IList<SampleEvent> messages, CancellationToken cancellationToken = default)
        {
            var index = 0;

            while (index < messages.Count && await _channel.Writer.WaitToWriteAsync(cancellationToken))
            {
                while (index < messages.Count && _channel.Writer.TryWrite(messages[index]))
                {
                   
                    index++;
                }
            }
        }

        public void CompleteWriter(Exception ex = null) => _channel.Writer.Complete(ex);

        public bool TryCompleteWriter(Exception ex = null) => _channel.Writer.TryComplete(ex);
        
    }
}
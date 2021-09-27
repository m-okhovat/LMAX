using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Disruptor;
using Disruptor.Dsl;

namespace DisruptorSample
{
    class Program
    {
        static void Main(string[] args)
        {

            // Specify the size of the ring buffer, must be power of 2.
            const int bufferSize = 1024;

            // Construct the Disruptor
            var disruptor = new Disruptor<SampleEvent>(() => new SampleEvent(), bufferSize);

            // Connect the handler
            disruptor.HandleEventsWith(new SampleEventHandler());

            // Start the Disruptor, starts all threads running
            disruptor.Start();

            // Get the ring buffer from the Disruptor to be used for publishing.
            var ringBuffer = disruptor.RingBuffer;

            var producer = new SampleEventProducer(ringBuffer);
                var memory = new Memory<byte>(new byte[12]);
            var stopwatch = Stopwatch.StartNew();
            stopwatch.Start();
            var iterationCounts = 1000001;
            for (var i = 0;i <iterationCounts ; i++)
            {
                MemoryMarshal.Write(memory.Span, ref i);

                producer.ProduceUsingRawApi(memory);

            }
            stopwatch.Stop();
            var totalMicroseconds = stopwatch.ElapsedTicks / (Stopwatch.Frequency / (1000L * 1000L));
            var avgOneItemDelivery = totalMicroseconds / iterationCounts;

            Console.WriteLine($"totalMicroseconds = {totalMicroseconds} and avgOneItemDelivery = {avgOneItemDelivery}");

            Console.ReadLine();
        }
    }
    
    public class SampleEvent
    {
        public int Id { get; set; }
        public double Value { get; set; }
    }

    public class SampleEventHandler : IEventHandler<SampleEvent>
    {
        public void OnEvent(SampleEvent data, long sequence, bool endOfBatch)
        {
           Console.WriteLine($"Event: {data.Id} => {data.Value}");
        }
    }

    public class SampleEventProducer
    {
        private readonly RingBuffer<SampleEvent> _ringBuffer;

        public SampleEventProducer(RingBuffer<SampleEvent> ringBuffer)
        {
            _ringBuffer = ringBuffer;
        }

        public void ProduceUsingRawApi(ReadOnlyMemory<byte> input)
        {
            // Grab the next sequence
            var sequence = _ringBuffer.Next();
            try
            {
                // Get the event in the Disruptor for the sequence
                var data = _ringBuffer[sequence];

                // Fill with data
                data.Id = MemoryMarshal.Read<int>(input.Span);
                data.Value = MemoryMarshal.Read<double>(input.Span.Slice(4));
            }
            finally
            {
                // Publish the event
                _ringBuffer.Publish(sequence);
            }
        }

        public void ProduceUsingScope(ReadOnlyMemory<byte> input)
        {
            using (var scope = _ringBuffer.PublishEvent())
            {
                var data = scope.Event();

                // Fill with data
                data.Id = MemoryMarshal.Read<int>(input.Span);
                data.Value = MemoryMarshal.Read<double>(input.Span.Slice(4));

                // The event is published at the end of the scope
            }
        }
    }
}

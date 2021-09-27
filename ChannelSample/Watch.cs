using System.Diagnostics;

namespace ChannelSample
{
    public class Watch
    {
        private readonly Stopwatch _stopwatch;

        public Watch()
        {
            _stopwatch = new Stopwatch();
        }

        public long ElapsedTicks => _stopwatch.ElapsedTicks;
        public void Start()
        {

            _stopwatch.Start();
        }

        public void Stop()
        {
            _stopwatch.Stop();
        }
    }
}
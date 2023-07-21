using System;
using System.Diagnostics;

namespace SBA
{
    public class Benchmark
    {
        private readonly Stopwatch stopwatch;
        private TimeSpan elapsed;

        public Benchmark()
        {
            stopwatch = new Stopwatch();
            elapsed = TimeSpan.Zero;
        }

        public void Start()
        {
            stopwatch.Restart();
        }

        public void Stop()
        {
            stopwatch.Stop();
            elapsed += stopwatch.Elapsed;
        }

        public void Reset()
        {
            stopwatch.Reset();
            elapsed = TimeSpan.Zero;
        }

        public TimeSpan Elapsed => elapsed;

        public long ElapsedMilliseconds => (long)elapsed.TotalMilliseconds;

        public long ElapsedTicks => elapsed.Ticks;

        public void PrintElapsedTime(string label = "Elapsed Time")
        {
            Console.WriteLine($"{label}: {elapsed}");
        }
    }
}

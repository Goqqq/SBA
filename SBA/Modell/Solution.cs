using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBA;

public class Solution
{
    public int ID { get; init; }
    public int arrivalTime { get; init; }
    public int departureTime { get; init; }
    public int startPosition { get; init; }
    public int endPosition { get; init; }
    public List<Benchmark> benchmarks { get; init; }

    public Solution(
        int ID,
        int arrivalTime,
        int departureTime,
        int startPosition,
        int endPosition,
        List<Benchmark> benchmarks
    )
    {
        try
        {
            if (arrivalTime < 0)
            {
                throw new ArgumentException("Arrival time cannot have a negative value");
            }
            else if (departureTime < 0)
            {
                throw new ArgumentException("Departure time cannot have a negative value");
            }
            else if (startPosition < 0)
            {
                throw new ArgumentException("Start position cannot have a negative value");
            }
            else if (endPosition < 0)
            {
                throw new ArgumentException("End position cannot have a negative value");
            }

            this.ID = ID;
            this.arrivalTime = arrivalTime;
            this.departureTime = departureTime;
            this.startPosition = startPosition;
            this.endPosition = endPosition;
            this.benchmarks = benchmarks;
        }
        catch (ArgumentException ex)
        {
            // Display an error message to the user or log the error for debugging
            MessageBox.Show(
                $"{ex.Message}. {ex.StackTrace}",
                "Error",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            Environment.Exit(1);
            // You can also choose to re-throw the exception to propagate it further up the call stack
            // throw;
        }
    }

    //public TimeSpan GetBenchmarkTime()
    //{
    //    //TimeSpan sum = TimeSpan.Zero;
    //    //foreach (Benchmark bm in benchmarks)
    //    //{
    //    //    sum += bm.Elapsed;
    //    //}
    //    //return sum;
    //    return benchmarks.Aggregate(TimeSpan.Zero, (sum, current));
    //}
}

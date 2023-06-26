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

    public Solution(int ID, int arrivalTime, int departureTime, int startPosition, int endPosition)
    {
        this.ID = ID;
        this.arrivalTime = arrivalTime;
        this.departureTime = departureTime;
        this.startPosition = startPosition;
        this.endPosition = endPosition;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBA;

public class Quay
{
    private int _capacity;
    public int Capacity
    {
        get { return _capacity; }
        init
        {
            try
            {
                if (value < 0)
                {
                    throw new ArgumentException("Quay cannot have a negative capacity");
                }
                else
                {
                    _capacity = value;
                }
            }
            catch (ArgumentException)
            {
                throw;
            }
        }
    }

    public Quay(int capacity)
    {
        Capacity = capacity;
    }

    public override string ToString()
    {
        return $"Quay Details: {Capacity}";
    }
}

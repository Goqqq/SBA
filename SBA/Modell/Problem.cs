using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBA.Modell;

public class Problem
{
    public Quay Quay { get; init; }
    public List<Ship> Ships { get; set; }
    private static UniqueIDGenerator generator = new UniqueIDGenerator();

    public Problem(Quay quay, List<Ship> ships)
    {
        Quay = quay;
        Ships = new List<Ship>();
        setShips(ships);
    }

    private void setShips(List<Ship> ships)
    {
        for (int i = ships.Count - 1; i >= 0; i--)
        {
            Ship ship = ships[i];
            if (ship.Size <= Quay.Capacity)
            {
                ship.ID = generator.GenerateID();
                Ships.Add(ship);
            }
            else
            {
                ships.RemoveAt(i);
            }
        }
    }
}

public class UniqueIDGenerator
{
    public static int counter;

    public int GenerateID()
    {
        int uniqueID = counter;
        counter++;
        return uniqueID;
    }

    public static void ResetCounter()
    {
        counter = 0;
    }
}

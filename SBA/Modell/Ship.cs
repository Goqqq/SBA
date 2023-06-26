using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBA;

public class Ship
{
    public int ID { get; init; }
    private int _size;
    public int Size
    {
        get { return _size; }
        init
        {
            try
            {
                if (value < 0)
                {
                    throw new ArgumentException("ship cannot have a negative size");
                }
                else
                {
                    _size = value;
                }
            }
            catch (ArgumentException)
            {
                throw;
            }
        }
    } //Länge(l_i)
    private int _handlingTime;
    public int HandlingTime //Liegezeit(t_i)
    {
        get { return _handlingTime; }
        init
        {
            try
            {
                if (value < 0)
                {
                    throw new ArgumentException("handling time cannot have a negative value");
                }
                else
                {
                    _handlingTime = value;
                }
            }
            catch (ArgumentException)
            {
                throw;
            }
        }
    }
    private static UniqueIDGenerator generator = new UniqueIDGenerator();

    public Ship(int handlingTime, int size)
    {
        try
        {
            HandlingTime = handlingTime;
            Size = size;
            ID = generator.GenerateID();
        }
        catch (ArgumentException)
        {
            throw;
        }
    }

    public override string ToString()
    {
        return $"Ship Details: ID={ID}, HandlingTime={HandlingTime}, Size={Size}";
    }

    public static List<Ship> DeepCopy(List<Ship> originalList)
    {
        List<Ship> copyList = new List<Ship>();

        foreach (Ship ship in originalList)
        {
            Ship copyShip = new Ship(ship.HandlingTime, ship.Size) { ID = ship.ID, };

            copyList.Add(copyShip);
        }

        return copyList;
    }
}

public class UniqueIDGenerator
{
    private static int counter;

    public int GenerateID()
    {
        int uniqueID = counter;
        counter++;
        return uniqueID;
    }
}

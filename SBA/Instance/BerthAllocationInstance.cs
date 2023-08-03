using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SBA;

public class BerthAllocationInstance
{
    public Ship[] Ships { get; set; }
    public Quay Quay { get; set; }

    public static BerthAllocationInstance ReadInstances(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);

        int numberOfShips = int.Parse(lines[0].Split(":")[1]);

        Ship[] ships = new Ship[numberOfShips];
        for (int i = 0; i < numberOfShips; i++)
        {
            string[] shipData = lines[i + 1].Split(", ");
            int IDname = int.Parse(shipData[0].Split(":")[1]);
            int size = int.Parse(shipData[1].Split(":")[1]); // Corrected index to get the size
            int handlingTime = int.Parse(shipData[2].Split(":")[1]);

            ships[i] = new Ship(IDname, handlingTime, size);
        }

        int quayCapacity = int.Parse(lines[numberOfShips + 1].Split(":")[1]);
        Quay quay = new Quay(quayCapacity);

        var instance = new BerthAllocationInstance { Ships = ships, Quay = quay };

        return instance;
    }

    public static void GenerateInstances(
        int[] numberOfShipsValues,
        int minSize,
        int maxSize,
        int minHandlingTime,
        int maxHandlingTime,
        int[] berthLengthValues,
        int numberOfInstancesPerValue,
        int seed
    )
    {
        Random random = new Random(seed);

        string dataDirectory = Path.Combine(
            Environment.CurrentDirectory,
            "C:/Users/Torben/source/repos/SBA/SBA/Instance/Data"
        );

        foreach (int numberOfShips in numberOfShipsValues)
        {
            foreach (int quayCapacity in berthLengthValues)
            {
                for (
                    int instanceIndex = 1;
                    instanceIndex <= numberOfInstancesPerValue;
                    instanceIndex++
                )
                {
                    Ship[] ships = new Ship[numberOfShips];
                    for (int i = 0; i < numberOfShips; i++)
                    {
                        int capacity = random.Next(minSize, maxSize + 1);
                        int handlingTime = random.Next(minHandlingTime, maxHandlingTime + 1);

                        ships[i] = new Ship(i + 1, capacity, handlingTime);
                    }

                    Quay quay = new Quay(quayCapacity);

                    var instance = new BerthAllocationInstance { Ships = ships, Quay = quay };

                    string fileName =
                        $"instance_{numberOfShips}_{quayCapacity}_{instanceIndex}.txt";
                    string filePath = Path.Combine(dataDirectory, fileName);
                    instance.SaveToFile(filePath);
                }
            }
        }
    }

    public void SaveToFile(string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine($"Ships:{Ships.Length}");
            int IDname = 1;
            foreach (Ship ship in Ships)
            {
                writer.WriteLine($"ID:{IDname++}, Size:{ship.Size}, Handling:{ship.HandlingTime}");
            }
            writer.WriteLine($"QuayCapacity:{Quay.Capacity}");
        }
    }
}

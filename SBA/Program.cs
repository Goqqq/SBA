using SBA.Modell;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SBA;

internal static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    public static void Main(String[] args)
    {
        Quay quay = new Quay(20);
        List<Ship> ships = new();
        Ship ship1 = new(4, 6);
        Ship ship2 = new(2, 3);
        Ship ship3 = new(3, 4);
        Ship ship4 = new(5, 5);
        Ship ship5 = new(6, 4);
        Ship ship6 = new(7, 7);
        //Ship ship6 = new(6, 5);
        ships.Add(ship1);
        ships.Add(ship2);
        ships.Add(ship3);
        ships.Add(ship4);
        ships.Add(ship5);
        ships.Add(ship6);
        Problem problem = new(quay, ships);
        //ships.Add(ship6);
        Allocator allocator = new(problem.Ships, problem.Quay);
        allocator.AllocateShips();
        allocator.PrintSolution();
        List<Solution> solution = allocator.GetSolution();

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new SBA(solution, quay.Capacity));
    }
}

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
        Quay quay = new Quay(10);
        List<Ship> ships = new();
        Ship ship1 = new(4, 6);
        Ship ship2 = new(2, 3);
        Ship ship3 = new(3, 4);
        Ship ship4 = new(5, 5);
        Ship ship5 = new(6, 4);
        ships.Add(ship1);
        ships.Add(ship2);
        ships.Add(ship3);
        ships.Add(ship4);
        ships.Add(ship5);
        Allocator allocator = new(ships, quay);
        allocator.AllocateShips();
        allocator.PrintSolution();
        List<Solution> solution = allocator.GetSolution();

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new SBA(solution, quay.Capacity));

        // Create and show the form
        //Application.SetCompatibleTextRenderingDefault(false);
        //shipForm = new ShipForm(ships, quay);
        //Application.Run(shipForm);
        //SBA form1 = new();
        //SBA.Show();
        //Application.Run(form1);
    }
}

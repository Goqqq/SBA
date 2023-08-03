using ScottPlot.Drawing.Colormaps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Gurobi;
using SBA.Modell;
using System.Security.Cryptography.X509Certificates;

namespace SBA;


public class GurobiOptimizer
{
    private Problem problem;

    public GurobiOptimizer(Problem problem)
    {
        this.problem = problem;
    }

    public List<Solution> Solve()
    {    
        GRBEnv env = new GRBEnv();
        GRBModel model = new GRBModel(env);

        model.Set(GRB.StringAttr.ModelName, "SBA");
        model.Set(GRB.DoubleParam.TimeLimit, 300);

        int n = problem.Ships.Count;
        int M = 10000;
        List<Solution> solutions = new List<Solution>();

        // Define variables
        GRBVar[] s = new GRBVar[n];   // Arrival time of ships
        GRBVar[] b = new GRBVar[n];   // Berthing position of ships
        GRBVar[,] x = new GRBVar[n, n];  // Binary variable for the time relation between two ships
        GRBVar[,] y = new GRBVar[n, n];  // Binary variable for the spatial relation between two ships
        
        for (int i = 0; i < n; i++)
        {
            s[i] = model.AddVar(0, GRB.INFINITY, 0, GRB.CONTINUOUS, "s_" + i);
            b[i] = model.AddVar(0, problem.Quay.Capacity - problem.Ships[i].Size, 0, GRB.CONTINUOUS, "b_" + i);

            for (int j = 0; j < n; j++)
            {
                x[i, j] = model.AddVar(0, 1, 0, GRB.BINARY, "x_" + i + "_" + j);
                y[i, j] = model.AddVar(0, 1, 0, GRB.BINARY, "y_" + i + "_" + j);
            }
        }

        // Set objective: minimize the sum of departure times
        GRBLinExpr objective = new GRBLinExpr();
        for (int i = 0; i < n; i++)
        {
            objective += s[i] + problem.Ships[i].HandlingTime;
        }
        model.SetObjective(objective, GRB.MINIMIZE);
        // model.Set(GRB.IntAttr.ModelSense, GRB.MINIMIZE);

        // Add constraints
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                if (i != j)
                {
                    // Time constraint
                    GRBConstr timeConstraint = model.AddConstr(s[i] + problem.Ships[i].HandlingTime <= s[j] + M * (1 - x[i, j]), "time_" + i + "_" + j);

                    // Space constraint
                    GRBConstr spaceConstraint = model.AddConstr(b[i] + problem.Ships[i].Size <= b[j] + M * (1 - y[i, j]), "space_" + i + "_" + j);

                    // Separation constraint
                    GRBConstr separationConstraint = model.AddConstr(x[i, j] + x[j, i] + y[i, j] + y[j, i] >= 1, "sep_" + i + "_" + j);
                }
            }

            // Berth length constraint
            model.AddConstr(b[i] >= 0, "berthLower_" + i);
            model.AddConstr(b[i] <= problem.Quay.Capacity - problem.Ships[i].Size, "berthUpper_" + i);

            // Arrival time constraint
            model.AddConstr(s[i] >= 0, "arrival_" + i);
        }

        // Optimize model
        model.Update();
        model.Optimize();

        // Print solution
        for (int i = 0; i < n; i++)
        {
            int arrivalTime = (int)s[i].X;
            int departureTime = arrivalTime + problem.Ships[i].HandlingTime;
            int startPosition = (int)b[i].X;
            int endPosition = startPosition + problem.Ships[i].Size;

            Solution solution = new Solution(
                ID: i,
                arrivalTime: arrivalTime,
                departureTime: departureTime,
                startPosition: startPosition,
                endPosition: endPosition,
                benchmarks: new List<Benchmark>()
            );
            solutions.Add(solution);
        }

        foreach (Solution sol in solutions)
        {
            Debug.WriteLine($"Solution for ship {sol.ID}:");
            Debug.WriteLine($"Arrival time: {sol.arrivalTime}");
            Debug.WriteLine($"Departure time: {sol.departureTime}");
            Debug.WriteLine($"Start position: {sol.startPosition}");
            Debug.WriteLine($"End position: {sol.endPosition}");
        }

        // Dispose of model and env
        model.Dispose();
        env.Dispose();

        return solutions;
    }
}
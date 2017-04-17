using System;
using System.IO;
namespace SavingsMethod
{
    /// <summary>
    /// Describes a solution to a CVRP 
    /// </summary>
    public class CVRPSolution
    {

        /// <summary>
        /// The quality of the best-known solution.
        /// </summary>
        public double TotalCost { get; set; }

        /// <summary>
        /// Descibes the actual tours in the solution
        /// </summary>
        public int[][] Solution { get; set; }

        internal void WriteToFile(string p, TimeSpan ts)
        {
            StreamWriter file = new StreamWriter(p);
            for (int i = 0; i < Solution.Length; i++)
            {
                file.Write("Route #" + i + ":");
                for (int j = 0; j < Solution[i].Length; j++)
                {
                    file.Write(" " + Solution[i][j]);
                }
                file.WriteLine();
            }
            file.WriteLine("Cost " + TotalCost);
            file.WriteLine("Time " + ts.Milliseconds);
            file.Close();
        }
    }
}


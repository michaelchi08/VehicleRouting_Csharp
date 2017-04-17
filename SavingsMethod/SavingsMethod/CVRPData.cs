#region License Information
/* Adapted by Stefan S. in Fall 2012
 * 
 * HeuristicLab
 * Copyright (C) 2002-2012 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
 *
 * This file is part of HeuristicLab.
 *
 * HeuristicLab is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * HeuristicLab is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with HeuristicLab. If not, see <http://www.gnu.org/licenses/>.
 */
#endregion

using System;

namespace SavingsMethod
{
    /// <summary>
    /// Describes instances of the Vehicle Routing Problem (VRP).
    /// </summary>
    public class CVRPData
    {
        private TSPLIBParser myParser;

        public CVRPData(TSPLIBParser myParser)
        {
            this.myParser = myParser;
            BestKnownQuality = myParser.BestKnown;
            Capacity = myParser.Capacity;
            MaxLength = myParser.MaxLength; // Add by Will&Ying 10252012
            Coordinates = myParser.Vertices;
            Demands = myParser.Demands;
            Dimension = myParser.Dimension;
            Name = myParser.Name;

            CalculateDistanceMatrix();

           
            Validate();
        }

        /// <summary>
        /// The name of the instance
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The number of customers and the depot
        /// </summary>
        public int Dimension { get; private set; }

        /// <remarks>
        /// Distances computed from coordinates
        /// </remarks>
        public double[,] Distances { get; private set; }

        /// <summary>
        /// Coordinates read from file
        /// </remarks>
        private double[,] Coordinates { get; set; }

        /// <summary>
        /// The demand vector that specifies how many goods need to be delivered.
        /// The vector has to include the depot, but with a demand of 0.
        /// </summary>
        public double[] Demands { get; private set; }

        /// <summary>
        /// The capacity of the vehicles, which is the same for all (homogeneous fleet).
        /// </summary>
        public double Capacity { get; private set; }

        /// <summary>
        /// The total route length, which is the same for all (homogeneous fleet).
        /// Add by Will and Ying
        /// </summary>
        public double MaxLength { get; private set; }

        /// <summary>
        /// Optional! The quality of the best-known solution.
        /// </summary>
        public double? BestKnownQuality { get; private set; }

        /// <summary>
        /// Need to run in order to get the distance matrix
        /// </summary>
        public void CalculateDistanceMatrix()
        {
            Distances = new double[Dimension, Dimension];
            for (int i = 0; i < Dimension; i++)
            {
                for (int j = i + 1; j < Dimension; j++)
                {
                    Distances[i, j] = Math.Sqrt((Coordinates[i, 1] - Coordinates[j, 1]) * (Coordinates[i, 1] - Coordinates[j, 1]) + (Coordinates[i, 0] - Coordinates[j, 0]) * (Coordinates[i, 0] - Coordinates[j, 0]));
                    Distances[j, i] = Distances[i, j];
                }
            }
        }

        /// <summary>
        /// Makes sure data makes sense
        /// </summary>
        internal void Validate()
        {
            if (Dimension < 1)
                throw new Exception("Dimension < 1");

            if (Distances.Length != Dimension * Dimension)
                throw new Exception("Distances.Length != Dimension * Dimension");

            if (Demands.Length != Dimension)
                throw new Exception("Demands.Length != Dimension");

            for (int i = 0; i < Dimension; i++)
            {
                if (Demands[i] < 0)
                    throw new Exception("Demands[" + i + "] < 0");

                if (Demands[i] == 0 && i > 0)
                    Console.WriteLine("WARNING: Demands[" + i + "] == 0");

                if (Demands[i] > Capacity)
                    throw new Exception("Demands[" + i + "] > Capacity");

                for (int j = i + 1; j < Dimension; j++)
                {
                    if (Distances[i, j] == 0)
                        Console.WriteLine("WARNING: Distances[" + i + "," + j + "] == 0");
                }
            }

            if (Capacity <= 0)
                throw new Exception("Capacity <= 0");

            Console.WriteLine("Data Validated");
        }
    }

    /// <summary>
    /// Object that represents a tour: the node sequence and the total demand.
    /// </summary>
    public class VTour
    {
        /// <summary>
        /// The tour itself
        /// </summary>
        public int[] NodesArray { get; set; }
        /// <summary>
        /// Sum of the demand on the tour
        /// </summary>
        public double Demand { get; set; }

    


    }
    /// <summary>
    /// A line in the savings list
    /// </summary>
    public class SavingsCouple
    {
        /// <summary>
        /// First Node in the Couple
        /// </summary>
        public int FirstNode { get; set; }
        /// <summary>
        /// Second Node in the Couple
        /// </summary>
        public int SecondNode { get; set; }
        /// <summary>
        /// Savings for the couple
        /// </summary>
        public double Saving { get; set; }
    }
}


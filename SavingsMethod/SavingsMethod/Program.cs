using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SavingsMethod
{
    class Program
    {
        /// <summary>
        /// Entry point of the program 
        /// </summary>
        /// <param name="args">The first argument can be the name of the input file</param>
        static void Main(string[] args)
        {
            // Start the timer
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            Console.WriteLine("Welcome to our Savings Method implementation");

            // The name of the input file
            String inputFileName;

        

            // If there are command line arguments, the first one is the file name
            if (args.Length != 0)
                inputFileName = args[0];
            else
                inputFileName = "D022-04g.dat";

            // Create the parser object, and read in all the data from file. This object can read ANY TSPLIB file. Code borrowed from HeuristicLab.
            TSPLIBParser myParser = new TSPLIBParser(inputFileName);
            myParser.Parse();

            // Create the vrp data object, using the parsed data
            CVRPData vrpData = new CVRPData(myParser);

            // Run the heuristic
            CVRPSolution mySolution = SavingsMethodHeuristic(vrpData);

            // Check if solution makes sense
            int solutionCheck = IsFeasible(mySolution.Solution, vrpData);
            switch (IsFeasible(mySolution.Solution, vrpData))
            {
                case 0:
                    Console.WriteLine("Solution verified");
                    break;
                case 1:
                    Console.WriteLine("Not allocating nodes correctly");
                    break;
                case 2:
                    Console.WriteLine("Using node many times");
                    break;
                case 3:
                    Console.WriteLine("Not using node");
                    break;
                //////////////////////////////////////////////////////////////////////////////
                //Add by Will&Ying
                case 4:
                    Console.WriteLine("Exceeding max distance");
                    break;
                //////////////////////////////////////////////////////////////////////////////
            }

            // Calculate the total time
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;

            // Output the solution
            mySolution.WriteToFile(inputFileName + ".opt", ts);


            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
        /// <summary>
        /// Returns zero if it is fine, 1 violating demand, 2 for using node many times, 3 for not using node
        /// 4 for exceeding max distance // Add by Will&Ying
        /// </summary>
        /// <param name="p"></param>
        /// <param name="vrpData"></param>
        /// <returns></returns>
        private static int IsFeasible(int[][] p, CVRPData vrpData)
        {
            int[] nodes = new int[vrpData.Dimension];
            nodes[0] = 1;

            for (int i = 0; i < p.Length; i++)
            {
                double demand = 0;
                for (int j = 0; j < p[i].Length; j++)
                {
                    demand = vrpData.Demands[p[i][j]];
                    nodes[p[i][j]]++;
                }
                if (demand > vrpData.Capacity)
                    return 1;
            }

            for (int i = 0; i < vrpData.Dimension; i++)
            {
                if (nodes[i] > 1)
                    return 2;
                if (nodes[i] < 1)
                    return 3;
            }
            ////////////////////////////////////////////////////////
            //Add by Will&Ying
            for (int i = 0; i < p.Length; i++)
            {
                double distance = 0;
                distance = vrpData.Distances[0, p[i][0]];
                for (int j = 0; j < p[i].Length-1; j++)
                {
                    distance = distance + vrpData.Distances[p[i][j], p[i][j+1]];
                } 
                distance = distance + vrpData.Distances[p[i][p[i].Length-1], 0];
                if (distance > vrpData.MaxLength)
                {
                    return 4;
                }
            }
            ////////////////////////////////////////////////////////
            return 0;
        }

        private static CVRPSolution SavingsMethodHeuristic(CVRPData vrpData)
        {
            Console.WriteLine("Starting the SavingsMethodHeuristic");

            //The savings list
            List<SavingsCouple> Savings = CalcSavings(vrpData);
            Savings = Savings.OrderByDescending(x => x.Saving).ToList();

            // Current tour list. Populated in the following loop, and will have tours removed and added as the heuristic progresses.
            List<VTour> Tours = new List<VTour>();
            // List of nodes that are either at the start of a tour or at the end. These can be merged. Initially all nodes are in the list.
            List<int> availNodes = new List<int>();
            // Create the inital tours
            for (int i = 1; i < vrpData.Dimension; i++)
            {
                VTour tour = new VTour();
                int[] nodes = new int[1];
                nodes[0] = i;
                tour.NodesArray = nodes;
                tour.Demand = vrpData.Demands[i];

                Tours.Add(tour);
                availNodes.Add(i);
            }

            //We will iterate through the savings list
            for (int i = 0; i < Savings.Count; i++)
            {
                //flag variable that indicate if both nodes in savings couple are at the extremites of a tour
                int flag = 0;

                //check if the nodes are in the extremites
                for (int j = 0; j < availNodes.Count; j++)
                    if ((availNodes[j] == Savings[i].FirstNode) || (availNodes[j] == Savings[i].SecondNode))
                        flag++;

                //If the nodes are at the extremities, try to merge
                if (flag == 2)
                {
                    //we need to find the two tours that contain the nodes
                    //id of the tours
                    int indexOfFirstRoute = -1;
                    int indexOfSecondRoute = -1;
                    double tourlength = 0;                //Add by Will&Ying

                    for (int j = 0; j < Tours.Count; j++)
                    {
                        //We found the tour that finishes with the first node
                        if (Tours[j].NodesArray[Tours[j].NodesArray.Length - 1] == Savings[i].FirstNode)
                        {
                            indexOfFirstRoute = j;
                            ///////////////////////////////////////////////////////////////////////////////////////////////////////
                            //Calculate total length
                            //Add by Will&Ying 10252012
                            tourlength = vrpData.Distances[0, Tours[j].NodesArray[0]];
                            for (int m = 0; m < Tours[j].NodesArray.Length - 1; m++)
                            {
                                tourlength = tourlength + vrpData.Distances[Tours[j].NodesArray[m], Tours[j].NodesArray[m + 1]];
                            }
                            tourlength = tourlength + vrpData.Distances[Tours[j].NodesArray[Tours[j].NodesArray.Length-1], 0];
                        }
                        //We found the tour that starts with the second node
                        if (Tours[j].NodesArray[0] == Savings[i].SecondNode)
                        {
                            indexOfSecondRoute = j;
                            ///////////////////////////////////////////////////////////////////////////////////////////////////////
                            //Calculate total length
                            //Add by Will&Ying 10252012
                            tourlength = tourlength + vrpData.Distances[0, Tours[j].NodesArray[0]];
                            for (int m = 0; m < Tours[j].NodesArray.Length - 1; m++)
                            {
                                tourlength = tourlength + vrpData.Distances[Tours[j].NodesArray[m], Tours[j].NodesArray[m + 1]];
                            }

                            tourlength = tourlength + vrpData.Distances[Tours[j].NodesArray[Tours[j].NodesArray.Length-1], 0];

                            //////////////////////////////////////////////////////////////////////////////////////////////////////
                        }
                        //////////////////////////////////////////////////////////////////////////////////////////////////////////
                        //Add by Will&Ying
                        tourlength = tourlength - Savings[i].Saving;
                        //////////////////////////////////////////////////////////////////////////////////////////////////////////
                    }
                 

                    /////////////////////////////////////////////////////////////////////////////////////////////////////////////
                    // Merge routes if:
                    // 1. pair from savings finishes one route and starts another
                    // 2. its not the same route
                    // 3. capacity not violated by merging
                    // 4. Total tour length is not larger than Max.Length //Add by Will&Ying 10252012
                    if (indexOfSecondRoute >= 0 && indexOfFirstRoute >= 0 && indexOfSecondRoute != indexOfFirstRoute && Tours[indexOfFirstRoute].Demand + Tours[indexOfSecondRoute].Demand <= vrpData.Capacity
                      && tourlength <= vrpData.MaxLength)
                    {
                        int newlength = Tours[indexOfFirstRoute].NodesArray.Length + Tours[indexOfSecondRoute].NodesArray.Length;
     
                        // Create a new sequence of nodes which corresponds to the merged route. It will include nodes from both routes
                        int[] tour = new int[newlength];
                        for (int j = 0; j < newlength; j++)
                        {

                            if (j < Tours[indexOfFirstRoute].NodesArray.Length)
                                tour[j] = Tours[indexOfFirstRoute].NodesArray[j];
                                
                            else
                            {
                                //subtract the length of tour A
                                int normIndex = j - Tours[indexOfFirstRoute].NodesArray.Length;
                                tour[j] = Tours[indexOfSecondRoute].NodesArray[normIndex];
                            }
                        }


                        VTour Tour = new VTour();
                        Tour.Demand = Tours[indexOfFirstRoute].Demand + Tours[indexOfSecondRoute].Demand;
                     

                        //add the combined tour and remove the old ones
                        Tour.NodesArray = tour;
                        Tours.Add(Tour);

                        // If the any of the tours merged are of length greater than 2, their corresponding extreme (starting or ending) node cannot
                        // be merged anymore. It can be removed from the savings list.
                        if (Tours[indexOfFirstRoute].NodesArray.Length > 1)
                            availNodes.Remove(Tours[indexOfFirstRoute].NodesArray.Last());
                        if (Tours[indexOfSecondRoute].NodesArray.Length > 1)
                            availNodes.Remove(Tours[indexOfSecondRoute].NodesArray.First());

                        // Remove the merged routes from the route list.
                        Tours.RemoveAt(indexOfFirstRoute);
                        if (indexOfFirstRoute > indexOfSecondRoute)
                            Tours.RemoveAt(indexOfSecondRoute);
                        else
                            Tours.RemoveAt(indexOfSecondRoute - 1);
                    }
                }
            }

            //Tours now contain the solution
            CVRPSolution sol = new CVRPSolution();
            sol.Solution = new int[Tours.Count][];

            for (int i = 0; i < Tours.Count; i++)
            {
                sol.Solution[i] = Tours[i].NodesArray;
                sol.TotalCost = sol.TotalCost + vrpData.Distances[0, sol.Solution[i][0]] + vrpData.Distances[sol.Solution[i].Last(), 0];
                for (int j = 0; j < sol.Solution[i].Length - 1; j++)
                    sol.TotalCost = sol.TotalCost + vrpData.Distances[sol.Solution[i][j], sol.Solution[i][j + 1]];
            }
            Console.WriteLine("Heuristic done.\nTotalcost : " + sol.TotalCost);
            return sol;
        }
        /// <summary>
        /// Calculate the savings
        /// </summary>
        private static List<SavingsCouple> CalcSavings(CVRPData vrpData)
        {
            //The savings list
            List<SavingsCouple> savings = new List<SavingsCouple>();
            //Go over all possible couples once and create a coupling, we rely on the fact that the distances are symmetric
            for (int i = 1; i < vrpData.Dimension; i++)
            {
                for (int j = 1; j < vrpData.Dimension; j++)
                {
                    if (j != i)
                    {
                        SavingsCouple cpl = new SavingsCouple();
                        cpl.FirstNode = i;
                        cpl.SecondNode = j;
                        cpl.Saving = vrpData.Distances[i, 0] + vrpData.Distances[0, j] - vrpData.Distances[i, j];
                        savings.Add(cpl);
                    }
                }

            }
            return savings;
        }

    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;


namespace VRP_SM
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch timer = new Stopwatch();                                                      // get running time
            timer.Start();                                                                          // start to calculate time
            List<Node> nodeList;                                                                    // a list of all nodes
            double bestKnown;                                                                       // best known solution
            double capacity;                                                                        // vehicle capacity
            int n;                                                                                  // number of nodes including the depot

            //string file = "A-n44-k6.vrp";
            string file = args[0];
            ReadData(out nodeList, out bestKnown, out capacity, out n, file);                       // read data from input file

            // Initialize the routes, each customer node forms a route with the depot
            List<Path> routeList = new List<Path>();                                                // define a route list
            for (int i = 1; i < nodeList.Count; i++)                                                // for each customer node  
            {
                routeList.Add(new Path(new List<Node>() { nodeList[0], nodeList[i], nodeList[0] }));// create route list
            }

            // Initialize the savings list for each node pair (i,j)
            List<Savings> savingsList = new List<Savings>();                                        // define a saving list
            for (int i = 1; i < nodeList.Count - 1; i++)        
            {
                for (int j = i + 1; j < nodeList.Count; j++)
                {
                    savingsList.Add(new Savings(i, j, nodeList[0], nodeList[i], nodeList[j]));      // create saving list
                }
            }

            // Sort the savings list in decreasing order
            var sortedSavingsList = from element in savingsList
                                    orderby element.S descending
                                    select element;

            // Combine routes
            Path r1 = null;                                                                         // route includes node i 
            Path r2 = null;                                                                         // route includes node j
            Node ndi = null;                                                                        // node list i
            Node ndj = null;                                                                        // node list j

            foreach (Savings sij in sortedSavingsList)
            {
                int i = sij.I;                                                                      // i of node pair (i,j) on saving list
                int j = sij.J;                                                                      // j of node pair (i,j) on saving list

                ndi = nodeList[i];  
                ndj = nodeList[j];  

                r1 = ndi.onPath;
                r2 = ndj.onPath;

                //check if node i and node j belong to different routes, and the load of two routes is less than the capacity
                if (r1 != r2 && (r1.Load + r2.Load) <= capacity)    
                {
                    int index1 = r1.AllNodes.Count;                                                 // the count of all nodes on route1
                    int index2 = r2.AllNodes.Count;                                                 // the count of all nodes on route2

                    //check if node i is the first node of the path
                    if (ndi == r1.AllNodes[1])          
                    {
                        //check if node j is the first node of the path
                        if (ndj == r2.AllNodes[1])     
                        {
                            r1.AllNodes.RemoveAt(0);                                                // remove the node with index = 0 on route1
                            r1.AllNodes.Reverse();                                                  // reverse the order of the rest nodes on route1
                            r2.AllNodes.RemoveAt(0);                                                // remove the node with index = 0 on route2
                            
                            routeList.Remove(r1);                                                   // remove route1 (arc (0,i))from route list
                            routeList.Remove(r2);                                                   // remove route2 (arc (j,0))from route list
                            routeList.Add(new Path(r1.AllNodes.Concat(r2.AllNodes).ToList()));      // add a new path (i,j)
                            continue;
                        }
                        //check if node j is the last node of the path
                        if (ndj == r2.AllNodes[index2 - 2]) 
                        {
                            r2.AllNodes.RemoveAt(index2 - 1);                                       // remove the node with index = index2-1 on route2
                            r1.AllNodes.RemoveAt(0);                                                // remove the node with index = 0 on route1

                            routeList.Remove(r1);                                                   // remove r1 (arc (0,i))from route list
                            routeList.Remove(r2);                                                   // remove r2 (arc (j,0))from route list
                            routeList.Add(new Path(r2.AllNodes.Concat(r1.AllNodes).ToList()));      // add a new path (j,i)
                            continue;
                        }
                    }
                    //check if node i is the last node of the path
                    if (ndi == r1.AllNodes[index1 - 2]) 
                    {
                        //check if node j is the first node of the path
                        if (ndj == r2.AllNodes[1]) 
                        {
                            r1.AllNodes.RemoveAt(index1 - 1);
                            r2.AllNodes.RemoveAt(0);

                            routeList.Remove(r1);
                            routeList.Remove(r2);
                            routeList.Add(new Path(r1.AllNodes.Concat(r2.AllNodes).ToList()));
                            continue;
                        }
                        //check if node j is the last node of the path
                        if (ndj == r2.AllNodes[index2 - 2]) 
                        {
                            r1.AllNodes.RemoveAt(index1 - 1);
                            r2.AllNodes.Reverse();
                            r2.AllNodes.RemoveAt(0);

                            routeList.Remove(r1);
                            routeList.Remove(r2);
                            routeList.Add(new Path(r1.AllNodes.Concat(r2.AllNodes).ToList()));
                        }
                    }
                }
            }

            // 3-opt enhancement
            // make the distance matrix
            double[,] d = new double[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    d[i, j] = Link.GetLength(nodeList[i], nodeList[j]);
                }
            }

            for (int i = 0; i < routeList.Count; i++)
                if (routeList[i].AllNodes.Count > 4)
                    routeList[i] = new Path(ThreeOPT(routeList[i], d));

            // Output the results in console window
            double totalCost = 0;                                                           // total cost 

            timer.Stop();                                                                   // stop to calculate run time
          
            // get the elapsed time as a TimeSpan value
            TimeSpan totalTime = timer.Elapsed;

            // save results to an outfile
            using (StreamWriter writer = new StreamWriter(file + ".opt"))
            {
                Console.SetOut(writer);
                for (int i = 0; i < routeList.Count; i++)
                {
                    totalCost += routeList[i].Length;                                           // calculate the total path lenght of all routes in route list
                    routeList[i].Display(i + 1);                                                // display route number and all nodes 
                    //Console.WriteLine(routeList[i].Load);

                }

                Console.WriteLine("Cost {0}", totalCost);                                       // total cost
                Console.WriteLine("Best known {0}", bestKnown);                                 // best known solution
                Console.WriteLine("Time " + Math.Round(totalTime.TotalMilliseconds));           // total run time (unit: milliseconds)
                
            }
            // Recover the standard output stream so that a completion message can be displayed.
            StreamWriter standardOutput = new StreamWriter(Console.OpenStandardOutput());
            standardOutput.AutoFlush = true;
            Console.SetOut(standardOutput);

            Console.WriteLine("The instance is solved successfully.");
            //foreach (Node nd in nodeList)
            //{
            //    Console.Write(nd.X);
            //    Console.Write(" ");
            //    Console.Write(nd.Y);
            //    Console.Write(" ");
            //    Console.Write(nd.Demand);
            //    Console.WriteLine();
            //}
            //Console.ReadKey();
        }


        // read input file
        static void ReadData(out List<Node> nodeList, out double bestKnown, out double capacity, out int n, string file)
        {
            string line;
            FileStream aFile = new FileStream(file, FileMode.Open);             // open input file
            StreamReader sr = new StreamReader(aFile);                                      // read characters from input file 
            nodeList = new List<Node>();                                                    // create a node list

            Dictionary<string, string> info = new Dictionary<string, string>();             // create a dictionary in order to read general information from input file
            char[] charArray = new char[] { ' ', ':' };                                     // create a char array used in storing character information in the input file
            string[] stringArray;                                                           // create a string array

            line = sr.ReadLine().Trim();                                                           // reads a line of characters from the current string in the input file

            while (line != "NODE_COORD_SECTION")                                            // if have not read line including characters "NODE_COORD_SECTION"
            {
                stringArray = line.Split(charArray, StringSplitOptions.RemoveEmptyEntries); // store characters in each line to stringArray 
                info.Add(stringArray[0], stringArray[1]);                                   // adds specified key that is characters in '' and value that is characters after ':' to the dictionary info
                line = sr.ReadLine().Trim();                                                       // read the next line
            }

            bestKnown = double.Parse(info["BEST_KNOWN"]);                                   // stores the value of Key "BEST_KNOWN" to variable bestKnown
            capacity = double.Parse(info["CAPACITY"]);                                      // stores the value of Key "CAPACITY" to variable capacity
            n = int.Parse(info["DIMENSION"]);                                               // stores the value of Key "DIMENSION" to variable n

            // read node coordinates data
            for (int i = 0; i < n; i++)                                                     // after line "NODE_COORD_SECTION", n lines include coordinates data of n nodes
            {
                line = sr.ReadLine().Trim();                                                // reads a line of coordiantes data from the current string in the input file                                         
                stringArray = line.Split(' ');                                              // store coordiates data to stringArray temporarily 
                nodeList.Add(new Node(i, double.Parse(stringArray[1]), double.Parse(stringArray[2]))); // add coordiates data to the node list 
            }

            line = sr.ReadLine().Trim();                                                    // after store all coordiates data of all nodes, line = "DEMAND_SECTION"

            // read node demand data
            for (int i = 0; i < n; i++)                                                     // the following n lines include demand data of n nodes
            {
                line = sr.ReadLine().Trim();                                                       // reads a line of demand data from the current string in the input file 
                stringArray = line.Split(' ');                                              // store demand data to stringArray temporarily 
                nodeList[i].Demand = double.Parse(stringArray[1]);                          // add demand data to the node list
            }
            sr.Close();                                                                     // close stream reader
        }

        // 3-opt algorithm referred to Lin (1965) "Computer solutions of the traveling salesman problem"
        static List<Node> ThreeOPT(Path route, double[,] d)
        {
            List<Node> nodes = route.AllNodes;
            if (route.AllNodes.Count < 5) return null;

            nodes.RemoveAt(nodes.Count - 1);
            int n = nodes.Count;

            int flag1 = 0;
            int flag2 = 0;

            while (flag1 == 0)
            {
                flag1 = 1;
                for (int k = 0; k < n - 3; k++)
                {
                    double dd = 0;
                    for (int j = k + 1; j < n - 1; j++)
                    {
                        if (d[nodes[k].Id, nodes[j + 1].Id] + d[nodes[0].Id, nodes[j].Id] <= d[nodes[0].Id, nodes[j + 1].Id] + d[nodes[k].Id, nodes[j].Id])
                        { dd = d[nodes[k].Id, nodes[j + 1].Id] + d[nodes[0].Id, nodes[j].Id]; flag2 = 1; }
                        else
                        { dd = d[nodes[0].Id, nodes[j + 1].Id] + d[nodes[k].Id, nodes[j].Id]; flag2 = 2; }

                        if (dd + d[nodes[k + 1].Id, nodes[n - 1].Id] < d[nodes[0].Id, nodes[n - 1].Id] + d[nodes[k].Id, nodes[k + 1].Id] + d[nodes[j].Id, nodes[j + 1].Id])
                        {
                            flag1 = 0;
                            List<Node> tempList = null;
                            List<Node> partList = null;
                            if (j != n - 2)
                            {
                                tempList = nodes.GetRange(j + 2, n - j - 2);
                                tempList = tempList.Concat(nodes.GetRange(k + 1, j - k)).ToList();
                            }
                            else 
                            {
                                tempList = nodes.GetRange(k + 1, j - k);
                            }
                            
                            partList = nodes.GetRange(0, k + 1);
                            if (flag2 == 1)
                            {    
                                tempList = tempList.Concat(partList).ToList();
                            }
                            else 
                            {
                                partList.Reverse();
                                tempList = tempList.Concat(partList).ToList();
                            }
                            tempList.Add(nodes[j + 1]);
                            nodes = tempList;
                            break;
                        }
                    }
                    if (flag1 == 0) break;
                }
            }

            int index = 0;
            for (int i = 0; i < n; i++)
            {
                if (nodes[i].Id == 0)
                { index = i; break; }
            }

           
            List<Node> temp = null;
            temp = nodes.GetRange(0, index);
            nodes = nodes.Concat(temp).ToList();
            nodes.RemoveRange(0, index);

            nodes.Add(nodes[0]);

            return nodes;
        }
    }
}

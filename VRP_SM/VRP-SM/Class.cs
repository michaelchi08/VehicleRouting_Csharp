using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VRP_SM
{
    class Node
    {
        public int Id { get; set; } // Node id

        public double X { get; set; } // X coordinate of the node

        public double Y { get; set; } // Y coordinate of the node

        public double Demand { get; set; } // Node demand

        public Path onPath { get; set; } // The path which includes the node

        // Initialize the node
        public Node(int n, double x, double y, int d = 0)
        {
            Id = n; X = x; Y = y; Demand = d;
        }

    }

    class Link
    {
        public Node Tail { get; set; } // The tail node of the link

        public Node Head { get; set; } // The head node of the link

        public double Length { get; set; } // The length of the link

        // Compute the distance between two nodes
        public static double GetLength(Node n1, Node n2) 
        {
            return Math.Sqrt(Math.Pow(n1.X - n2.X, 2) + Math.Pow(n1.Y - n2.Y, 2));
        }

        public Link(Node t, Node h)
        {
            Tail = t; Head = h;
            Length = Math.Sqrt(Math.Pow(Tail.X - Head.X, 2) + Math.Pow(Tail.Y - Head.Y, 2));
        }
    }

    class Path
    {
        public List<Node> AllNodes { get; set; }    // All nodes on the path

        public double Length { get; set; }          // The length of the path

        public double Load { get; set; }            // The sum of the demand of each node on this path

        //public Path() { }

        public Path(List<Node> nodes)
        {
            AllNodes = nodes;
            Length = 0;
            Load = 0;

            // Calculate Length and Load of the path and set the onPath property of all nodes (excluding the depot) on this path
            for (int i = 0; i < AllNodes.Count - 1; i++)
            {
                Length += Link.GetLength(AllNodes[i], AllNodes[i + 1]);
                if (i > 0)
                {
                    Load += AllNodes[i].Demand;
                    AllNodes[i].onPath = this;
                }
                    
            }
        }

        public void Display(int n)
        {
            Console.Write("Route #{0}:", n);
            for (int i = 1; i < AllNodes.Count - 1; i++)
            {
                Console.Write(" {0}", AllNodes[i].Id);
            }
            Console.Write("\n");
        }
    }

    class Savings
    {
        public int I { get; set; }
        public int J { get; set; }

        public double S { get; set; }

        // Calculate the savings, nd0 = depot, nd1 = node i, nd2 = node j
        public Savings(int i, int j, Node nd0, Node nd1, Node nd2, double lambda = 1.0)
        {
            I = i; J = j;
            S = Link.GetLength(nd1, nd0) + Link.GetLength(nd0, nd2) - lambda * Link.GetLength(nd1, nd2);
        }
    }
}

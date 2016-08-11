using System;
using System.Collections.Generic;
using System.Linq;

namespace LouvainCommunityPL
{
    /// <summary>
    /// To handle several pieces of data for the algorithm in one structure.
    /// </summary>
    class Status
    {
        public Dictionary<int, int> Node2Com;
        public Double TotalWeight;
        public Dictionary<int, double> Degrees;
        public Dictionary<int, double> GDegrees;
        public Dictionary<int, double> Loops;
        public Dictionary<int, double> Internals;


        /// <summary>
        /// Get the modularity of the partition of the graph fast using precomputed status.
        /// </summary>
        /// <returns></returns>
        public double Modularity
        {
            get
            {
                double links = TotalWeight;
                double result = 0;
                foreach (int community in Node2Com.Values.Distinct())
                {
                    double in_degree = Internals.GetValueOrDefault(community);
                    double degree = Degrees.GetValueOrDefault(community);
                    if (links > 0)
                    {
                        result += in_degree / links - Math.Pow(degree / (2 * links), 2);
                    }
                }
                return result;

            }
        }


        public Status()
        {
            Node2Com = new Dictionary<int, int>();
            TotalWeight = 0;
            Degrees = new Dictionary<int, double>();
            GDegrees = new Dictionary<int, double>();
            Loops = new Dictionary<int, double>();
            Internals = new Dictionary<int, double>();
        }

        public Status(Graph graph) : this()
        {            
            int count = 0;
            this.TotalWeight = graph.Size;
            foreach (int node in graph.Nodes)
            {
                Node2Com[node] = count;
                double deg = graph.Degree(node);
                if (deg < 0)
                {
                    throw new ArgumentException("Graph has negative weights.");
                }
                Degrees[count] = GDegrees[node] = deg;
                Internals[count] = Loops[node] = graph.EdgeWeight(node, node, 0);
                count += 1;
            }
        }

       
        /// <summary>
        /// Compute the communities in th eneighborhood of the node in the given graph.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="graph"></param>
        /// <returns></returns>
        public Dictionary<int, double> NeighCom(int node, Graph graph)
        {
            Dictionary<int, double> weights = new Dictionary<int, double>();
            foreach (Graph.Edge edge in graph.IncidentEdges(node))
            {
                if (!edge.SelfLoop)
                {
                    int neighborcom = Node2Com[edge.ToNode];
                    weights[neighborcom] = weights.GetValueOrDefault(neighborcom) + edge.Weight;
                }
            }
            return weights;
        }

        /// <summary>
        /// Remove node from community com and modify status.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="com"></param>
        /// <param name="weight"></param>
        public void Remove(int node, int com, double weight)
        {
            Degrees[com] = Degrees.GetValueOrDefault(com) - GDegrees.GetValueOrDefault(node);
            Internals[com] = Internals.GetValueOrDefault(com) - weight - Loops.GetValueOrDefault(node);
            Node2Com[node] = -1;
        }

        /// <summary>
        /// Insert node into community and modify status.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="com"></param>
        /// <param name="weight"></param>
        public void Insert(int node, int com, double weight)
        {
            Node2Com[node] = com;
            Degrees[com] = Degrees.GetValueOrDefault(com) + GDegrees.GetValueOrDefault(node);
            Internals[com] = Internals.GetValueOrDefault(com) + weight + Loops.GetValueOrDefault(node);
        }       
    }
}
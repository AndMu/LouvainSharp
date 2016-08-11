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
        private readonly Dictionary<int, int> m_NodeToCommunities = new Dictionary<int, int>();
        private readonly Dictionary<int, double> m_CommunityDegrees;
        public Dictionary<int, double> GDegrees;
        public Dictionary<int, double> Loops;
        public Dictionary<int, double> Internals;

        public Double TotalWeight { get; }

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
                foreach (int community in m_NodeToCommunities.Values.Distinct())
                {
                    double in_degree = Internals.GetValueOrDefault(community);
                    double degree = m_CommunityDegrees.GetValueOrDefault(community);
                    if (links > 0)
                    {
                        result += in_degree / links - Math.Pow(degree / (2 * links), 2);
                    }
                }
                return result;

            }
        }

        public IReadOnlyDictionary<int, int> CurrentPartition => m_NodeToCommunities; 


        public Status()
        {            
            TotalWeight = 0;
            m_CommunityDegrees = new Dictionary<int, double>();
            GDegrees = new Dictionary<int, double>();
            Loops = new Dictionary<int, double>();
            Internals = new Dictionary<int, double>();
        }

        public Status(IGraph graph) : this()
        {            
            int count = 0;
            this.TotalWeight = graph.TotalWeight;
            foreach (int node in graph.Nodes)
            {
                m_NodeToCommunities[node] = count;
                double deg = graph.GetDegree(node);
                if (deg < 0)
                {
                    throw new ArgumentException("Graph has negative weights.");
                }
                m_CommunityDegrees[count] = GDegrees[node] = deg;
                Internals[count] = Loops[node] = graph.GetEdgeWeight(node, node);
                count += 1;
            }
        }

       
        /// <summary>
        /// Compute the communities in the neighborhood of the node in the given graph.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="graph"></param>
        /// <returns></returns>
        public Dictionary<int, double> NeighCom(int node, IGraph graph)
        {
            Dictionary<int, double> weights = new Dictionary<int, double>();
            foreach (Edge edge in graph.GetIncidentEdges(node))
            {
                if (!edge.SelfLoop)
                {
                    int neighborcom = m_NodeToCommunities[edge.ToNode];
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
            m_CommunityDegrees[com] = m_CommunityDegrees.GetValueOrDefault(com) - GDegrees.GetValueOrDefault(node);
            Internals[com] = Internals.GetValueOrDefault(com) - weight - Loops.GetValueOrDefault(node);
            m_NodeToCommunities[node] = -1;
        }

        /// <summary>
        /// Insert node into community and modify status.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="com"></param>
        /// <param name="weight"></param>
        public void Insert(int node, int com, double weight)
        {
            m_NodeToCommunities[node] = com;
            m_CommunityDegrees[com] = m_CommunityDegrees.GetValueOrDefault(com) + GDegrees.GetValueOrDefault(node);
            Internals[com] = Internals.GetValueOrDefault(com) + weight + Loops.GetValueOrDefault(node);
        }


        public double GetCommunityDegree(int community)
        {
            return m_CommunityDegrees.GetValueOrDefault(community);
        }
    }
}
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
        readonly Dictionary<int, int> m_NodeToCommunities = new Dictionary<int, int>();
        readonly Dictionary<int, double> m_CommunityDegrees;
        readonly Dictionary<int, double> m_NodeDegrees;
        readonly Dictionary<int, double> m_SelfLoopWeights;
        readonly Dictionary<int, double> m_CommunityInternalWeights;

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
                    double in_degree = m_CommunityInternalWeights.GetValueOrDefault(community);
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
            m_NodeDegrees = new Dictionary<int, double>();
            m_SelfLoopWeights = new Dictionary<int, double>();
            m_CommunityInternalWeights = new Dictionary<int, double>();
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
                m_CommunityDegrees[count] = m_NodeDegrees[node] = deg;
                m_CommunityInternalWeights[count] = m_SelfLoopWeights[node] = graph.GetEdgeWeight(node, node);
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
            m_CommunityDegrees[com] = m_CommunityDegrees.GetValueOrDefault(com) - m_NodeDegrees.GetValueOrDefault(node);
            m_CommunityInternalWeights[com] = m_CommunityInternalWeights.GetValueOrDefault(com) - weight - m_SelfLoopWeights.GetValueOrDefault(node);
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
            m_CommunityDegrees[com] = m_CommunityDegrees.GetValueOrDefault(com) + m_NodeDegrees.GetValueOrDefault(node);
            m_CommunityInternalWeights[com] = m_CommunityInternalWeights.GetValueOrDefault(com) + weight + m_SelfLoopWeights.GetValueOrDefault(node);
        }


        public double GetCommunityDegree(int community)
        {
            return m_CommunityDegrees.GetValueOrDefault(community);
        }

        public double GetNodeDegree(int node)
        {
            return m_NodeDegrees.GetValueOrDefault(node);
        }
    }
}
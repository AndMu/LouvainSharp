using System;
using System.Collections.Generic;
using System.Linq;

namespace LouvainCommunityPL
{
    public class Modularity
    {
        /// <summary>
        /// Compute the modularity of a partition of a graph.
        /// Raises:
        /// KeyNotFoundException if the partition does not partition all graph nodes
        /// InvalidOperationException if the graph has no link
        /// References:
        /// 1. Newman, M.E.J. & Girvan, M. Finding and evaluating community structure in networks. Physical Review E 69,
        /// 26113(2004).
        /// </summary>
        /// <param name="graph">The graph which is decomposed.</param>
        /// <param name="partition">
        /// The partition of the nodes in the graph (i.e., a dictionary where keys are nodes and values are
        /// communities).
        /// </param>
        /// <returns>The modularity.</returns>
        public static double Compute(Graph graph, Dictionary<int, int> partition)
        {
            Dictionary<int, double> inc = new Dictionary<int, double>();
            Dictionary<int, double> deg = new Dictionary<int, double>();

            double links = graph.Size;
            if (links == 0)
            {
                throw new InvalidOperationException("A graph without links has undefined modularity.");
            }

            foreach (int node in graph.Nodes)
            {
                int com = partition[node];
                deg[com] = DictGet(deg, com, 0) + graph.Degree(node);
                foreach (var edge in graph.IncidentEdges(node))
                {
                    int neighbor = edge.ToNode;
                    if (partition[neighbor] == com)
                    {
                        double weight;
                        if (neighbor == node)
                        {
                            weight = edge.Weight;
                        }
                        else
                        {
                            weight = edge.Weight/2;
                        }
                        inc[com] = DictGet(inc, com, 0) + weight;
                    }
                }
            }

            double res = 0;
            foreach (int component in partition.Values.Distinct())
            {
                res += DictGet(inc, component, 0)/links - Math.Pow(DictGet(deg, component, 0)/(2*links), 2);
            }
            return res;
        }

        private static B DictGet<A, B>(Dictionary<A, B> dict, A key, B defaultValue)
        {
            B result;
            if (dict.TryGetValue(key, out result))
            {
                return result;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
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

        public Status()
        {
            Node2Com = new Dictionary<int, int>();
            TotalWeight = 0;
            Degrees = new Dictionary<int, double>();
            GDegrees = new Dictionary<int, double>();
            Loops = new Dictionary<int, double>();
            Internals = new Dictionary<int, double>();
        }

        public Status(Graph graph, Dictionary<int, int> part)
            : this()
        {
            int count = 0;
            this.TotalWeight = graph.Size;
            if (part == null)
            {
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
            else
            {
                foreach (int node in graph.Nodes)
                {
                    int com = part[node];
                    Node2Com[node] = com;
                    double deg = graph.Degree(node);
                    Degrees[com] = DictGet(Degrees, com, 0) + deg;
                    GDegrees[node] = deg;
                    double inc = 0;
                    foreach (Graph.Edge edge in graph.IncidentEdges(node))
                    {
                        int neighbor = edge.ToNode;
                        if (edge.Weight <= 0)
                        {
                            throw new ArgumentException("Graph must have postive weights.");
                        }
                        if (part[neighbor] == com)
                        {
                            if (neighbor == node)
                            {
                                inc += edge.Weight;
                            }
                            else
                            {
                                inc += edge.Weight/2;
                            }
                        }
                    }
                    Internals[com] = DictGet(Internals, com, 0) + inc;
                }
            }
        }

        /// <summary>
        /// Compute the modularity of the partition of the graph fast using precomputed status.
        /// </summary>
        /// <returns></returns>
        public double Modularity()
        {
            double links = TotalWeight;
            double result = 0;
            foreach (int community in Node2Com.Values.Distinct())
            {
                double in_degree = DictGet(Internals, community, 0);
                double degree = DictGet(Degrees, community, 0);
                if (links > 0)
                {
                    result += in_degree/links - Math.Pow(degree/(2*links), 2);
                }
            }
            return result;
        }

        /// <summary>
        /// Used in parallelized OneLevel
        /// </summary>
        private Tuple<double, int> EvaluateIncrease(int com, double dnc, double degc_totw)
        {
            double incr = dnc - DictGet(Degrees, com, 0)*degc_totw;
            return Tuple.Create(incr, com);
        }

        /// <summary>
        /// Compute one level of communities.
        /// </summary>
        /// <param name="graph">The graph to use.</param>
        public void OneLevel(Graph graph)
        {
            bool modif = true;
            int nb_pass_done = 0;
            double cur_mod = this.Modularity();
            double new_mod = cur_mod;

            while (modif && nb_pass_done != Community.PASS_MAX)
            {
                cur_mod = new_mod;
                modif = false;
                nb_pass_done += 1;

                foreach (int node in graph.Nodes)
                {
                    int com_node = Node2Com[node];
                    double degc_totw = DictGet(GDegrees, node, 0)/(TotalWeight*2);
                    Dictionary<int, double> neigh_communities = NeighCom(node, graph);
                    Remove(node, com_node, DictGet(neigh_communities, com_node, 0));

                    Tuple<double, int> best;
                    best = (from entry in neigh_communities.AsParallel()
                        select EvaluateIncrease(entry.Key, entry.Value, degc_totw))
                        .Concat(new[] {Tuple.Create(0.0, com_node)}.AsParallel())
                        .Max();
                    int best_com = best.Item2;
                    Insert(node, best_com, DictGet(neigh_communities, best_com, 0));
                    if (best_com != com_node)
                    {
                        modif = true;
                    }
                }
                new_mod = this.Modularity();
                if (new_mod - cur_mod < Community.MIN)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Compute the communities in th eneighborhood of the node in the given graph.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="graph"></param>
        /// <returns></returns>
        private Dictionary<int, double> NeighCom(int node, Graph graph)
        {
            Dictionary<int, double> weights = new Dictionary<int, double>();
            foreach (Graph.Edge edge in graph.IncidentEdges(node))
            {
                if (!edge.SelfLoop)
                {
                    int neighborcom = Node2Com[edge.ToNode];
                    weights[neighborcom] = DictGet(weights, neighborcom, 0) + edge.Weight;
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
        private void Remove(int node, int com, double weight)
        {
            Degrees[com] = DictGet(Degrees, com, 0) - DictGet(GDegrees, node, 0);
            Internals[com] = DictGet(Internals, com, 0) - weight - DictGet(Loops, node, 0);
            Node2Com[node] = -1;
        }

        /// <summary>
        /// Insert node into community and modify status.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="com"></param>
        /// <param name="weight"></param>
        private void Insert(int node, int com, double weight)
        {
            Node2Com[node] = com;
            Degrees[com] = DictGet(Degrees, com, 0) + DictGet(GDegrees, node, 0);
            Internals[com] = DictGet(Internals, com, 0) + weight + DictGet(Loops, node, 0);
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
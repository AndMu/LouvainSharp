﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace LouvainCommunityPL
{
    /// <summary>
    /// This class implements community detection.
    /// Adapted from python-louvain, http://perso.crans.org/aynaud/communities/, by Kyle Miller (v-kymil@microsoft.com)
    /// February 2014
    /// Original copyright:
    /// Copyright (C) 2009 by
    /// Thomas Aynaud
    /// <thomas.aynaud@ lip6.fr>
    /// All rights reserved.
    /// BSD license.
    /// </summary>
    public static class Community
    {
        internal static int PASS_MAX = -1;
        internal static double MIN = 0.0000001;

        /// <summary>
        /// Compute the partition of the graph nodes which maximises the modularity using the Louvain heuristics (or try...)
        /// This is the partition of the highest modularity, i.e., the highest partition of the dendrogram generated by the Louvain
        /// algorithm.
        /// See also: GenerateDendrogram to obtain all the decomposition levels
        /// Notes: Uses the Louvain algorithm
        /// References:
        /// 1. Blondel, V.D. et al. Fast unfolding of communities in large networks. J. Stat. Mech 10008, 1-12(2008).
        /// </summary>
        /// <param name="graph">The graph which is decomposed.</param>
        /// <param name="partition">
        /// The algorithm will start using this partition of nodes. It is a dictionary where keys are nodes
        /// and values are communities.
        /// </param>
        /// <returns>The partition, with communities number from 0 onward, sequentially</returns>
        public static Dictionary<int, int> BestPartition(Graph graph)
        {
            Dendrogram dendro = GenerateDendrogram(graph);
            return dendro.PartitionAtLevel(dendro.Length - 1);
        }

        public static Dendrogram GenerateDendrogram(Graph graph)
        {
            Dictionary<int, int> partition;

            // Special case, when there is no link, the best partition is everyone in its own community.
            if (graph.NumberOfEdges == 0)
            {
                partition = new Dictionary<int, int>();
                int i = 0;
                foreach (int node in graph.Nodes)
                {
                    partition[node] = i++;
                }
                return new Dendrogram(partition);
            }

            Graph current_graph = new Graph(graph);
            Status status = new Status(current_graph);
            double mod = status.Modularity;
            List<Dictionary<int, int>> status_list = new List<Dictionary<int, int>>();
            OneLevel(current_graph, status);
            double new_mod;
            new_mod = status.Modularity;

            int iterations = 1;
            do
            {
                iterations++;
                partition = Renumber(status.Node2Com);
                status_list.Add(partition);
                mod = new_mod;
                current_graph = current_graph.Quotient(partition);
                status = new Status(current_graph);
                OneLevel(current_graph, status);
                new_mod = status.Modularity;
            }
            while (new_mod - mod >= MIN);
            //Console.Out.WriteLine("(GenerateDendrogram: {0} iterations in {1})", iterations, stopwatch.Elapsed);

            return new Dendrogram(status_list);
        }

        private static Dictionary<A, int> Renumber<A>(Dictionary<A, int> dict)
        {
            var ret = new Dictionary<A, int>();
            var new_values = new Dictionary<int, int>();

            foreach (A key in dict.Keys.OrderBy(a => a))
            {
                int value = dict[key];
                int new_value;
                if (!new_values.TryGetValue(value, out new_value))
                {
                    new_value = new_values[value] = new_values.Count;
                }
                ret[key] = new_value;
            }
            return ret;
        }


        /// <summary>
        /// Compute one level of communities.
        /// </summary>
        /// <param name="graph">The graph to use.</param>
        static void OneLevel(Graph graph, Status status)
        {
            bool modif = true;
            int nb_pass_done = 0;
            double cur_mod = status.Modularity;
            double new_mod = cur_mod;

            while (modif && nb_pass_done != Community.PASS_MAX)
            {
                cur_mod = new_mod;
                modif = false;
                nb_pass_done += 1;

                foreach (int node in graph.Nodes)
                {
                    int com_node = status.Node2Com[node];
                    double degc_totw = status.GDegrees.GetValueOrDefault(node) / (status.TotalWeight * 2);
                    Dictionary<int, double> neigh_communities = status.NeighCom(node, graph);
                    status.Remove(node, com_node, neigh_communities.GetValueOrDefault(com_node));

                    Tuple<double, int> best;
                    best = (from entry in neigh_communities.AsParallel()
                            select EvaluateIncrease(status, entry.Key, entry.Value, degc_totw))
                        .Concat(new[] { Tuple.Create(0.0, com_node) }.AsParallel())
                        .Max();
                    int best_com = best.Item2;
                    status.Insert(node, best_com, neigh_communities.GetValueOrDefault(best_com));
                    if (best_com != com_node)
                    {
                        modif = true;
                    }
                }
                new_mod = status.Modularity;
                if (new_mod - cur_mod < Community.MIN)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Used in parallelized OneLevel
        /// </summary>
        static Tuple<double, int> EvaluateIncrease(Status status, int com, double dnc, double degc_totw)
        {
            double incr = dnc - status.Degrees.GetValueOrDefault(com) * degc_totw;
            return Tuple.Create(incr, com);
        }
    }


}
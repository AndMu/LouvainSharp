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
        static int PASS_MAX = -1;
        static double MIN = 0.0000001;

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
        /// <returns>The partition, with communities number from 0 onward, sequentially</returns>
        public static IDictionary<int, int> BestPartition(IGraph graph)
        {
            var dendrogram = GenerateDendrogram(graph);
            return dendrogram.GetPartitionAtLevel(dendrogram.Length - 1);
        }

        public static Dendrogram GenerateDendrogram(IGraph graph)
        {
            IDictionary<int, int> partition;

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

            IGraph currentGraph = graph;
            IStatus currentStatus = new Status(currentGraph);
            var status_list = new List<IDictionary<int, int>>();

            double previousModularity = currentStatus.CurrentModularity;
            OneLevel(currentGraph, currentStatus);
            double newModularity = currentStatus.CurrentModularity;
            
            do
            {
                partition = Renumber(currentStatus.CurrentPartition);
                status_list.Add(partition);

                previousModularity = newModularity;

                currentGraph = currentGraph.GetQuotient(partition);
                currentStatus = new Status(currentGraph);

                OneLevel(currentGraph, currentStatus);

                newModularity = currentStatus.CurrentModularity;
            }
            while (newModularity - previousModularity >= MIN);

            return new Dendrogram(status_list);
        }

        /// <summary>
        /// Renumbers the communities in the specified partition
        /// so there are no "gaps" in the range of community numbers
        /// </summary>        
        /// <param name="partition">The partition of a graph into communities</param>
        /// <returns></returns>
        static Dictionary<int, int> Renumber(IReadOnlyDictionary<int, int> partition)
        {
            var renumberedPartition = new Dictionary<int, int>();
            var newCommunityIds = new Dictionary<int, int>();
            int nextNewCommunityId = 0;

            foreach (var nodeId in partition.Keys.OrderBy(a => a))
            {
                var oldCommunityId = partition[nodeId];
                
                if (!newCommunityIds.ContainsKey(oldCommunityId))
                {
                    newCommunityIds.Add(oldCommunityId, nextNewCommunityId);
                    nextNewCommunityId += 1;
                }
                renumberedPartition[nodeId] = newCommunityIds[oldCommunityId];
            }
            return renumberedPartition;
        }


        /// <summary>
        /// Compute one level of communities.
        /// </summary>        
        static void OneLevel(IGraph graph, IStatus status)
        {
            bool modified = true;
            int currentPass = 0;
            double currentModularity = status.CurrentModularity;
            double newModularity = currentModularity;

            while (modified && currentPass != PASS_MAX)
            {
                currentModularity = newModularity;
                modified = false;
                currentPass += 1;

                foreach (var node in graph.Nodes)
                {
                    // get the node's current and neighbouring communities
                    int community = status.CurrentPartition[node];
                    Dictionary<int, double> neighbourCommunities = status.GetNeighbourCommunities(node, graph);

                    double degc_totw = status.GetNodeDegree(node) / (status.TotalWeight * 2);

                    // remove node from it's current community
                    status.RemoveNodeFromCommunity(node, community, neighbourCommunities.GetValueOrDefault(community));

                    // determine the community to move the node into which yield the best increase in modularity
                    int bestCommunity = neighbourCommunities
                        .AsParallel()
                        .Select(entry => EvaluateIncrease(status, entry.Key, entry.Value, degc_totw))                       
                        .Concat(new[] { Tuple.Create(0.0, community) }
                        .AsParallel())
                        .Max()
                        .Item2;
                    
                    // add node to the determined community
                    status.AddNodeToCommunity(node, bestCommunity, neighbourCommunities.GetValueOrDefault(bestCommunity));
                    if (bestCommunity != community)
                    {
                        modified = true;
                    }
                }
                newModularity = status.CurrentModularity;

                if (newModularity - currentModularity < MIN)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Used in parallelized OneLevel
        /// </summary>
        static Tuple<double, int> EvaluateIncrease(IStatus status, int com, double dnc, double degc_totw)
        {            
            double incr = dnc - status.GetCommunityDegree(com) * degc_totw;
            return Tuple.Create(incr, com);
        }
    }


}
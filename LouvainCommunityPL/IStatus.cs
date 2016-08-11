using System.Collections.Generic;

namespace LouvainCommunityPL
{
    public interface IStatus
    {
        double TotalWeight { get; }

        double CurrentModularity { get; }

        IReadOnlyDictionary<int, int> CurrentPartition { get; }


        Dictionary<int, double> GetNeighbourCommunities(int node, IGraph graph);

        void RemoveNodeFromCommunity(int node, int com, double weight);

        void AddNodeToCommunity(int node, int com, double weight);

        double GetCommunityDegree(int community);

        double GetNodeDegree(int node);
    }
}
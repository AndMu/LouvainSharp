using System.Collections.Generic;

namespace LouvainCommunityPL
{
    public interface IStatus
    {
        double TotalWeight { get; }

        double Modularity { get; }

        IReadOnlyDictionary<int, int> CurrentPartition { get; }

        Dictionary<int, double> NeighCom(int node, IGraph graph);

        void Remove(int node, int com, double weight);

        void Insert(int node, int com, double weight);

        double GetCommunityDegree(int community);

        double GetNodeDegree(int node);
    }
}
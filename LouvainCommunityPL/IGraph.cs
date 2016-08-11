using System.Collections.Generic;

namespace LouvainCommunityPL
{
    public interface IGraph
    {
        int NumberOfEdges { get; }

        double TotalWeight { get; }

        IEnumerable<int> Nodes { get; }
        
        
        IEnumerable<Edge> GetIncidentEdges(int node);

        IGraph GetQuotient(IDictionary<int, int> partition);

        double GetDegree(int node);

        double GetEdgeWeight(int node1, int node2);
    }
}
using System.Collections.Generic;

namespace LouvainCommunityPL
{
    public interface IGraph
    {
        int NumberOfEdges { get; }

        double TotalWeight { get; }


        IEnumerable<int> Nodes { get; }
        
        
        IEnumerable<Edge> IncidentEdges(int node);

        IGraph Quotient(IDictionary<int, int> partition);

        double Degree(int node);

        double EdgeWeight(int node1, int node2);
    }
}
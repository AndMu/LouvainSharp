// -----------------------------------------------------------------------------------------------------------
//  Copyright (c) 2016, Andreas Grünwald
//  Licensed under the MIT License. 
//  
//  This software was derieved from "LouvainSharp" by markusmobius, which was licensed under the MIT License   
//  Copyright (c) 2015 markusmobius
//  https://github.com/markusmobius/LouvainSharp
//
// -----------------------------------------------------------------------------------------------------------

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
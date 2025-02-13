﻿using System;
using System.Collections.Generic;
using Advanced.Algorithms.DataStructures.Graph;

namespace Advanced.Algorithms.Graph;

/// <summary>
///     Find Toplogical order of a graph using Kahn's algorithm.
/// </summary>
public static class KahnsTopSort
{
    /// <summary>
    ///     Returns the vertices in Topologically Sorted Order.
    /// </summary>
    public static T[] GetTopSort<T>(this IDiGraph<T> graph)
    {
        var inEdgeMap = new Dictionary<T, int>();

        var kahnQueue = new Queue<T>();

        foreach (var vertex in graph.VerticesAsEnumerable)
        {
            inEdgeMap.Add(vertex.Key, vertex.InEdgeCount);

            //init queue with vertices having not in edges
            if (vertex.InEdgeCount == 0) kahnQueue.Enqueue(vertex.Key);
        }

        //no vertices with zero number of in edges
        if (kahnQueue.Count == 0) throw new Exception("Graph has a cycle.");

        var result = new List<T>(graph.VerticesCount);

        var visitCount = 0;
        //until queue is empty
        while (kahnQueue.Count > 0)
        {
            //cannot exceed vertex number of iterations
            if (visitCount > graph.VerticesCount) throw new Exception("Graph has a cycle.");

            //pick a neighbour
            var nextPick = graph.GetVertex(kahnQueue.Dequeue());

            //if in edge count is 0 then ready for result
            if (inEdgeMap[nextPick.Key] == 0) result.Add(nextPick.Key);

            //decrement in edge count for neighbours
            foreach (var edge in nextPick.OutEdges)
            {
                inEdgeMap[edge.TargetVertexKey]--;
                kahnQueue.Enqueue(edge.TargetVertexKey);
            }

            visitCount++;
        }

        return result.ToArray();
    }
}
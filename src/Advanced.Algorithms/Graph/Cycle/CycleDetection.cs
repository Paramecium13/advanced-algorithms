﻿using System.Collections.Generic;
using Advanced.Algorithms.DataStructures.Graph;

namespace Advanced.Algorithms.Graph;

/// <summary>
///     Cycle detection using Depth First Search.
/// </summary>
public static class CycleDetector
{
    /// <summary>
    ///     Returns true if a cycle exists
    /// </summary>
    public static bool HasCycle<T>(this IDiGraph<T> graph)
    {
        var visiting = new HashSet<T>();
        var visited = new HashSet<T>();

        foreach (var vertex in graph.VerticesAsEnumerable)
            if (!visited.Contains(vertex.Key))
                if (Dfs(vertex, visited, visiting))
                    return true;

        return false;
    }

    private static bool Dfs<T>(IDiGraphVertex<T> current,
        HashSet<T> visited, HashSet<T> visiting)
    {
        visiting.Add(current.Key);

        foreach (var edge in current.OutEdges)
        {
            //if we encountered a visiting vertex again
            //then their is a cycle
            if (visiting.Contains(edge.TargetVertexKey)) return true;

            if (visited.Contains(edge.TargetVertexKey)) continue;

            if (Dfs(edge.TargetVertex, visited, visiting)) return true;
        }

        visiting.Remove(current.Key);
        visited.Add(current.Key);

        return false;
    }
}
﻿using System;
using System.Collections.Generic;
using Advanced.Algorithms.DataStructures.Graph;
using Advanced.Algorithms.DataStructures.Graph.AdjacencyList;

namespace Advanced.Algorithms.Graph;

/// <summary>
///     An Edmond Karp max flow implementation on weighted directed graph using
///     adjacency list representation of graph and residual graph.
/// </summary>
public class EdmondKarpMaxFlow<T, TW> where TW : IComparable
{
    private readonly IFlowOperators<TW> @operator;

    public EdmondKarpMaxFlow(IFlowOperators<TW> @operator)
    {
        this.@operator = @operator;
    }

    /// <summary>
    ///     Compute max flow by searching a path
    ///     and then augmenting the residual graph until
    ///     no more path exists in residual graph with possible flow.
    /// </summary>
    public TW ComputeMaxFlow(IDiGraph<T> graph,
        T source, T sink)
    {
        ValidateOperator(graph);

        var residualGraph = CreateResidualGraph(graph);

        var path = Bfs(residualGraph, source, sink);

        var result = @operator.DefaultWeight;

        while (path != null)
        {
            result = @operator.AddWeights(result, AugmentResidualGraph(graph, residualGraph, path));
            path = Bfs(residualGraph, source, sink);
        }

        return result;
    }


    /// <summary>
    ///     Compute max flow by searching a path
    ///     and then augmenting the residual graph until
    ///     no more path exists in residual graph with possible flow.
    /// </summary>
    public WeightedDiGraph<T, TW> ComputeMaxFlowAndReturnResidualGraph(IDiGraph<T> graph,
        T source, T sink)
    {
        ValidateOperator(graph);

        var residualGraph = CreateResidualGraph(graph);

        var path = Bfs(residualGraph, source, sink);

        var result = @operator.DefaultWeight;

        while (path != null)
        {
            result = @operator.AddWeights(result, AugmentResidualGraph(graph, residualGraph, path));
            path = Bfs(residualGraph, source, sink);
        }

        return residualGraph;
    }

    private void ValidateOperator(IDiGraph<T> graph)
    {
        if (@operator == null)
            throw new ArgumentException("Provide an operator implementation for generic type W during initialization.");

        if (!graph.IsWeightedGraph)
            if (@operator.DefaultWeight.GetType() != typeof(int))
                throw new ArgumentException("Edges of unweighted graphs are assigned an imaginary weight of one (1)." +
                                            "Provide an appropriate IFlowOperators<int> operator implementation during initialization.");
    }

    /// <summary>
    ///     Return all flow Paths.
    /// </summary>
    internal List<List<T>> ComputeMaxFlowAndReturnFlowPath(WeightedDiGraph<T, TW> graph,
        T source, T sink)
    {
        var residualGraph = CreateResidualGraph(graph);

        var path = Bfs(residualGraph, source, sink);

        var flow = @operator.DefaultWeight;

        var result = new List<List<T>>();
        while (path != null)
        {
            result.Add(path);
            flow = @operator.AddWeights(flow, AugmentResidualGraph(graph, residualGraph, path));
            path = Bfs(residualGraph, source, sink);
        }

        return result;
    }

    /// <summary>
    ///     Augment current Path to residual Graph.
    /// </summary>
    private TW AugmentResidualGraph(IDiGraph<T> graph,
        WeightedDiGraph<T, TW> residualGraph, List<T> path)
    {
        var min = @operator.MaxWeight;

        for (var i = 0; i < path.Count - 1; i++)
        {
            var vertex1 = residualGraph.FindVertex(path[i]);
            var vertex2 = residualGraph.FindVertex(path[i + 1]);

            var edgeValue = vertex1.OutEdges[vertex2];

            if (min.CompareTo(edgeValue) > 0) min = edgeValue;
        }

        //augment path
        for (var i = 0; i < path.Count - 1; i++)
        {
            var vertex1 = residualGraph.FindVertex(path[i]);
            var vertex2 = residualGraph.FindVertex(path[i + 1]);

            //substract from forward paths
            vertex1.OutEdges[vertex2] = @operator.SubstractWeights(vertex1.OutEdges[vertex2], min);

            //add for backward paths
            vertex2.OutEdges[vertex1] = @operator.AddWeights(vertex2.OutEdges[vertex1], min);
        }

        return min;
    }

    /// <summary>
    ///     Bredth first search to find a path to sink in residual graph from source.
    /// </summary>
    private List<T> Bfs(WeightedDiGraph<T, TW> residualGraph, T source, T sink)
    {
        //init parent lookup table to trace path
        var parentLookUp = new Dictionary<WeightedDiGraphVertex<T, TW>, WeightedDiGraphVertex<T, TW>>();
        foreach (var vertex in residualGraph.Vertices) parentLookUp.Add(vertex.Value, null);

        //regular BFS stuff
        var queue = new Queue<WeightedDiGraphVertex<T, TW>>();
        var visited = new HashSet<WeightedDiGraphVertex<T, TW>>();
        queue.Enqueue(residualGraph.Vertices[source]);
        visited.Add(residualGraph.Vertices[source]);

        WeightedDiGraphVertex<T, TW> currentVertex = null;

        while (queue.Count > 0)
        {
            currentVertex = queue.Dequeue();

            //reached sink? then break otherwise dig in
            if (currentVertex.Key.Equals(sink)) break;

            foreach (var edge in currentVertex.OutEdges)
                //visit only if edge have available flow
                if (!visited.Contains(edge.Key)
                    && edge.Value.CompareTo(@operator.DefaultWeight) > 0)
                {
                    //keep track of this to trace out path once sink is found
                    parentLookUp[edge.Key] = currentVertex;
                    queue.Enqueue(edge.Key);
                    visited.Add(edge.Key);
                }
        }

        //could'nt find a path
        if (currentVertex == null || !currentVertex.Key.Equals(sink)) return null;

        //traverse back from sink to find path to source
        var path = new Stack<T>();

        path.Push(sink);

        while (currentVertex != null && !currentVertex.Key.Equals(source))
        {
            path.Push(parentLookUp[currentVertex].Key);
            currentVertex = parentLookUp[currentVertex];
        }

        //now reverse the stack to get the path from source to sink
        var result = new List<T>();

        while (path.Count > 0) result.Add(path.Pop());

        return result;
    }

    /// <summary>
    ///     Clones this graph and creates a residual graph.
    /// </summary>
    private WeightedDiGraph<T, TW> CreateResidualGraph(IDiGraph<T> graph)
    {
        var newGraph = new WeightedDiGraph<T, TW>();

        //clone graph vertices
        foreach (var vertex in graph.VerticesAsEnumerable) newGraph.AddVertex(vertex.Key);

        //clone edges
        foreach (var vertex in graph.VerticesAsEnumerable)
            //Use either OutEdges or InEdges for cloning
            //here we use OutEdges
        foreach (var edge in vertex.OutEdges)
        {
            //original edge
            newGraph.AddEdge(vertex.Key, edge.TargetVertex.Key, edge.Weight<TW>());
            //add a backward edge for residual graph with edge value as default(W)
            newGraph.AddEdge(edge.TargetVertex.Key, vertex.Key, default);
        }

        return newGraph;
    }
}
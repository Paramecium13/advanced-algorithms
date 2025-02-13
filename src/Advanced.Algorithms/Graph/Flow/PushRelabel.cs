﻿using System;
using System.Collections.Generic;
using System.Linq;
using Advanced.Algorithms.DataStructures.Graph;
using Advanced.Algorithms.DataStructures.Graph.AdjacencyList;

namespace Advanced.Algorithms.Graph;

/// <summary>
///     A Push-Relabel algorithm implementation.
/// </summary>
public class PushRelabelMaxFlow<T, TW> where TW : IComparable
{
    private readonly IFlowOperators<TW> @operator;

    public PushRelabelMaxFlow(IFlowOperators<TW> @operator)
    {
        this.@operator = @operator;
    }

    /// <summary>
    ///     Computes Max Flow using Push-Relabel algorithm.
    /// </summary>
    public TW ComputeMaxFlow(IDiGraph<T> graph,
        T source, T sink)
    {
        if (@operator == null)
            throw new ArgumentException("Provide an operator implementation for generic type W during initialization.");

        if (!graph.IsWeightedGraph)
            if (@operator.DefaultWeight.GetType() != typeof(int))
                throw new ArgumentException("Edges of unweighted graphs are assigned an imaginary weight of one (1)." +
                                            "Provide an appropriate IFlowOperators<int> operator implementation during initialization.");

        //clone to create a residual graph
        var residualGraph = CreateResidualGraph(graph);

        //init vertex Height and Overflow object (ResidualGraphVertexStatus)
        var vertexStatusMap = new Dictionary<T, ResidualGraphVertexStatus>();
        foreach (var vertex in residualGraph.Vertices)
            if (vertex.Value.Key.Equals(source))
                //for source vertex
                //init source height to Maximum (equal to total vertex count)
                vertexStatusMap.Add(vertex.Value.Key,
                    new ResidualGraphVertexStatus(residualGraph.Vertices.Count,
                        @operator.DefaultWeight));
            else
                vertexStatusMap.Add(vertex.Value.Key,
                    new ResidualGraphVertexStatus(0,
                        @operator.DefaultWeight));

        //init source neighbour overflow to capacity of source-neighbour edges
        foreach (var edge in residualGraph.Vertices[source].OutEdges.ToList())
        {
            //update edge vertex overflow
            vertexStatusMap[edge.Key.Key].Overflow = edge.Value;

            //increment reverse edge
            residualGraph.Vertices[edge.Key.Key]
                .OutEdges[residualGraph.Vertices[source]] = edge.Value;

            //set to minimum
            residualGraph.Vertices[source].OutEdges[edge.Key] = @operator.DefaultWeight;
        }

        var overflowVertex = FindOverflowVertex(vertexStatusMap, source, sink);

        //until there is not more overflow vertices
        while (!overflowVertex.Equals(default(T)))
        {
            //if we can't push this vertex
            if (!Push(residualGraph.Vertices[overflowVertex], vertexStatusMap))
                //increase its height and try again
                Relabel(residualGraph.Vertices[overflowVertex], vertexStatusMap);

            overflowVertex = FindOverflowVertex(vertexStatusMap, source, sink);
        }

        //overflow of sink will be the net flow
        return vertexStatusMap[sink].Overflow;
    }

    /// <summary>
    ///     Increases the height of a vertex by one greater than min height of neighbours.
    /// </summary>
    private void Relabel(WeightedDiGraphVertex<T, TW> vertex,
        Dictionary<T, ResidualGraphVertexStatus> vertexStatusMap)
    {
        var min = int.MaxValue;

        foreach (var edge in vertex.OutEdges)
            //+ive out capacity  
            if (min.CompareTo(vertexStatusMap[edge.Key.Key].Height) > 0
                && edge.Value.CompareTo(@operator.DefaultWeight) > 0)
                min = vertexStatusMap[edge.Key.Key].Height;

        vertexStatusMap[vertex.Key].Height = min + 1;
    }

    /// <summary>
    ///     Tries to Push the overflow in current vertex to neighbours if possible.
    ///     Push is possible if neighbour edge is not full
    ///     and any of neighbour has height of current vertex
    ///     otherwise returns false.
    /// </summary>
    private bool Push(WeightedDiGraphVertex<T, TW> overflowVertex,
        Dictionary<T, ResidualGraphVertexStatus> vertexStatusMap)
    {
        var overflow = vertexStatusMap[overflowVertex.Key].Overflow;

        foreach (var edge in overflowVertex.OutEdges)
            //if out edge has +ive weight and neighbour height is less then flow is possible
            if (edge.Value.CompareTo(@operator.DefaultWeight) > 0
                && vertexStatusMap[edge.Key.Key].Height
                < vertexStatusMap[overflowVertex.Key].Height)
            {
                var possibleWeightToPush = edge.Value.CompareTo(overflow) < 0 ? edge.Value : overflow;

                //decrement overflow
                vertexStatusMap[overflowVertex.Key].Overflow =
                    @operator.SubstractWeights(vertexStatusMap[overflowVertex.Key].Overflow,
                        possibleWeightToPush);

                //increment flow of target vertex
                vertexStatusMap[edge.Key.Key].Overflow =
                    @operator.AddWeights(vertexStatusMap[edge.Key.Key].Overflow,
                        possibleWeightToPush);

                //decrement edge weight
                overflowVertex.OutEdges[edge.Key] = @operator.SubstractWeights(edge.Value, possibleWeightToPush);
                //increment reverse edge weight
                edge.Key.OutEdges[overflowVertex] =
                    @operator.AddWeights(edge.Key.OutEdges[overflowVertex], possibleWeightToPush);

                return true;
            }

        return false;
    }

    /// <summary>
    ///     Returns a vertex with an overflow.
    /// </summary>
    private T FindOverflowVertex(Dictionary<T, ResidualGraphVertexStatus> vertexStatusMap,
        T source, T sink)
    {
        foreach (var vertexStatus in vertexStatusMap)
            //ignore source and sink (which can have non-zero overflow)
            if (!vertexStatus.Key.Equals(source) && !vertexStatus.Key.Equals(sink) &&
                vertexStatus.Value.Overflow.CompareTo(@operator.DefaultWeight) > 0)
                return vertexStatus.Key;

        return default;
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
            newGraph.AddEdge(vertex.Key, edge.TargetVertexKey, edge.Weight<TW>());
            //add a backward edge for residual graph with edge value as default(W)
            newGraph.AddEdge(edge.TargetVertexKey, vertex.Key, default);
        }

        return newGraph;
    }

    /// <summary>
    ///     An object to keep track of Vertex Overflow and Height.
    /// </summary>
    internal class ResidualGraphVertexStatus
    {
        public ResidualGraphVertexStatus(int height, TW overflow)
        {
            Height = height;
            Overflow = overflow;
        }

        /// <summary>
        ///     Current overflow in this vertex.
        /// </summary>
        public TW Overflow { get; set; }

        /// <summary>
        ///     Current height of the vertex.
        /// </summary>
        public int Height { get; set; }
    }
}
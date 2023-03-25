﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Advanced.Algorithms.DataStructures.Graph.AdjacencyList;

/// <summary>
///     A directed graph implementation.
///     IEnumerable enumerates all vertices.
/// </summary>
public class DiGraph<T> : IGraph<T>, IDiGraph<T>, IEnumerable<T>
{
    public DiGraph()
    {
        Vertices = new Dictionary<T, DiGraphVertex<T>>();
    }

    private Dictionary<T, DiGraphVertex<T>> Vertices { get; }

    /// <summary>
    ///     Return a reference vertex to start traversing Vertices
    ///     Time complexity: O(1).
    /// </summary>
    private DiGraphVertex<T> ReferenceVertex
    {
        get
        {
            using (var enumerator = Vertices.GetEnumerator())
            {
                if (enumerator.MoveNext()) return enumerator.Current.Value;
            }

            return null;
        }
    }

    IDiGraphVertex<T> IDiGraph<T>.ReferenceVertex => ReferenceVertex;

    public IDiGraphVertex<T> GetVertex(T value)
    {
        return Vertices[value];
    }

    IDiGraph<T> IDiGraph<T>.Clone()
    {
        return Clone();
    }

    IEnumerable<IDiGraphVertex<T>> IDiGraph<T>.VerticesAsEnumerable => Vertices.Select(x => x.Value);

    public IEnumerator GetEnumerator()
    {
        return Vertices.Select(x => x.Key).GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator() as IEnumerator<T>;
    }

    public int VerticesCount => Vertices.Count;
    public bool IsWeightedGraph => false;
    IGraphVertex<T> IGraph<T>.ReferenceVertex => ReferenceVertex;

    /// <summary>
    ///     Do we have an edge between the given source and destination?
    ///     Time complexity: O(1).
    /// </summary>
    public bool HasEdge(T source, T dest)
    {
        if (!Vertices.ContainsKey(source) || !Vertices.ContainsKey(dest))
            throw new ArgumentException("source or destination is not in this graph.");

        return Vertices[source].OutEdges.Contains(Vertices[dest])
               && Vertices[dest].InEdges.Contains(Vertices[source]);
    }

    public bool ContainsVertex(T value)
    {
        return Vertices.ContainsKey(value);
    }

    IGraphVertex<T> IGraph<T>.GetVertex(T key)
    {
        return Vertices[key];
    }

    IGraph<T> IGraph<T>.Clone()
    {
        return Clone();
    }

    public IEnumerable<IGraphVertex<T>> VerticesAsEnumberable => Vertices.Select(x => x.Value);


    /// <summary>
    ///     Add a new vertex to this graph.
    ///     Time complexity: O(1).
    /// </summary>
    public void AddVertex(T value)
    {
        if (value == null) throw new ArgumentNullException();

        var newVertex = new DiGraphVertex<T>(value);

        Vertices.Add(value, newVertex);
    }

    /// <summary>
    ///     Remove an existing vertex frm graph.
    ///     Time complexity: O(V) where V is the total number of vertices in this graph.
    /// </summary>
    public void RemoveVertex(T value)
    {
        if (value == null) throw new ArgumentNullException();

        if (!Vertices.ContainsKey(value)) throw new Exception("Vertex not in this graph.");

        foreach (var vertex in Vertices[value].InEdges) vertex.OutEdges.Remove(Vertices[value]);

        foreach (var vertex in Vertices[value].OutEdges) vertex.InEdges.Remove(Vertices[value]);

        Vertices.Remove(value);
    }

    /// <summary>
    ///     Add an edge from source to destination vertex.
    ///     Time complexity: O(1).
    /// </summary>
    public void AddEdge(T source, T dest)
    {
        if (source == null || dest == null) throw new ArgumentException();

        if (!Vertices.ContainsKey(source) || !Vertices.ContainsKey(dest))
            throw new Exception("Source or Destination Vertex is not in this graph.");

        if (Vertices[source].OutEdges.Contains(Vertices[dest]) || Vertices[dest].InEdges.Contains(Vertices[source]))
            throw new Exception("Edge already exists.");

        Vertices[source].OutEdges.Add(Vertices[dest]);
        Vertices[dest].InEdges.Add(Vertices[source]);
    }

    /// <summary>
    ///     Remove an existing edge between source and destination.
    ///     Time complexity: O(1).
    /// </summary>
    public void RemoveEdge(T source, T dest)
    {
        if (source == null || dest == null) throw new ArgumentException();

        if (!Vertices.ContainsKey(source) || !Vertices.ContainsKey(dest))
            throw new Exception("Source or Destination Vertex is not in this graph.");

        if (!Vertices[source].OutEdges.Contains(Vertices[dest])
            || !Vertices[dest].InEdges.Contains(Vertices[source]))
            throw new Exception("Edge do not exists.");

        Vertices[source].OutEdges.Remove(Vertices[dest]);
        Vertices[dest].InEdges.Remove(Vertices[source]);
    }

    public IEnumerable<T> OutEdges(T vertex)
    {
        if (!Vertices.ContainsKey(vertex)) throw new ArgumentException("vertex is not in this graph.");

        return Vertices[vertex].OutEdges.Select(x => x.Key);
    }

    public IEnumerable<T> InEdges(T vertex)
    {
        if (!Vertices.ContainsKey(vertex)) throw new ArgumentException("vertex is not in this graph.");

        return Vertices[vertex].InEdges.Select(x => x.Key);
    }

    /// <summary>
    ///     Clones this graph.
    /// </summary>
    public DiGraph<T> Clone()
    {
        var newGraph = new DiGraph<T>();

        foreach (var vertex in Vertices) newGraph.AddVertex(vertex.Key);

        foreach (var vertex in Vertices)
        foreach (var edge in vertex.Value.OutEdges)
            newGraph.AddEdge(vertex.Value.Key, edge.Key);

        return newGraph;
    }
}

internal class DiGraphVertex<T> : IDiGraphVertex<T>, IGraphVertex<T>, IEnumerable<T>
{
    public DiGraphVertex(T value)
    {
        Key = value;
        OutEdges = new HashSet<DiGraphVertex<T>>();
        InEdges = new HashSet<DiGraphVertex<T>>();
    }

    public HashSet<DiGraphVertex<T>> OutEdges { get; }
    public HashSet<DiGraphVertex<T>> InEdges { get; }
    public T Key { get; set; }

    IEnumerable<IDiEdge<T>> IDiGraphVertex<T>.OutEdges => OutEdges.Select(x => new DiEdge<T, int>(x, 1));
    IEnumerable<IDiEdge<T>> IDiGraphVertex<T>.InEdges => InEdges.Select(x => new DiEdge<T, int>(x, 1));

    public int OutEdgeCount => OutEdges.Count;
    public int InEdgeCount => InEdges.Count;

    public IDiEdge<T> GetOutEdge(IDiGraphVertex<T> targetVertex)
    {
        return new DiEdge<T, int>(targetVertex, 1);
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return OutEdges.Select(x => x.Key).GetEnumerator();
    }

    IEnumerable<IEdge<T>> IGraphVertex<T>.Edges => OutEdges.Select(x => new Edge<T, int>(x, 1));

    public IEdge<T> GetEdge(IGraphVertex<T> targetVertex)
    {
        return new Edge<T, int>(targetVertex, 1);
    }
}
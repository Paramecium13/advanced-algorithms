﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Advanced.Algorithms.DataStructures.Graph.AdjacencyMatrix;

/// <summary>
///     A directed graph implementation using dynamically growing/shrinking adjacency matrix array.
///     IEnumerable enumerates all vertices.
/// </summary>
public class DiGraph<T> : IGraph<T>, IDiGraph<T>, IEnumerable<T>
{
    private BitArray[] matrix;
    private int nextAvailableIndex;
    private Dictionary<int, T> reverseVertexIndices;

    private Dictionary<T, int> vertexIndices;
    private readonly Dictionary<T, DiGraphVertex<T>> vertexObjects;

    public DiGraph()
    {
        vertexIndices = new Dictionary<T, int>();
        reverseVertexIndices = new Dictionary<int, T>();
        matrix = new BitArray[1];
        vertexObjects = new Dictionary<T, DiGraphVertex<T>>();

        for (var i = 0; i < MaxSize; i++) matrix[i] = new BitArray(MaxSize);
    }

    private int MaxSize => matrix.Length;
    IDiGraphVertex<T> IDiGraph<T>.ReferenceVertex => GetReferenceVertex();

    IDiGraphVertex<T> IDiGraph<T>.GetVertex(T key)
    {
        return vertexObjects[key];
    }

    public IEnumerable<IDiGraphVertex<T>> VerticesAsEnumerable => GetVerticesAsEnumerable();

    IDiGraph<T> IDiGraph<T>.Clone()
    {
        return Clone();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return vertexIndices.Select(x => x.Key).GetEnumerator();
    }

    public int VerticesCount { get; private set; }

    public bool IsWeightedGraph => false;

    public IGraphVertex<T> ReferenceVertex => GetReferenceVertex();

    public bool ContainsVertex(T key)
    {
        return vertexIndices.ContainsKey(key);
    }

    public IGraphVertex<T> GetVertex(T key)
    {
        return vertexObjects[key];
    }

    /// <summary>
    ///     do we have an edge between the given source and destination?
    ///     Time complexity: O(1).
    /// </summary>
    public bool HasEdge(T source, T dest)
    {
        if (source == null || dest == null) throw new ArgumentException();

        if (!vertexIndices.ContainsKey(source) || !vertexIndices.ContainsKey(dest))
            throw new Exception("Source or destination vertex does'nt exist.");

        var sourceIndex = vertexIndices[source];
        var destIndex = vertexIndices[dest];

        return matrix[sourceIndex].Get(destIndex);
    }

    IEnumerable<IGraphVertex<T>> IGraph<T>.VerticesAsEnumberable => GetVerticesAsEnumerable();

    IGraph<T> IGraph<T>.Clone()
    {
        return Clone();
    }

    private DiGraphVertex<T> GetReferenceVertex()
    {
        if (VerticesCount == 0) throw new Exception("Empty graph.");

        return vertexObjects[this.First()];
    }

    /// <summary>
    ///     Add a new vertex to this graph.
    ///     Time complexity: O(1).
    /// </summary>
    public void AddVertex(T value)
    {
        if (value == null) throw new ArgumentNullException();

        if (vertexIndices.ContainsKey(value)) throw new Exception("Vertex exists.");

        if (VerticesCount < MaxSize / 2) HalfMatrixSize();

        if (nextAvailableIndex == MaxSize) DoubleMatrixSize();

        vertexIndices.Add(value, nextAvailableIndex);
        reverseVertexIndices.Add(nextAvailableIndex, value);
        vertexObjects.Add(value, new DiGraphVertex<T>(this, value));

        nextAvailableIndex++;
        VerticesCount++;
    }

    /// <summary>
    ///     Remove an existing vertex from graph
    ///     Time complexity: O(V) where V is the number of vertices.
    /// </summary>
    public void RemoveVertex(T value)
    {
        if (value == null) throw new ArgumentNullException();

        if (!vertexIndices.ContainsKey(value)) throw new Exception("Vertex does'nt exist.");

        if (VerticesCount <= MaxSize / 2) HalfMatrixSize();

        var index = vertexIndices[value];

        //clear edges
        for (var i = 0; i < MaxSize; i++)
        {
            matrix[i].Set(index, false);
            matrix[index].Set(i, false);
        }

        reverseVertexIndices.Remove(index);
        vertexIndices.Remove(value);
        vertexObjects.Remove(value);

        VerticesCount--;
    }

    /// <summary>
    ///     add an edge from source to destination vertex
    ///     Time complexity: O(1).
    /// </summary>
    public void AddEdge(T source, T dest)
    {
        if (source == null || dest == null) throw new ArgumentException();

        if (!vertexIndices.ContainsKey(source) || !vertexIndices.ContainsKey(dest))
            throw new Exception("Source or destination vertex does'nt exist.");

        var sourceIndex = vertexIndices[source];
        var destIndex = vertexIndices[dest];
        if (matrix[sourceIndex].Get(destIndex)) throw new Exception("Edge already exists.");

        matrix[sourceIndex].Set(destIndex, true);
    }

    /// <summary>
    ///     remove an existing edge between source and destination
    ///     Time complexity: O(1).
    /// </summary>
    public void RemoveEdge(T source, T dest)
    {
        if (source == null || dest == null) throw new ArgumentException();

        if (!vertexIndices.ContainsKey(source) || !vertexIndices.ContainsKey(dest))
            throw new Exception("Source or destination vertex does'nt exist.");

        var sourceIndex = vertexIndices[source];
        var destIndex = vertexIndices[dest];
        if (!matrix[sourceIndex].Get(destIndex)) throw new Exception("Edge do not exists.");

        matrix[sourceIndex].Set(destIndex, false);
    }

    public IEnumerable<T> OutEdges(T vertex)
    {
        if (!vertexIndices.ContainsKey(vertex)) throw new ArgumentException("vertex is not in this graph.");

        var index = vertexIndices[vertex];

        var result = new List<T>();

        for (var i = 0; i < MaxSize; i++)
            if (matrix[index].Get(i))
                yield return reverseVertexIndices[i];
    }

    public int OutEdgeCount(T vertex)
    {
        if (!vertexIndices.ContainsKey(vertex)) throw new ArgumentException("vertex is not in this graph.");

        var index = vertexIndices[vertex];

        var count = 0;

        for (var i = 0; i < MaxSize; i++)
            if (matrix[index].Get(i))
                count++;

        return count;
    }

    public IEnumerable<T> InEdges(T vertex)
    {
        if (!vertexIndices.ContainsKey(vertex)) throw new ArgumentException("vertex is not in this graph.");

        var index = vertexIndices[vertex];

        var result = new List<T>();

        for (var i = 0; i < MaxSize; i++)
            if (matrix[i].Get(index))
                yield return reverseVertexIndices[i];
    }

    public int InEdgeCount(T vertex)
    {
        if (!vertexIndices.ContainsKey(vertex)) throw new ArgumentException("vertex is not in this graph.");

        var index = vertexIndices[vertex];

        var count = 0;

        for (var i = 0; i < MaxSize; i++)
            if (matrix[i].Get(index))
                count++;

        return count;
    }

    private void DoubleMatrixSize()
    {
        var newMatrix = new BitArray[MaxSize * 2];
        for (var i = 0; i < MaxSize * 2; i++) newMatrix[i] = new BitArray(MaxSize * 2);

        var newVertexIndices = new Dictionary<T, int>();
        var newReverseIndices = new Dictionary<int, T>();

        var k = 0;
        foreach (var vertex in vertexIndices)
        {
            newVertexIndices.Add(vertex.Key, k);
            newReverseIndices.Add(k, vertex.Key);
            k++;
        }

        nextAvailableIndex = k;

        for (var i = 0; i < MaxSize; i++)
        {
            newMatrix[i] = new BitArray(MaxSize * 2);
            for (var j = 0; j < MaxSize; j++)
            {
                if (!matrix[i].Get(j) || !reverseVertexIndices.ContainsKey(i) ||
                    !reverseVertexIndices.ContainsKey(j))
                    continue;

                var newI = newVertexIndices[reverseVertexIndices[i]];
                var newJ = newVertexIndices[reverseVertexIndices[j]];

                newMatrix[newI].Set(newJ, true);
            }
        }

        matrix = newMatrix;
        vertexIndices = newVertexIndices;
        reverseVertexIndices = newReverseIndices;
    }

    private void HalfMatrixSize()
    {
        var newMatrix = new BitArray[MaxSize / 2];
        for (var i = 0; i < MaxSize / 2; i++) newMatrix[i] = new BitArray(MaxSize / 2);

        var newVertexIndices = new Dictionary<T, int>();
        var newReverseIndices = new Dictionary<int, T>();

        var k = 0;
        foreach (var vertex in vertexIndices)
        {
            newVertexIndices.Add(vertex.Key, k);
            newReverseIndices.Add(k, vertex.Key);
            k++;
        }

        nextAvailableIndex = k;

        for (var i = 0; i < MaxSize; i++)
        for (var j = 0; j < MaxSize; j++)
        {
            if (!matrix[i].Get(j) || !reverseVertexIndices.ContainsKey(i) ||
                !reverseVertexIndices.ContainsKey(j))
                continue;

            var newI = newVertexIndices[reverseVertexIndices[i]];
            var newJ = newVertexIndices[reverseVertexIndices[j]];

            newMatrix[newI].Set(newJ, true);
        }

        matrix = newMatrix;
        vertexIndices = newVertexIndices;
        reverseVertexIndices = newReverseIndices;
    }

    private IEnumerable<DiGraphVertex<T>> GetVerticesAsEnumerable()
    {
        return this.Select(x => vertexObjects[x]);
    }

    public DiGraph<T> Clone()
    {
        var graph = new DiGraph<T>();

        foreach (var vertex in this) graph.AddVertex(vertex);

        foreach (var vertex in this)
        foreach (var edge in OutEdges(vertex))
            graph.AddEdge(vertex, edge);

        return graph;
    }

    private class DiGraphVertex<T> : IDiGraphVertex<T>, IGraphVertex<T>
    {
        private readonly DiGraph<T> graph;
        private int vertexIndex;

        internal DiGraphVertex(DiGraph<T> graph, T vertexKey)
        {
            if (!graph.vertexIndices.ContainsKey(vertexKey))
                throw new ArgumentException("vertex is not in this graph.");

            this.graph = graph;
            Key = vertexKey;
            vertexIndex = graph.vertexIndices[vertexKey];
        }

        private int MaxSize => graph.MaxSize;
        private BitArray[] Matrix => graph.matrix;

        private Dictionary<T, int> VertexIndices => graph.vertexIndices;
        private Dictionary<int, T> ReverseVertexIndices => graph.reverseVertexIndices;

        public T Key { get; }

        IEnumerable<IDiEdge<T>> IDiGraphVertex<T>.OutEdges => graph.OutEdges(Key)
            .Select(x => new DiEdge<T, int>(graph.vertexObjects[x], 1));

        IEnumerable<IDiEdge<T>> IDiGraphVertex<T>.InEdges => graph.InEdges(Key)
            .Select(x => new DiEdge<T, int>(graph.vertexObjects[x], 1));

        public int OutEdgeCount => graph.OutEdgeCount(Key);
        public int InEdgeCount => graph.InEdgeCount(Key);

        public IDiEdge<T> GetOutEdge(IDiGraphVertex<T> targetVertex)
        {
            if (!VertexIndices.ContainsKey(targetVertex.Key))
                throw new ArgumentException("vertex is not in this graph.");

            var index = VertexIndices[targetVertex.Key];
            var key = targetVertex as DiGraphVertex<T>;
            return new DiEdge<T, int>(targetVertex, 1);
        }

        IEnumerable<IEdge<T>> IGraphVertex<T>.Edges => graph.OutEdges(Key)
            .Select(x => new Edge<T, int>(graph.vertexObjects[x], 1));

        public IEdge<T> GetEdge(IGraphVertex<T> targetVertex)
        {
            if (!VertexIndices.ContainsKey(targetVertex.Key))
                throw new ArgumentException("vertex is not in this graph.");

            var index = VertexIndices[targetVertex.Key];
            var key = targetVertex as DiGraphVertex<T>;
            return new Edge<T, int>(targetVertex, 1);
        }
    }
}
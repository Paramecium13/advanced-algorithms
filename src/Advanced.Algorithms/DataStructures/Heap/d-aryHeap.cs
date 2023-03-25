﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Advanced.Algorithms.DataStructures;

/// <summary>
///     A D-ary minMax heap implementation.
/// </summary>
public class DaryHeap<T> : IEnumerable<T> where T : IComparable
{
    private readonly IComparer<T> comparer;
    private readonly bool isMaxHeap;
    public int Count;

    private T[] heapArray;
    private readonly int k;

    /// <summary>
    ///     Time complexity: O(n) when initial is provided otherwise O(1).
    /// </summary>
    /// <param name="k">The number of children per heap node.</param>
    /// <param name="initial">The initial items if any.</param>
    public DaryHeap(int k, SortDirection sortDirection = SortDirection.Ascending, IEnumerable<T> initial = null)
    {
        isMaxHeap = sortDirection == SortDirection.Descending;
        comparer = new CustomComparer<T>(sortDirection, Comparer<T>.Default);

        if (k <= 2) throw new Exception("Number of nodes k must be greater than 2.");

        this.k = k;

        if (initial != null)
        {
            var items = initial as T[] ?? initial.ToArray();
            var initArray = new T[items.Length];

            var i = 0;
            foreach (var item in items)
            {
                initArray[i] = item;
                i++;
            }

            Count = initArray.Length;
            BulkInit(initArray);
        }
        else
        {
            heapArray = new T[k];
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return heapArray.Take(Count).GetEnumerator();
    }

    /// <summary>
    ///     Initialize with given input.
    ///     Time complexity: O(n).
    /// </summary>
    private void BulkInit(T[] initial)
    {
        var i = (initial.Length - 1) / k;

        while (i >= 0)
        {
            BulkInitRecursive(i, initial);
            i--;
        }

        heapArray = initial;
    }

    /// <summary>
    ///     Recursively load bulk init values.
    /// </summary>
    private void BulkInitRecursive(int i, T[] initial)
    {
        var parent = i;
        var minMax = FindMinMaxChildIndex(i, initial);

        if (minMax != -1 && comparer.Compare(initial[minMax], initial[parent]) < 0)
        {
            (initial[minMax], initial[parent]) = (initial[parent], initial[minMax]);

            BulkInitRecursive(minMax, initial);
        }
    }


    /// <summary>
    ///     Time complexity: O(log(n) base K).
    /// </summary>
    public void Insert(T newItem)
    {
        if (Count == heapArray.Length) DoubleArray();

        heapArray[Count] = newItem;

        //percolate up
        for (var i = Count; i > 0; i = (i - 1) / k)
            if (comparer.Compare(heapArray[i], heapArray[(i - 1) / k]) < 0)
            {
                (heapArray[(i - 1) / k], heapArray[i]) = (heapArray[i], heapArray[(i - 1) / k]);
            }
            else
            {
                break;
            }

        Count++;
    }

    /// <summary>
    ///     Time complexity: O(log(n) base K).
    /// </summary>
    public T Extract()
    {
        if (Count == 0) throw new Exception("Empty heap");
        var minMax = heapArray[0];

        //move last element to top
        heapArray[0] = heapArray[Count - 1];
        Count--;

        var currentParent = 0;
        //now percolate down
        while (true)
        {
            var swapped = false;

            //init to left-most child
            var minMaxChildIndex = FindMinMaxChildIndex(currentParent, heapArray);

            if (minMaxChildIndex != -1 &&
                comparer.Compare(heapArray[currentParent], heapArray[minMaxChildIndex]) > 0)
            {
                (heapArray[minMaxChildIndex], heapArray[currentParent]) = (heapArray[currentParent], heapArray[minMaxChildIndex]);
                swapped = true;
            }

            if (!swapped) break;

            currentParent = minMaxChildIndex;
        }

        if (heapArray.Length / 2 == Count && heapArray.Length > 2) HalfArray();

        return minMax;
    }

    /// <summary>
    ///     Returns the max Index of child if any.
    ///     Otherwise returns -1.
    /// </summary>
    private int FindMinMaxChildIndex(int currentParent, T[] heap)
    {
        var currentMinMax = currentParent * k + 1;

        if (currentMinMax >= Count)
            return -1;

        for (var i = 2; i <= k; i++)
        {
            if (currentParent * k + i >= Count)
                break;

            var nextSibling = heap[currentParent * k + i];

            if (comparer.Compare(heap[currentMinMax], nextSibling) > 0) currentMinMax = currentParent * k + i;
        }

        return currentMinMax;
    }

    /// <summary>
    ///     Time complexity: O(1).
    /// </summary>
    public T Peek()
    {
        if (Count == 0) throw new Exception("Empty heap");

        return heapArray[0];
    }

    private void HalfArray()
    {
        var smallerArray = new T[heapArray.Length / 2];

        for (var i = 0; i < Count; i++) smallerArray[i] = heapArray[i];

        heapArray = smallerArray;
    }

    private void DoubleArray()
    {
        var biggerArray = new T[heapArray.Length * 2];

        for (var i = 0; i < Count; i++) biggerArray[i] = heapArray[i];

        heapArray = biggerArray;
    }
}
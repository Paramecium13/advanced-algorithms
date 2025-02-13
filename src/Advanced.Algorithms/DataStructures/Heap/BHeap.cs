﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Advanced.Algorithms.DataStructures;

/// <summary>
///     A binary heap implementation.
/// </summary>
public class BHeap<T> : IEnumerable<T> where T : IComparable
{
    private readonly IComparer<T> comparer;
    private readonly bool isMaxHeap;

    private T[] heapArray;

    public BHeap(SortDirection sortDirection = SortDirection.Ascending)
        : this(sortDirection, null, null)
    {
    }

    public BHeap(SortDirection sortDirection, IEnumerable<T> initial)
        : this(sortDirection, initial, null)
    {
    }

    public BHeap(SortDirection sortDirection, IComparer<T> comparer)
        : this(sortDirection, null, comparer)
    {
    }

    /// <summary>
    ///     Time complexity: O(n) if initial is provided. Otherwise O(1).
    /// </summary>
    /// <param name="initial">The initial items in the heap.</param>
    public BHeap(SortDirection sortDirection, IEnumerable<T> initial, IComparer<T> comparer)
    {
        isMaxHeap = sortDirection == SortDirection.Descending;

        if (comparer != null)
            this.comparer = new CustomComparer<T>(sortDirection, comparer);
        else
            this.comparer = new CustomComparer<T>(sortDirection, Comparer<T>.Default);

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

            BulkInit(initArray);
            Count = initArray.Length;
        }
        else
        {
            heapArray = new T[2];
        }
    }

    public int Count { get; private set; }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<T> GetEnumerator()
    {
        return heapArray.Take(Count).GetEnumerator();
    }

    private void BulkInit(T[] initial)
    {
        var i = (initial.Length - 1) / 2;

        while (i >= 0)
        {
            BulkInitRecursive(i, initial);
            i--;
        }

        heapArray = initial;
    }

    private void BulkInitRecursive(int i, T[] initial)
    {
        var parent = i;

        var left = 2 * i + 1;
        var right = 2 * i + 2;

        var minMax = left < initial.Length && right < initial.Length
            ? comparer.Compare(initial[left], initial[right]) < 0 ? left : right
            : left < initial.Length
                ? left
                : right < initial.Length
                    ? right
                    : -1;

        if (minMax != -1 && comparer.Compare(initial[minMax], initial[parent]) < 0)
        {
            (initial[minMax], initial[parent]) = (initial[parent], initial[minMax]);

            //if min is child then drill down child
            BulkInitRecursive(minMax, initial);
        }
    }

    /// <summary>
    ///     Time complexity: O(log(n)).
    /// </summary>
    public void Insert(T newItem)
    {
        if (Count == heapArray.Length) DoubleArray();

        heapArray[Count] = newItem;

        for (var i = Count; i > 0; i = (i - 1) / 2)
            if (comparer.Compare(heapArray[i], heapArray[(i - 1) / 2]) < 0)
            {
                (heapArray[(i - 1) / 2], heapArray[i]) = (heapArray[i], heapArray[(i - 1) / 2]);
            }
            else
            {
                break;
            }

        Count++;
    }

    /// <summary>
    ///     Time complexity: O(log(n)).
    /// </summary>
    public T Extract()
    {
        if (Count == 0) throw new Exception("Empty heap");

        var minMax = heapArray[0];

        Delete(0);

        return minMax;
    }

    /// <summary>
    ///     Time complexity: O(1).
    /// </summary>
    public T Peek()
    {
        if (Count == 0) throw new Exception("Empty heap");

        return heapArray[0];
    }

    /// <summary>
    ///     Time complexity: O(n).
    /// </summary>
    public void Delete(T value)
    {
        var index = FindIndex(value);

        if (index != -1)
        {
            Delete(index);
            return;
        }

        throw new Exception("Item not found.");
    }

    /// <summary>
    ///     Time complexity: O(n).
    /// </summary>
    public bool Exists(T value)
    {
        return FindIndex(value) != -1;
    }

    private void Delete(int parentIndex)
    {
        heapArray[parentIndex] = heapArray[Count - 1];
        Count--;

        //percolate down
        while (true)
        {
            var leftIndex = 2 * parentIndex + 1;
            var rightIndex = 2 * parentIndex + 2;

            var parent = heapArray[parentIndex];

            if (leftIndex < Count && rightIndex < Count)
            {
                var leftChild = heapArray[leftIndex];
                var rightChild = heapArray[rightIndex];

                var leftIsMinMax = false;

                if (comparer.Compare(leftChild, rightChild) < 0) leftIsMinMax = true;

                var minMaxChildIndex = leftIsMinMax ? leftIndex : rightIndex;

                if (comparer.Compare(heapArray[minMaxChildIndex], parent) < 0)
                {
                    (heapArray[parentIndex], heapArray[minMaxChildIndex]) = (heapArray[minMaxChildIndex], heapArray[parentIndex]);

                    if (leftIsMinMax)
                        parentIndex = leftIndex;
                    else
                        parentIndex = rightIndex;
                }
                else
                {
                    break;
                }
            }
            else if (leftIndex < Count)
            {
                if (comparer.Compare(heapArray[leftIndex], parent) < 0)
                {
                    (heapArray[parentIndex], heapArray[leftIndex]) = (heapArray[leftIndex], heapArray[parentIndex]);

                    parentIndex = leftIndex;
                }
                else
                {
                    break;
                }
            }
            else if (rightIndex < Count)
            {
                if (comparer.Compare(heapArray[rightIndex], parent) < 0)
                {
                    (heapArray[parentIndex], heapArray[rightIndex]) = (heapArray[rightIndex], heapArray[parentIndex]);

                    parentIndex = rightIndex;
                }
                else
                {
                    break;
                }
            }
            else
            {
                break;
            }
        }

        if (heapArray.Length / 2 == Count && heapArray.Length > 2) HalfArray();
    }


    private int FindIndex(T value)
    {
        for (var i = 0; i < Count; i++)
            if (heapArray[i].Equals(value))
                return i;
        return -1;
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
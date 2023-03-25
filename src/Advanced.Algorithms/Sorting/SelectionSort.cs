using System;
using System.Collections.Generic;

namespace Advanced.Algorithms.Sorting;

/// <summary>
///     A selection sort implementation.
/// </summary>
public static class SelectionSort<T> where T : IComparable
{
    /// <summary>
    ///     Time complexity: O(n^2).
    /// </summary>
    public static T[] Sort(T[] array, SortDirection sortDirection = SortDirection.Ascending)
    {
        var comparer = new CustomComparer<T>(sortDirection, Comparer<T>.Default);

        for (var i = 0; i < array.Length; i++)
            //select the smallest item in sub array and move it to front
        for (var j = i + 1; j < array.Length; j++)
        {
            if (comparer.Compare(array[j], array[i]) >= 0) continue;

            (array[i], array[j]) = (array[j], array[i]);
        }

        return array;
    }
}
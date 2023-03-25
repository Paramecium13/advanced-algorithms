using System;
using System.Collections.Generic;

namespace Advanced.Algorithms.Sorting;

/// <summary>
///     A bubble sort implementation.
/// </summary>
public static class BubbleSort
{
    /// <summary>
    ///     Time complexity: O(n^2).
    /// </summary>
    public static T[] Sort<T>(T[] array, SortDirection sortDirection = SortDirection.Ascending) where T : IComparable
    {
        var comparer = new CustomComparer<T>(sortDirection, Comparer<T>.Default);
        var swapped = true;

        while (swapped)
        {
            swapped = false;

            for (var i = 0; i < array.Length - 1; i++)
                //compare adjacent elements 
                if (comparer.Compare(array[i], array[i + 1]) > 0)
                {
                    (array[i], array[i + 1]) = (array[i + 1], array[i]);
                    swapped = true;
                }
        }

        return array;
    }
}
﻿using System;
using System.Collections.Generic;

namespace Advanced.Algorithms.Sorting;

/// <summary>
///     An insertion sort implementation.
/// </summary>
public static class InsertionSort<T> where T : IComparable
{
    /// <summary>
    ///     Time complexity: O(n^2).
    /// </summary>
    public static T[] Sort(T[] array, SortDirection sortDirection = SortDirection.Ascending)
    {
        var comparer = new CustomComparer<T>(sortDirection, Comparer<T>.Default);

        for (var i = 0; i < array.Length - 1; i++)
        for (var j = i + 1; j > 0; j--)
            if (comparer.Compare(array[j], array[j - 1]) < 0)
            {
                (array[j - 1], array[j]) = (array[j], array[j - 1]);
            }
            else
            {
                break;
            }

        return array;
    }
}
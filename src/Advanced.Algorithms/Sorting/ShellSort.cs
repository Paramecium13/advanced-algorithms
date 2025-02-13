﻿using System;
using System.Collections.Generic;

namespace Advanced.Algorithms.Sorting;

/// <summary>
///     A shell sort implementation.
/// </summary>
public static class ShellSort<T> where T : IComparable
{
    public static T[] Sort(T[] array, SortDirection sortDirection = SortDirection.Ascending)
    {
        var comparer = new CustomComparer<T>(sortDirection, Comparer<T>.Default);

        var k = array.Length / 2;
        var j = 0;

        while (k >= 1)
        {
            for (var i = k; i < array.Length; i = i + k, j = j + k)
            {
                if (comparer.Compare(array[i], array[j]) >= 0) continue;

                Swap(array, i, j);

                if (i <= k) continue;

                i -= k * 2;
                j -= k * 2;
            }

            j = 0;
            k /= 2;
        }

        return array;
    }

    private static void Swap(T[] array, int i, int j)
    {
        (array[i], array[j]) = (array[j], array[i]);
    }
}
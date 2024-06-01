using System.Collections;

namespace Foreman.Core;

public static class EnumerableExensions {
    /// <summary>
    /// This method is intended to find a position within a list which meets a particular condition. The expectation is that any
    /// list + assert combination always produces a list with some unknown amount of failed assertions followed by some unknown number
    /// of successful assertions. If the full list fails the assertion, then -1 will be returned.
    /// 
    /// For example, the list [1,2,3] assert (> 1) would return:
    /// [false, true, true] ~> 1
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="values">the list to interrogate.</param>
    /// <param name="assert">the assertion to perform.</param>
    /// <param name="from">the optional starting position to search within. All positions before this index will be ignored and could result in a returned value which exists in the middle of a sequence of successful assertions. Use carefully.</param>
    /// <param name="to">the optional ending position to search within. All positions after this index will be ignored and could result in a returned value of -1 even if the assertion is true at some position within the full sequnce. Use carefully.</param>
    /// <returns>The first position which passes the assertion, or -1.</returns>
    public static int IndexWhere<T>(this IList<T> values, Func<T,bool> assert, int? from = null, int? to = null) {
        int left = from ?? 0;
        int right = to ?? values.Count - 1;
        int middle = ((right - left) / 2) + left;

        bool result = assert(values[middle]);

        if (left == right) {
            return result == true ? left : -1;
        }

        if (result) {
            return values.IndexWhere(assert, from: left, to: middle);
        }

        return values.IndexWhere(assert, from: middle + 1, to: right);
    }
}
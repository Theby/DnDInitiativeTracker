using System;
using System.Collections.Generic;
using System.Linq;

namespace DnDInitiativeTracker.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// List must be in format: 1,2,3,4,5,6,7,8,9,10
        /// </summary>
        public static int[] ToIntegerArray(this string textIdList)
        {
            if (string.IsNullOrEmpty(textIdList))
                return Array.Empty<int>();

            var splitIds = textIdList.Split(',');
            return splitIds.Select(int.Parse).ToArray();
        }

        public static string ToIdList<T>(this IEnumerable<T> source, Func<T, int> selector)
        {
            return string.Join(",", source.Select(selector));
        }
    }
}
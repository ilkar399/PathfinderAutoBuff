using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

/*
 * Common claas extensions
 * TODO: Group all common extensions here
 * 
 */

namespace PathfinderAutoBuff.Utility.Extensions
{
    public static class CommonExtensions
    {
        public static void AddIfNotNull<TValue>(this IList<TValue> list, TValue value)
        {
            if ((object)value != null)
                list.Add(value);
        }

        public static void MoveUpList<T>(this List<T> list, T item)
        {
            int index = list.IndexOf(item);
            if (index == 0)
                return;
            else
            {
                list.RemoveAt(index);
                list.Insert(index - 1, item);
            }
        }

        public static void MoveDownList<T>(this List<T> list, T item)
        {
            int index = list.IndexOf(item);
            if (index >= (list.Count - 1))
                return;
            else
            {
                list.RemoveAt(index);
                list.Insert(index + 1, item);
            }
        }

        public static string RemoveHtmlTags(this string s)
        {
            return Regex.Replace(s, "<.*?>", String.Empty);
        }

    }
}

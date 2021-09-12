using System;
using System.Collections.Generic;
using System.Linq;
namespace NetDaemon.Service.App.CodeGeneration.Extensions
{
    internal static class CollectionExtensions
    {
        public static IEnumerable<IGrouping<TKey, TSource>> Duplicates<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        {
            var grouped = source.GroupBy(selector);
            return grouped.Where(i => i.Count() > 1);
        }

        // public static IEnumerable<TSource> Duplicates<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector)
        // {
        //     var grouped = source.GroupBy(selector);
        //     var moreThan1 = grouped.Where(i => i.Count() > 1);
        //     return moreThan1.SelectMany(i => i);
        // }

    }
}
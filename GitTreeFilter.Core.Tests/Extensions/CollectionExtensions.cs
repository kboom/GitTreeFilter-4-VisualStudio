using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GitTreeFilter.Core.Tests.Extensions
{
    public static class CollectionExtensions
    {
        private static readonly Random _rand = new();

        public static T RandomElement<T>(this IReadOnlyCollection<T> collection) => collection.ElementAt(_rand.Next() % collection.Count);
    }
}

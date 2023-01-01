using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GitTreeFilter.Core.Models
{
    public class GitChangeset : IReadOnlyCollection<GitItem>
    {
        private readonly Dictionary<string, GitItem> _items;

        public GitChangeset(HashSet<GitItem> changedPathsSet)
        {
            _items = changedPathsSet.ToDictionary(v => v.AbsoluteFilePath.ToLowerInvariant(), v => v);
        }
        public GitReferencePair<GitReference<GitCommitObject>> GitReferencePair { get; }

        public bool TryGetValue(string filePath, out GitItem item) => _items.TryGetValue(filePath.ToLowerInvariant(), out item);

        public GitItem this[string filePath] => TryGetValue(filePath, out var value) ? value : null;

        public int Count => _items.Count;
        IEnumerator IEnumerable.GetEnumerator() => _items.GetEnumerator();
        IEnumerator<GitItem> IEnumerable<GitItem>.GetEnumerator() => _items.Values.GetEnumerator();
    }
}

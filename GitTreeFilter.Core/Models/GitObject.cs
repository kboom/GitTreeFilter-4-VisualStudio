using LibGit2Sharp;
using System;
using System.Collections.Generic;

namespace GitTreeFilter.Core.Models
{
    public class GitObject
    {
        private readonly string _sha;

        public GitObject(string sha)
        {
            _sha = sha;
        }

        public string Sha => _sha;

        public sealed override bool Equals(object obj)
        {
            if (obj != null && typeof(GitObject).IsAssignableFrom(obj.GetType()))
            {
                var gitObject = obj as GitObject;
                return _sha == gitObject._sha;
            }

            return false;
        }
        public sealed override int GetHashCode() => 1138026772 + EqualityComparer<string>.Default.GetHashCode(_sha);
    }

    public sealed class GitCommitObject : GitObject
    {
        private readonly string _shortMessage;

        public GitCommitObject(string sha) : base(sha)
        {
            IsResolved = false;
        }

        public GitCommitObject(string sha, string shortMessage) : base(sha)
        {
            _shortMessage = shortMessage;
            IsResolved = true;
        }

        public bool IsResolved { get; private set; }

        public string ShortMessage => _shortMessage;
    }

    public abstract class GitReference<T> where T : GitObject
    {
        private readonly T _gitObject;

        public GitReference(T gitObject)
        {
            _gitObject = gitObject ?? throw new ArgumentNullException(nameof(gitObject));
        }

        public T Reference => _gitObject;

        public abstract string FriendlyName { get; }

        public abstract bool PinToMergeHead { get; }

        public override bool Equals(object obj) => obj is GitReference<T> reference && EqualityComparer<T>.Default.Equals(_gitObject, reference._gitObject);
        public override int GetHashCode() => -848059651 + EqualityComparer<T>.Default.GetHashCode(_gitObject);
    }

    internal static class GitReferenceExtensions
    {
        public static ObjectId ToObjectId<T>(this GitReference<T> reference) where T : GitObject
        {
            return new ObjectId(reference.Reference.Sha);
        }
    }

    public class GitCommit : GitReference<GitCommitObject>
    {
        public GitCommit(GitCommitObject target) : base(target) {
        }

        public override string FriendlyName => ShortSha;

        public string ShortMessage => Reference.ShortMessage;

        public string ShortSha => Reference.Sha.Substring(0, 7);

        public override bool PinToMergeHead => false;
    }

    public class GitBranch : GitReference<GitCommitObject>
    {
        private readonly string _shortName;
        private readonly bool _pinToMergeHead;

        public GitBranch(GitCommitObject target, string shortName, bool pinToMergeHead = false) : base(target)
        {
            if (target is null)
            {
                throw new ArgumentNullException(nameof(target));
            }

            if (string.IsNullOrEmpty(shortName))
            {
                throw new ArgumentException("Cannot be null or empty string", nameof(shortName));
            }

            _shortName = shortName;
            _pinToMergeHead = pinToMergeHead;
        }

        public override string FriendlyName => _shortName;

        public override bool PinToMergeHead => _pinToMergeHead;
    }

    public class GitTag : GitReference<GitCommitObject>
    {
        private readonly string _name;

        public GitTag(GitCommitObject target, string name) : base(target)
        {
            _name = name;
        }

        public override string FriendlyName => _name;

        public string ShortSha => Reference.Sha.Substring(0, 7);

        // convert to usable setting
        public override bool PinToMergeHead => false;
    }
}

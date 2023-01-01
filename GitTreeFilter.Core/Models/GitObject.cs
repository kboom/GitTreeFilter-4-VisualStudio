using System;
using System.Collections.Generic;
using System.Xml.Linq;
using LibGit2Sharp;

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
            _gitObject = gitObject;
        }

        public T Reference => _gitObject;

        public abstract string FriendlyName { get; }

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
    }

    public class GitBranch : GitReference<GitCommitObject>
    {
        private readonly string _shortName;

        public GitBranch(GitCommitObject target, string shortName) : base(target)
        {
            _shortName = shortName;
        }

        public override string FriendlyName => _shortName;
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

    }
}

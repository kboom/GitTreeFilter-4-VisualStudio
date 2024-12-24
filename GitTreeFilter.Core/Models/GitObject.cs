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

        public override string ToString() => $"{nameof(GitObject)}[{Sha}]";
    }

    public sealed class GitCommitObject : GitObject
    {
        public GitCommitObject(string sha) : base(sha)
        {
            _isResolved = false;
        }

        public GitCommitObject(string sha, string shortMessage) : base(sha)
        {
            _shortMessage = shortMessage;
            _isResolved = true;
        }

        public bool IsResolved => _isResolved;

        public string ShortMessage => _shortMessage;

        private readonly string _shortMessage;
        private readonly bool _isResolved;
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

        public abstract GitReference<GitCommitObject> Clone();

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            bool typesEqual = GetType().Equals(obj.GetType());
            bool shaEquality = obj is GitReference<T> otherGitReference && EqualityComparer<T>.Default.Equals(_gitObject, otherGitReference._gitObject);

            return typesEqual && shaEquality;
        }

        public override int GetHashCode() => _gitObject.GetHashCode();

        public override string ToString() => $"{GetType().Name}[{Reference}]";
    }

    public static class GitReferenceExtensions
    {
        public static ObjectId ToObjectId<T>(this GitReference<T> reference) where T : GitObject
        {
            return new ObjectId(reference.Reference.Sha);
        }

        public static GitCommit Tip(this GitBranch branch)
        {
            return new GitCommit(branch.Reference);
        }
    }

    public class GitCommit : GitReference<GitCommitObject>
    {
        public GitCommit(GitCommit source) : base(source.Reference)
        {
        }

        public GitCommit(GitCommitObject target) : base(target) {
        }

        public override string FriendlyName => ShortSha;

        public string ShortMessage => Reference.ShortMessage;

        public string ShortSha => Reference.Sha.Substring(0, 7);

        public override GitReference<GitCommitObject> Clone()
        {
            return new GitCommit(this);
        }
    }

    public class GitBranch : GitReference<GitCommitObject>
    {
        public GitBranch(GitBranch source) : this(source.Reference, source._shortName)
        {

        }

        public GitBranch(GitCommitObject target, string shortName) : base(target)
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
        }

        public override string FriendlyName => _shortName;

        public override GitReference<GitCommitObject> Clone() => new GitBranch(this);

        private readonly string _shortName;
    }

    public class GitTag : GitReference<GitCommitObject>
    {
        public GitTag(GitTag source) : this(source.Reference, source._name)
        {
        }

        public GitTag(GitCommitObject target, string name) : base(target)
        {
            _name = name;
        }

        public override string FriendlyName => _name;

        public string ShortSha => Reference.Sha.Substring(0, 7);

        public override GitReference<GitCommitObject> Clone() => new GitTag(this);

        private readonly string _name;
    }
}

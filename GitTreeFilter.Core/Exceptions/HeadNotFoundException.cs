namespace GitTreeFilter.Core.Exceptions
{
    public sealed class HeadNotFoundException : GitOperationException
    {
        public HeadNotFoundException() : base("Repository HEAD could not be found") { }
    }
}

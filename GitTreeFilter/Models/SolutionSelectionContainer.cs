namespace GitTreeFilter.Models;

class SolutionSelectionContainer<T> where T : ISolutionSelection
{
    public string FullName
    {
        get
        {
            return Item.FullPath;
        }
    }

    public string OldFullName
    {
        get
        {
            return Item.OldFullPath; 
        }
    }

    public T Item { get; set; }
}

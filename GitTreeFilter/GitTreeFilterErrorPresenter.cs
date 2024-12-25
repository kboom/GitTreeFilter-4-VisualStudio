using Microsoft.VisualStudio.Shell;
using System.Windows.Forms;

namespace GitTreeFilter;

public class GitTreeFilterErrorPresenter
{
    private const string c_caption = "Git Tree Filter";

    public void ShowError(string error)
    {
        ThreadHelper.ThrowIfNotOnUIThread();
        _ = MessageBox.Show(error, c_caption);
    }
}

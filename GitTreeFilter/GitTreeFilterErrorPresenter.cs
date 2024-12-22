using System.Windows.Forms;

namespace GitTreeFilter;

public class GitTreeFilterErrorPresenter
{
    public void ShowError(string error) => _ = MessageBox.Show(
            error,
            "Git Tree Filter Error");

    public void ShowInfo(string msg) => _ = MessageBox.Show(
            msg,
            "Nothing to compare");
}

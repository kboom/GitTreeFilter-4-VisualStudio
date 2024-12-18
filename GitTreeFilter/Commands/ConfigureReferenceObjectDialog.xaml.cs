using System.Collections.ObjectModel;
using System.Windows;
using GitTreeFilter.Core.Models;
using LibGit2Sharp;
using Microsoft.VisualStudio.PlatformUI;

namespace GitTreeFilter.Commands
{
    public partial class ConfigureReferenceObjectDialog : DialogWindow
    {

        public ObservableCollection<GitBranch> BranchListData { get; } = new ObservableCollection<GitBranch>();

        public ObservableCollection<GitCommit> CommitListData { get; } = new ObservableCollection<GitCommit>();

        public ObservableCollection<GitTag> TagListData { get; } = new ObservableCollection<GitTag>();

        public GitReference<GitCommitObject> SelectedReference { get; set; }

        public ConfigureReferenceObjectDialog()
        {
            InitializeComponent();

            DataContext = this;
            BranchList.ItemsSource = BranchListData;
            CommitList.ItemsSource = CommitListData;
            TagList.ItemsSource = TagListData;
        }

        public void SetDefaultReference(GitReference<GitCommitObject> reference)
        {
            if(reference == null)
            {
                return;
            }

            SelectedReference = reference;

            // this is not showing up selection correctly
            switch (reference)
            {
                case GitBranch branch:
                    Tabs.SelectedItem = BranchesTab;
                    BranchList.SelectedItem = branch;
                    return;
                case GitCommit commit:
                    Tabs.SelectedItem = CommitsTab;
                    CommitList.SelectedItem = commit;
                    return;
                case GitTag tag:
                    Tabs.SelectedItem = TagsTab;
                    TagList.SelectedItem = tag;
                    return;
                default:
                    return;
            }
        }

        private void OKBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void CancelBtn_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        // TODO: add checkbox in case of branch to pin to merge head!
        private void BranchList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (BranchList.SelectedItem != null)
            {
                SelectedReference = BranchList.SelectedItem as GitReference<GitCommitObject>;
            }
        }

        private void CommitList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CommitList.SelectedItem != null)
            {
                SelectedReference = CommitList.SelectedItem as GitReference<GitCommitObject>;
            }
        }

        private void TagList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (TagList.SelectedItem != null)
            {
                SelectedReference = TagList.SelectedItem as GitReference<GitCommitObject>;
            }
        }
    }
}

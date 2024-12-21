using GitTreeFilter.Core.Models;
using Microsoft.VisualStudio.PlatformUI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace GitTreeFilter.Commands
{
    public partial class ConfigureReferenceObjectDialog : DialogWindow, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<GitBranch> BranchListData { get; } = new ObservableCollection<GitBranch>();

        public ObservableCollection<GitCommit> CommitListData { get; } = new ObservableCollection<GitCommit>();

        public ObservableCollection<GitTag> TagListData { get; } = new ObservableCollection<GitTag>();

        public GitReference<GitCommitObject> SelectedReference
        {
            get => _selectedReference;
            set
            {
                if (_selectedReference != value)
                {
                    _selectedReference = value;
                    UpdateControls();
                    OnPropertyChanged(nameof(SelectedReference));
                }
            }
        }

        public bool PinToMergeHead { 
            get => _pinToMergeHead;
            set
            {
                _pinToMergeHead = value;
                SelectedReference = SelectedReference switch
                {
                    GitBranch branch => new GitBranch(branch, value),
                    GitTag tag => new GitTag(tag, value),
                    _ => SelectedReference
                };
                OnPropertyChanged(nameof(PinToMergeHead));
            }
        }

        public ConfigureReferenceObjectDialog()
        {
            InitializeComponent();

            DataContext = this;
            BranchList.ItemsSource = BranchListData;
            CommitList.ItemsSource = CommitListData;
            TagList.ItemsSource = TagListData;
        }

        private void UpdateControls()
        {
            // this is not showing up selection correctly
            switch (SelectedReference)
            {
                case GitBranch branch:
                    OKBtn.IsEnabled = true;
                    MergeHeadCheckBox.Visibility = Visibility.Visible;
                    Tabs.SelectedItem = BranchesTab;
                    break;
                case GitCommit commit:
                    OKBtn.IsEnabled = true;
                    MergeHeadCheckBox.Visibility = Visibility.Hidden;
                    Tabs.SelectedItem = CommitsTab;
                    break;
                case GitTag tag:
                    OKBtn.IsEnabled = true;
                    MergeHeadCheckBox.Visibility = Visibility.Visible;
                    Tabs.SelectedItem = TagsTab;
                    break;
                default:
                    OKBtn.IsEnabled = false;
                    break;
            }
        }

        public void SetDefaultReference(GitReference<GitCommitObject> reference)
        {
            if(reference == null)
            {
                return;
            }

            SelectedReference = reference;
            PinToMergeHead = SelectedReference.PinToMergeHead;
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

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private GitReference<GitCommitObject> _selectedReference;
        private bool _pinToMergeHead;
    }
}

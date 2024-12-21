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

        public bool PinToMergeHead
        {
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

        public Visibility MergeHeadCheckBoxVisibility =>
            SelectedReference is GitBranch || SelectedReference is GitTag
                ? Visibility.Visible
                : Visibility.Hidden;

        public bool IsOkEnabled
        {
            get => _isEnabled;
            set {
                _isEnabled = value;
                OnPropertyChanged(nameof(IsOkEnabled));
            }
        }

        public ConfigureReferenceObjectDialog()
        {
            InitializeComponent();
            DataContext = this;

            PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == nameof(SelectedReference))
                {
                    IsOkEnabled = SelectedReference != null;
                }
            };
        }

        private void UpdateControls()
        {
            // this is not showing up selection correctly
            switch (SelectedReference)
            {
                case GitBranch branch:
                    Tabs.SelectedItem = BranchesTab;
                    break;
                case GitCommit commit:
                    Tabs.SelectedItem = CommitsTab;
                    break;
                case GitTag tag:
                    Tabs.SelectedItem = TagsTab;
                    break;
                default:
                    SelectedReference = null;
                    break;
            }
        }

        public void SetDefaultReference(GitReference<GitCommitObject> reference)
        {
            if (reference == null)
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
        private bool _isEnabled;
    }
}

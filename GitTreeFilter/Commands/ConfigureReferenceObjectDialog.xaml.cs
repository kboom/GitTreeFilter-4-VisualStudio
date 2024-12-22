using GitTreeFilter.Core.Models;
using Microsoft;
using Microsoft.VisualStudio.PlatformUI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace GitTreeFilter.Commands;

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
                _selectedReference = value.Clone();
                UpdateTabSelection();
                OnPropertyChanged(nameof(SelectedReference));
            }
        }
    }

    public SessionSettings SessionSettings
    {
        get => _sessionSettings;
        set
        {
            Assumes.NotNull(value);
            _sessionSettings = value;
            OnPropertyChanged(nameof(SessionSettings));
            OnPropertyChanged(nameof(PinToMergeHead));
        }
    }

    public bool PinToMergeHead
    {
        get => _sessionSettings.PinToMergeHead;
        set
        {
            _sessionSettings = _sessionSettings.WithPinToMergeHead(value);
            OnPropertyChanged(nameof(PinToMergeHead));
        }
    }

    public Visibility MergeHeadCheckBoxVisibility
    {
        get => _mergeHeadCheckBoxVisibility;
        set
        {
            _mergeHeadCheckBoxVisibility = value;
            OnPropertyChanged(nameof(MergeHeadCheckBoxVisibility));
        }
    }

    public bool IsOkEnabled
    {
        get => _isEnabled;
        set
        {
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
                UpdateTabSelection();
            }
        };

        Tabs.SelectionChanged += (sender, args) => MergeHeadCheckBoxVisibility = Tabs.SelectedItem switch
        {
            var item when item == BranchesTab || item == TagsTab => Visibility.Visible,
            _ => Visibility.Hidden
        };
    }

    private void UpdateTabSelection()
    {
        switch (SelectedReference)
        {
            case GitBranch _:
                Tabs.SelectedItem = BranchesTab;
                break;
            case GitCommit _:
                Tabs.SelectedItem = CommitsTab;
                break;
            case GitTag _:
                Tabs.SelectedItem = TagsTab;
                break;
            default:
                break;
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

    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private GitReference<GitCommitObject> _selectedReference;
    private bool _pinToMergeHead;
    private bool _isEnabled;
    private Visibility _mergeHeadCheckBoxVisibility;
    private SessionSettings _sessionSettings = SessionSettings.Default;
}

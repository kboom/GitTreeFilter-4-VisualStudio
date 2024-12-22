using GitTreeFilter.Core;
using GitTreeFilter.Core.Models;
using GitTreeFilter.Models;
using Microsoft.VisualStudio.Shell.Interop;

namespace GitTreeFilter
{
    internal class VsFileDiffProvider
    {
        private readonly string _documentPath;
        private readonly string _oldDocumentPath;
        private readonly MessagePresenter _messagePresenter;
        private readonly IVsDifferenceService _vsDifferenceService;
        private readonly ISolutionRepository _solutionRepository;

        public VsFileDiffProvider(
            IVsDifferenceService vsDifferenceService,
            SolutionSelectionContainer<ISolutionSelection> selectionContainer,
            MessagePresenter messagePresenter,
            ISolutionRepository solutionRepository)
        {
            _vsDifferenceService = vsDifferenceService;
            _messagePresenter = messagePresenter;
            _solutionRepository = solutionRepository;
            _documentPath = selectionContainer.FullName;
            _oldDocumentPath = selectionContainer.OldFullName;
        }

        public void ShowFileDiff()
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if(TryCreateChangeset(out var leftItem))
            {
                var leftFileMoniker = leftItem.BaseBranchRevisionOfFile;
                var rightFileMoniker = _documentPath;

                PresentComparisonWindow(leftFileMoniker, rightFileMoniker);
            } else
            {
                _messagePresenter.ShowInfo("This file has no changes but was displayed to retain the hierarchy.");
            }
        }

        private bool TryCreateChangeset(out GitItem item)
        {
            item = default;
            try
            {
                var originalFilePath = string.IsNullOrEmpty(_oldDocumentPath) ? _documentPath : _oldDocumentPath;
                return _solutionRepository.TryReadItem(originalFilePath, out item);
            }
            catch (GitOperationException e)
            {
                _messagePresenter.ShowError(e.Message);
                return false;
            }
        }

        private void PresentComparisonWindow(string leftFileMoniker, string rightFileMoniker)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            var filename = System.IO.Path.GetFileName(_documentPath);

            var reference = _solutionRepository.ComparisonConfig.ReferenceObject;
            var leftLabel = $"{filename}@{reference.FriendlyName}";
            var rightLabel = $"{filename}@Working Tree";
            var caption = $"{System.IO.Path.GetFileName(rightFileMoniker)} <{reference.FriendlyName}>";

            var diffServiceOptions = __VSDIFFSERVICEOPTIONS.VSDIFFOPT_LeftFileIsTemporary;
            _ = _vsDifferenceService.OpenComparisonWindow2(
                leftFileMoniker,
                rightFileMoniker,
                caption,
                string.Empty,
                leftLabel,
                rightLabel,
                string.Empty,
                string.Empty,
                (uint)diffServiceOptions);

            System.IO.File.Delete(leftFileMoniker);
        }
    }
}

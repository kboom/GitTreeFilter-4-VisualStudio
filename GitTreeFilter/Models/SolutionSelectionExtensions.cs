using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Platform.WindowManagement;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Differencing;
using System;
using System.Collections.Generic;

namespace GitTreeFilter.Models
{
    internal static class SolutionSelectionExtensions
    {
        public static bool HasNoAssociatedDiffWindow(this SolutionSelectionContainer<ISolutionSelection> selectedItem, IVsUIShell vsUIShell)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (selectedItem.Item is SelectedProjectItem)
            {
                if (selectedItem.Item.Document == null)
                {
                    return true;
                }

                if (selectedItem.Item.Document.Windows.Count == 0)
                {
                    return true;
                }
            }

            foreach (var vsWindowFrame in GetAllWindowFramesFromShell(vsUIShell))
            {
                if (vsWindowFrame is WindowFrame frameImpl
                    && frameImpl.IsDocument
                    && frameImpl.DocumentMoniker.Equals(selectedItem.FullName, StringComparison.OrdinalIgnoreCase)
                    && IsDiffWindowFrame(frameImpl))
                {
                    return false;
                }
            }

            return true;
        }

        public static void FocusAssociatedDiffWindow(this SolutionSelectionContainer<ISolutionSelection> selectedItem, IVsUIShell vsUIShell)
        {
            ThreadHelper.ThrowIfNotOnUIThread(); 
            foreach (var vsWindowFrame in GetAllWindowFramesFromShell(vsUIShell))
            {
                if (vsWindowFrame is WindowFrame frameImpl
                    && frameImpl.IsDocument
                    && frameImpl.DocumentMoniker.Equals(selectedItem.FullName, StringComparison.OrdinalIgnoreCase)
                    && IsDiffWindowFrame(frameImpl))
                {
                    frameImpl.Show();
                    return;
                }
            }
        }

        private static List<IVsWindowFrame> GetAllWindowFramesFromShell(IVsUIShell vsUIShell)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var allWindowFrames = new List<IVsWindowFrame>();
            ErrorHandler.ThrowOnFailure(vsUIShell.GetDocumentWindowEnum(out var windowEnumerator));
            if (windowEnumerator.Reset() != VSConstants.S_OK)
            {
                return allWindowFrames;
            }

            var tempFrameList = new IVsWindowFrame[1];
            bool hasMorewindows;
            do
            {
                hasMorewindows = windowEnumerator.Next(1, tempFrameList, out var fetched) == VSConstants.S_OK && fetched == 1;

                if (!hasMorewindows || tempFrameList[0] == null)
                {
                    continue;
                }

                allWindowFrames.Add(tempFrameList[0]);

            } while (hasMorewindows);

            return allWindowFrames;
        }

        private static bool IsDiffWindowFrame(IVsWindowFrame frame)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            ErrorHandler.Succeeded(frame.GetProperty((int)__VSFPROPID.VSFPROPID_DocView, out var docView));
            if (docView is IVsDifferenceCodeWindow vsDifferenceCodeWindow)
            {
                if (vsDifferenceCodeWindow.DifferenceViewer is IDifferenceViewer2 diffViewer)
                {
                    return diffViewer.LeftViewExists && diffViewer.RightViewExists;
                }
            }

            return false;
        }
    }
}

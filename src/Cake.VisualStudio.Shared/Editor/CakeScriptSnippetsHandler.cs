using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;

using MSXML;

using System;

namespace Cake.VisualStudio.Shared.Editor
{
    internal class CakeScriptSnippetsHandler : IOleCommandTarget, IVsExpansionClient
    {
        private readonly ITextView textView;
        private readonly IVsTextView vsTextView;
        private readonly CakeScriptSnippetsProvider provider;
        private readonly IOleCommandTarget nextCommandHandler;
        private readonly IVsExpansionManager expansionManager;
        private IVsExpansionSession session;

        internal CakeScriptSnippetsHandler(IVsTextView textViewAdapter, ITextView textView, CakeScriptSnippetsProvider provider)
        {
            this.textView = textView;
            this.provider = provider;
            vsTextView = textViewAdapter;

            //get the text manager from the service provider
            var textManager = (IVsTextManager2)provider.ServiceProvider.GetService(typeof(SVsTextManager)) ?? throw new ArgumentOutOfRangeException(nameof(provider), "Could not create a SVsTextManager!");
            textManager.GetExpansionManager(out expansionManager);
            session = null;


            //add the command to the command chain
            textViewAdapter.AddCommandFilter(this, out nextCommandHandler);
        }

        public int QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            if (!VsShellUtilities.IsInAutomationFunction(provider.ServiceProvider))
            {
                if (pguidCmdGroup == VSConstants.VSStd2K && cCmds > 0)
                {
                    // make the Insert Snippet command appear on the context menu
                    if (prgCmds[0].cmdID == (uint)VSConstants.VSStd2KCmdID.INSERTSNIPPET)
                    {
                        prgCmds[0].cmdf = (int)Constants.MSOCMDF_ENABLED | (int)Constants.MSOCMDF_SUPPORTED;
                        return VSConstants.S_OK;
                    }
                }
            }

            return nextCommandHandler.QueryStatus(ref pguidCmdGroup, cCmds, prgCmds, pCmdText);
        }

        public int Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            /*
            if (VsShellUtilities.IsInAutomationFunction(provider.ServiceProvider))
            {
                return nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
            }
            */

            // Show the snippet picker
            if (nCmdID == (uint)VSConstants.VSStd2KCmdID.INSERTSNIPPET)
            {
                expansionManager.InvokeInsertionUI(
                    vsTextView,
                    this,      //the expansion client
                    Helpers.Constants.LanguageGuid,
                    null,       //use all snippet types
                    0,          //number of types (0 for all)
                    0,          //ignored if iCountTypes == 0
                    null,       //use all snippet kinds
                    0,          //use all snippet kinds
                    0,          //ignored if iCountTypes == 0
                    "Cake Snippets", //the text to show in the prompt
                    string.Empty);  //only the ENTER key causes insert 

                return VSConstants.S_OK;
            }

            //the expansion insertion is handled in OnItemChosen
            //if the expansion session is still active, handle tab/back-tab/return/cancel
            if (session != null)
            {
                if (nCmdID == (uint)VSConstants.VSStd2KCmdID.BACKTAB)
                {
                    session.GoToPreviousExpansionField();
                    return VSConstants.S_OK;
                }
                else if (nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB)
                {

                    session.GoToNextExpansionField(0); //false to support cycling through all the fields
                    return VSConstants.S_OK;
                }
                else if (nCmdID == (uint)VSConstants.VSStd2KCmdID.RETURN || nCmdID == (uint)VSConstants.VSStd2KCmdID.CANCEL)
                {
                    if (session.EndCurrentExpansion(0) == VSConstants.S_OK)
                    {
                        session = null;
                        return VSConstants.S_OK;
                    }
                }
            }

            //neither an expansion session nor a completion session is open, but we got a tab, so check whether the last word typed is a snippet shortcut 
            if (session == null && nCmdID == (uint)VSConstants.VSStd2KCmdID.TAB)
            {
                //get the word that was just added 
                CaretPosition pos = textView.Caret.Position;
                TextExtent word = provider.NavigatorService.GetTextStructureNavigator(textView.TextBuffer).GetExtentOfWord(pos.BufferPosition - 1); //use the position 1 space back
                string textString = word.Span.GetText(); //the word that was just added
                                                         //if it is a code snippet, insert it, otherwise carry on
                if (InsertAnyExpansion(textString, null, null))
                {
                    EndSession();
                    return VSConstants.S_OK;
                }
            }

            return nextCommandHandler.Exec(ref pguidCmdGroup, nCmdID, nCmdexecopt, pvaIn, pvaOut);
        }

        public int EndExpansion()
        {
            session = null;
            return VSConstants.S_OK;
        }

        public int FormatSpan(IVsTextLines pBuffer, TextSpan[] ts)
        {
            return VSConstants.S_OK;
        }

        public int GetExpansionFunction(IXMLDOMNode xmlFunctionNode, string bstrFieldName, out IVsExpansionFunction pFunc)
        {
            pFunc = null;
            return VSConstants.S_OK;
        }

        public int IsValidKind(IVsTextLines pBuffer, TextSpan[] ts, string bstrKind, out int pfIsValidKind)
        {
            pfIsValidKind = 1;
            return VSConstants.S_OK;
        }

        public int IsValidType(IVsTextLines pBuffer, TextSpan[] ts, string[] rgTypes, int iCountTypes, out int pfIsValidType)
        {
            pfIsValidType = 1;
            return VSConstants.S_OK;
        }

        public int OnAfterInsertion(IVsExpansionSession pSession)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeInsertion(IVsExpansionSession pSession)
        {
            return VSConstants.S_OK;
        }

        public int PositionCaretForEditing(IVsTextLines pBuffer, TextSpan[] ts)
        {
            return VSConstants.S_OK;
        }

        public int OnItemChosen(string pszTitle, string pszPath)
        {
            if (InsertAnyExpansion(null, pszTitle, pszPath))
            {
                EndSession();
            }

            return VSConstants.S_OK;
        }

        private void EndSession()
        {
            if (session != null)
            {
                session.EndCurrentExpansion(0);
                session = null;
            }
        }

        private bool InsertAnyExpansion(string shortcut, string title, string path)
        {
            //first get the location of the caret, and set up a TextSpan
            //get the column number from  the IVsTextView, not the ITextView
            vsTextView.GetCaretPos(out int startLine, out int endColumn);

            var addSpan = new TextSpan
            {
                iStartIndex = endColumn,
                iEndIndex = endColumn,
                iStartLine = startLine,
                iEndLine = startLine
            };

            if (shortcut != null) //get the expansion from the shortcut
            {
                //reset the TextSpan to the width of the shortcut, 
                //because we're going to replace the shortcut with the expansion
                addSpan.iStartIndex = addSpan.iEndIndex - shortcut.Length;

                expansionManager.GetExpansionByShortcut(
                    this,
                    Helpers.Constants.LanguageGuid,
                    shortcut,
                    vsTextView,
                    new TextSpan[] { addSpan },
                    0,
                    out path,
                    out title);

            }
            if (title != null && path != null)
            {
                vsTextView.GetBuffer(out IVsTextLines textLines);
                var bufferExpansion = (IVsExpansion)textLines;

                if (bufferExpansion != null)
                {
                    int hr = bufferExpansion.InsertNamedExpansion(
                        title,
                        path,
                        addSpan,
                        this,
                        Helpers.Constants.LanguageGuid,
                        0,
                       out session);
                    if (VSConstants.S_OK == hr)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
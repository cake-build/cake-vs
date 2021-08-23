using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;

using System;
using System.ComponentModel.Composition;

namespace Cake.VisualStudio.Shared.Editor
{
    [Export(typeof(IVsTextViewCreationListener))]
    [Name("Cake snippets handler")]
    [ContentType(Helpers.Constants.CakeContentType)]
    [TextViewRole(PredefinedTextViewRoles.Editable)]
    internal class CakeScriptSnippetsProvider : IVsTextViewCreationListener
    {
        [Import]
        internal IVsEditorAdaptersFactoryService AdapterService = null;

        [Import]
        internal ICompletionBroker CompletionBroker { get; set; }

        [Import]
        internal SVsServiceProvider ServiceProvider { get; set; }

        [Import]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            ITextView textView = AdapterService.GetWpfTextView(textViewAdapter);
            if (textView == null)
            {
                return;
            }

            Func<CakeScriptSnippetsHandler> generator = () => new CakeScriptSnippetsHandler(textViewAdapter, textView, this);
            textView.Properties.GetOrCreateSingletonProperty(generator);
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cake.VisualStudio.Classifier.Languages;
using Cake.VisualStudio.Helpers;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.DragDrop;
using Microsoft.VisualStudio.Utilities;

namespace Cake.VisualStudio.Editor
{
    [Export(typeof(IDropHandlerProvider))]
    [DropFormat("CF_VSSTGPROJECTITEMS")]
    [DropFormat("CF_VSREFPROJECTITEMS")]
    [DropFormat("FileDrop")]
    [Name("CakeDropHandler")]
    [ContentType(Constants.CakeContentType)]
    [Order(Before = "DefaultFileDropHandler")]
    class CakeScriptDropHandlerProvider : IDropHandlerProvider
    {
        [Import]
        ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        public IDropHandler GetAssociatedDropHandler(IWpfTextView wpfTextView)
        {
            ITextDocument document;

            if (TextDocumentFactoryService.TryGetTextDocument(wpfTextView.TextBuffer, out document))
            {
                return
                    wpfTextView.Properties.GetOrCreateSingletonProperty(
                        () => new CakeScriptDropHandler(wpfTextView, document));
            }

            return null;
        }
    }
}

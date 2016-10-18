using System;
using System.ComponentModel.Composition;
using Cake.VisualStudio.Helpers;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Cake.VisualStudio.Editor
{
    [Export(typeof(ISmartIndentProvider))]
    [ContentType(Constants.CakeContentType)]
    class SmartIndentProvider : ISmartIndentProvider
    {
        [Import] private IEditorOptionsFactoryService Factory { get; set; }

        public ISmartIndent CreateSmartIndent(ITextView textView)
        {
            if (textView == null)
            {
                throw new ArgumentNullException(nameof(textView));
            }

            return Factory == null
                ? new SmartIndent(textView)
                : new SmartIndent(textView, Factory.GetOptions(textView));
        }
    }
}
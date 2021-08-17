using Cake.VisualStudio.Helpers;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;

namespace Cake.VisualStudio.Editor
{
    class SmartIndent : ISmartIndent
    {
        private ITextView _textView;
        private readonly int _tabSize;
        private readonly IEditorOptions _options;

        public SmartIndent(ITextView textView, IEditorOptions options) : this(textView)
        {
            _options = options;
            _tabSize = _options.GetTabSize();
        }

        public SmartIndent(ITextView textView)
        {
            _textView = textView;
            _tabSize = 4;
        }

        public void Dispose()
        {
        }

        public int? GetDesiredIndentation(ITextSnapshotLine line)
        {
            var offset = 0;
            var prevLine = line.GetPreviousMatchingLine(l => !string.IsNullOrWhiteSpace(l.GetText()));
            if (prevLine.RequiresOffset("{")) offset += _tabSize;
            if (prevLine.RequiresOffset("(")) offset += _tabSize / 2;
            var prevOffset = GetPreviousOffset(prevLine);
            return CalculateOffset(prevOffset, offset);
        }

        private int CalculateOffset(int prevOffset, int offset)
        {
            var i = prevOffset + offset;
            return offset == _tabSize ? i%_tabSize == 0 ? i : i - _tabSize/2 : i;
        }

        private int GetPreviousOffset(ITextSnapshotLine prevLine)
        {
            return _options == null ? prevLine.GetColumnOfFirstNonWhitespaceCharacterOrEndOfLine(_tabSize) :
                prevLine.GetColumnOfFirstNonWhitespaceCharacterOrEndOfLine(_options);
            //return isEmpty ? 0 : prevLine.Length - 1;
        }
    }
}

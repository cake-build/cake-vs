﻿using System;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.OptionsExtensionMethods;

namespace Cake.VisualStudio.Editor
{
    /// <summary>
    /// Extensions for handling text lines (especially when working with indents)
    /// </summary>
    /// <remarks>
    /// This class includes a lot of null-checking in every extension.
    /// This is necessary as there is a hard-to-repro bug with crashes on opening an empty Cake files.
    /// Please do not change these checks without extensive testing.
    /// </remarks>
    internal static class ITextSnapshotLineExtensions
    {
        /// <summary>
        /// Returns the first non-whitespace position on the given line, or null if 
        /// the line is empty or contains only whitespace.
        /// </summary>
        public static int? GetFirstNonWhitespacePosition(this ITextSnapshotLine line)
        {
            if (line == null) return null;
            var text = line.GetText();

            for (int i = 0; i < text.Length; i++)
            {
                if (!char.IsWhiteSpace(text[i]))
                {
                    return line.Start + i;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the first non-whitespace position on the given line as an offset
        /// from the start of the line, or null if the line is empty or contains only
        /// whitespace.
        /// </summary>
        public static int? GetFirstNonWhitespaceOffset(this ITextSnapshotLine line)
        {
            if (line == null) return null;
            var text = line.GetText();

            for (int i = 0; i < text.Length; i++)
            {
                if (!char.IsWhiteSpace(text[i]))
                {
                    return i;
                }
            }

            return null;
        }

        /// <summary>
        /// Determines whether the specified line is empty or contains whitespace only.
        /// </summary>
        public static bool IsEmptyOrWhitespace(this ITextSnapshotLine line)
        {
            if (line == null) return true;
            var text = line.GetText();

            for (int i = 0; i < text.Length; i++)
            {
                if (!char.IsWhiteSpace(text[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public static ITextSnapshotLine GetPreviousMatchingLine(this ITextSnapshotLine line, Func<ITextSnapshotLine, bool> predicate)
        {
            if (line == null || line.LineNumber <= 0)
            {
                return null;
            }

            var snapshot = line.Snapshot;
            for (int lineNumber = line.LineNumber - 1; lineNumber >= 0; lineNumber--)
            {
                var currentLine = snapshot.GetLineFromLineNumber(lineNumber);
                if (!predicate(currentLine))
                {
                    continue;
                }

                return currentLine;
            }

            return null;
        }

        public static int GetColumnOfFirstNonWhitespaceCharacterOrEndOfLine(this ITextSnapshotLine line, IEditorOptions editorOptions)
        {
            return line == null ? 0 : line.GetColumnOfFirstNonWhitespaceCharacterOrEndOfLine(editorOptions.GetTabSize());
        }

        public static int GetColumnOfFirstNonWhitespaceCharacterOrEndOfLine(this ITextSnapshotLine line, int tabSize)
        {
            return line == null ? 0 : line.GetText().GetColumnOfFirstNonWhitespaceCharacterOrEndOfLine(tabSize);
        }

        public static int GetColumnFromLineOffset(this ITextSnapshotLine line, int lineOffset, IEditorOptions editorOptions)
        {
            return line == null ? 0 : line.GetText().GetColumnFromLineOffset(lineOffset, editorOptions.GetTabSize());
        }

        public static int GetColumnOfFirstNonWhitespaceCharacterOrEndOfLine(this string line, int tabSize)
        {
            if (line == null) return 0;
            var firstNonWhitespaceChar = line.GetFirstNonWhitespaceOffset();

            if (firstNonWhitespaceChar.HasValue)
            {
                return line.GetColumnFromLineOffset(firstNonWhitespaceChar.Value, tabSize);
            }
            else
            {
                // It's all whitespace, so go to the end
                return line.GetColumnFromLineOffset(line.Length, tabSize);
            }
        }

        public static int? GetFirstNonWhitespaceOffset(this string line)
        {
            if (line == null) return null;
            for (int i = 0; i < line.Length; i++)
            {
                if (!char.IsWhiteSpace(line[i]))
                {
                    return i;
                }
            }

            return null;
        }

        public static string GetLeadingWhitespace(this string lineText)
        {
            if (lineText == null) return null;
            var firstOffset = lineText.GetFirstNonWhitespaceOffset();

            return firstOffset.HasValue
                ? lineText.Substring(0, firstOffset.Value)
                : lineText;
        }

        public static int GetColumnFromLineOffset(this string line, int endPosition, int tabSize)
        {
            if (line == null) return 0;
            return ConvertTabToSpace(line, tabSize, 0, endPosition);
        }

        public static int ConvertTabToSpace(this string textSnippet, int tabSize, int initialColumn, int endPosition)
        {
            int column = initialColumn;

            // now this will calculate indentation regardless of actual content on the buffer except TAB
            for (int i = 0; i < endPosition; i++)
            {
                if (textSnippet[i] == '\t')
                {
                    column += tabSize - column % tabSize;
                }
                else
                {
                    column++;
                }
            }

            return column - initialColumn;
        }

        /// <summary>
        /// Checks if the given line at the given snapshot index starts with the provided value.
        /// </summary>
        public static bool StartsWith(this ITextSnapshotLine line, int index, string value, bool ignoreCase)
        {
            if (line == null) return false;
            var snapshot = line.Snapshot;
            if (index + value.Length > snapshot.Length)
            {
                return false;
            }

            for (int i = 0; i < value.Length; i++)
            {
                var snapshotIndex = index + i;
                var actualCharacter = snapshot[snapshotIndex];
                var expectedCharacter = value[i];

                if (ignoreCase)
                {
                    actualCharacter = char.ToLowerInvariant(actualCharacter);
                    expectedCharacter = char.ToLowerInvariant(expectedCharacter);
                }

                if (actualCharacter != expectedCharacter)
                {
                    return false;
                }
            }

            return true;
        }
    }
}

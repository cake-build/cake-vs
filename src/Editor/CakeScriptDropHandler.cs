using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Editor.DragDrop;

namespace Cake.VisualStudio.Editor
{
    internal class CakeScriptDropHandler : CakeDropHandler
    {
        private readonly FileInfo _scriptFile;
        private string _targetFileName;
        private readonly IWpfTextView _textView;
        private ITextDocument _document;
        private readonly string[] _supportedFileExtensions = new[] {".cake", ".dll"};

        public CakeScriptDropHandler(IWpfTextView textView, ITextDocument currentDocument)
        {
            _textView = textView;
            _scriptFile = new FileInfo(currentDocument.FilePath);
            _document = currentDocument;
        }

        public override DragDropPointerEffects HandleDataDropped(DragDropInfo dragDropInfo)
        {
            try
            {
                var relativePath = PackageUtilities.MakeRelative(_scriptFile.FullName, _targetFileName)
                    .Replace("\\", "/");
                string insertString = null;
                switch (Path.GetExtension(relativePath))
                {
                    case ".cake":
                        insertString = $"#load \"{relativePath}\"";
                        break;
                    case ".dll":
                        insertString = $"#r \"{relativePath}\"";
                        break;
                }
                
                using (var edit = _textView.TextBuffer.CreateEdit())
                {
                    edit.Insert(GetInsertPosition(), insertString + Environment.NewLine);
                    edit.Apply();
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return DragDropPointerEffects.Copy;
        }

        private int GetInsertPosition()
        {
            // need to improve logic here to find the first non-#load'ing line in the document.
            return 0;
        }

        public override bool IsDropEnabled(DragDropInfo dragDropInfo)
        {
            _targetFileName = GetScriptFileName(dragDropInfo);
            if (_targetFileName == null) return false;
            var ext = Path.GetExtension(_targetFileName);
            return _supportedFileExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase) &&
                   (File.Exists(_targetFileName) || Directory.Exists(_targetFileName));
        }

        private static string GetScriptFileName(DragDropInfo dragDropInfo)
        {
            var data = new DataObject(dragDropInfo.Data);

            if (dragDropInfo.Data.GetDataPresent("FileDrop"))
            {
                var files = data.GetFileDropList();

                if (files.Count == 1)
                {
                    return files[0];
                }
            }
            else if (dragDropInfo.Data.GetDataPresent("CF_VSSTGPROJECTITEMS") || dragDropInfo.Data.GetDataPresent("CF_VSREFPROJECTITEMS"))
            {
                return data.GetText();
            }
            else if (dragDropInfo.Data.GetDataPresent("MultiURL"))
            {
                return data.GetText();
            }
            return null;
        }
    }
}

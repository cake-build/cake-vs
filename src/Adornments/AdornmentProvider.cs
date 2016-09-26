// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel.Composition;
using System.IO;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Cake.VisualStudio.Adornments
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    class AdornmentProvider : IWpfTextViewCreationListener
    {
        private const string PropertyName = "ShowWatermark";
        private const double InitOpacity = 0.4D;

        private static bool _isVisible;
        private static bool _hasLoaded;
        private SettingsManager _settingsManager;

        [Import]
        public ITextDocumentFactoryService TextDocumentFactoryService { get; set; }

        [Import]
        public SVsServiceProvider ServiceProvider { get; set; }

        public void TextViewCreated(IWpfTextView textView)
        {
            ITextDocument document;

            if (!TextDocumentFactoryService.TryGetTextDocument(textView.TextDataModel.DocumentBuffer, out document))
            {
                return;
            }

            LoadSettings();

            var fileName = Path.GetFileName(document.FilePath).ToLowerInvariant();

            // Check if filename is absolute because when debugging, script files are sometimes dynamically created.
            if (string.IsNullOrEmpty(fileName) || !Path.IsPathRooted(document.FilePath))
            {
                return;
            }

            if (fileName.EndsWith(".cake"))
            {
                textView.Properties.GetOrCreateSingletonProperty(() => new LogoAdornment(textView, _isVisible, InitOpacity));
            }
        }

        private void LoadSettings()
        {
            if (_hasLoaded)
            {
                return;
            }

            _hasLoaded = true;

            _settingsManager = new ShellSettingsManager(ServiceProvider);
            var store = _settingsManager.GetReadOnlySettingsStore(SettingsScope.UserSettings);

            LogoAdornment.VisibilityChanged += AdornmentVisibilityChanged;

            _isVisible = store.GetBoolean(Vsix.Name, PropertyName, true);
        }

        private void AdornmentVisibilityChanged(object sender, bool isVisible)
        {
            var wstore = _settingsManager.GetWritableSettingsStore(SettingsScope.UserSettings);
            _isVisible = isVisible;

            if (!wstore.CollectionExists(Vsix.Name))
            {
                wstore.CreateCollection(Vsix.Name);
            }

            wstore.SetBoolean(Vsix.Name, PropertyName, isVisible);
        }
    }
}
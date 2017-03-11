// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Cake.VisualStudio.Helpers;
using Microsoft.VisualStudio.Package;
using Microsoft.VisualStudio.TextManager.Interop;
using System.Runtime.InteropServices;

namespace Cake.VisualStudio.ContentType
{
    [Guid("60914246-A28E-488D-AEB7-34CCDD35FC56")]
    class CakeLanguageService : LanguageService
    {
        private LanguagePreferences preferences = null;
        public override LanguagePreferences GetLanguagePreferences()
        {
            if (preferences == null)
            {
                preferences = new LanguagePreferences(Site, typeof(CakeLanguageService).GUID, Name);

                if (preferences != null)
                {
                    preferences.Init();
                    preferences.EnableCodeSense = true;
                    preferences.EnableMatchBraces = true;
                    preferences.EnableMatchBracesAtCaret = true;
                    preferences.EnableShowMatchingBrace = true;
                    preferences.EnableCommenting = true; ;
                    preferences.HighlightMatchingBraceFlags = _HighlightMatchingBraceFlags.HMB_USERECTANGLEBRACES;
                    preferences.LineNumbers = true;
                    preferences.MaxErrorMessages = 100;
                    preferences.AutoOutlining = false;
                    preferences.MaxRegionTime = 2000;
                    preferences.ShowNavigationBar = false;
                    preferences.InsertTabs = false;
                    preferences.IndentSize = 2;
                    preferences.ShowNavigationBar = false;
                    preferences.WordWrap = true;
                    preferences.WordWrapGlyphs = true;
                    preferences.AutoListMembers = true;
                    preferences.EnableQuickInfo = true;
                    preferences.ParameterInformation = true;
                }
            }

            return preferences;
        }

        public CakeLanguageService(object site)
        {
            SetSite(site);
        }

        public override IScanner GetScanner(IVsTextLines buffer)
        {
            return null;
        }

        public override AuthoringScope ParseSource(ParseRequest req)
        {
            return null;
        }

        public override string GetFormatFilterList()
        {
            return "Cake script (*.cake)|*.cake;";
        }

        public override string Name => Constants.CakeContentType;
    }
}

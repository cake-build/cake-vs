// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Cake.VisualStudio.Classifier.Languages
{
    static class CakeLanguage
    {
        public static List<string> SpecialKeywords
            => new List<string> {@"\bRunTarget\([\w\""]+\)", @"\bSetup\b", @"\bTeardown\b"};

        public static List<string> Preprocessors => new List<string> {@"^[#]{1}(load|l|reference|r|addin|tool)"};


        public static List<string> Keywords
            =>
                new List<string>
                {
                    @"(Task|WithCriteria|Does|IsDependentOn|OnError|ContinueOnError|ReportError|Finally|TaskSetup|TaskTeardown)\b"
                };
    }
}
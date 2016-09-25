// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace Cake.VisualStudio.Classifier.Languages
{
    static class BaseLanguage
    {
        public static List<string> Comments => new List<string> {@"//"};
        public static List<string> Quoted => new List<string> {@"([""'])(?:\\\1|.)*?\1"};

        public static List<string> Identifiers
            =>
                new List<string>
                {
                    @"\bbool\b",
                    @"\bbyte\b",
                    @"\bsbyte\b",
                    @"\bchar\b",
                    @"\bdecimal\b",
                    @"\bdouble\b",
                    @"\bfloat\b",
                    @"\bint\b",
                    @"\buint\b",
                    @"\blong\b",
                    @"\bulong\b",
                    @"\bobject\b",
                    @"\bshort\b",
                    @"\bushort\b",
                    @"\bstring\b",
                    @"\bvoid\b",
                    @"\bclass\b",
                    @"\bstruct\b",
                    @"\benum\b",
                    @"\binterface\b",
                    @"\bvar\b"
                };

        public static List<string> Operators
            => new List<string> {@"\b(new|is|as|using|checked|unchecked|typeof|sizeof|override|readonly|stackalloc)\b"};

        public static List<string> OtherKeywords
            => new List<string> {@"\b(event|delegate|fixed|add|remove|set|get|value)\b"};

        public static List<string> Linq
            =>
                new List<string>
                {
                    @"\b(from|where|select|group|into|orderby|join|let|on|equals|by|ascending|descending)\b"
                };

        public static List<string> Control
            =>
                new List<string>
                {
                    "\\b(if|else|while|for|foreach|in|do|return|continue|break|switch|case|default|goto|throw|try|catch|finally|lock|yield|await)\\b"
                };
    }
}
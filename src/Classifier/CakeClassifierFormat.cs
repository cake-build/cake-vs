// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel.Composition;
using System.Windows.Media;
using Cake.VisualStudio.Helpers;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Cake.VisualStudio.Classifier
{
    /// <summary>
    /// Defines an editor format for the CakeClassifier type that has a purple background
    /// and is underlined.
    /// </summary>
    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassifierNames.Cake)]
    [Name(ClassifierNames.Cake)]
    [UserVisible(true)] // This should be visible to the end user
    [Order(Before = Priority.Default)] // Set the priority to be after the default classifiers
    internal sealed class CakeClassifierFormat : ClassificationFormatDefinition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CakeClassifierFormat"/> class.
        /// </summary>
        public CakeClassifierFormat()
        {
            this.DisplayName = "Cake Script Classifier"; // Human readable version of the name
            this.BackgroundColor = Colors.BlueViolet;
            this.TextDecorations = System.Windows.TextDecorations.Underline;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassifierNames.Keywords)]
    [Name(ClassifierNames.Keywords)]
    internal sealed class CakeKeywordFormat : ClassificationFormatDefinition
    {
        public CakeKeywordFormat()
        {
            DisplayName = "Cake Task Declaration";
            ForegroundColor = Colors.OrangeRed;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassifierNames.SpecialKeywords)]
    [Name(ClassifierNames.SpecialKeywords)]
    internal sealed class CakeSpecialKeywordFormat : ClassificationFormatDefinition
    {
        public CakeSpecialKeywordFormat()
        {
            ForegroundColor = Colors.DodgerBlue;
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = ClassifierNames.Preprocessors)]
    [Name(ClassifierNames.Preprocessors)]
    internal sealed class CakePreprocessorFormat : ClassificationFormatDefinition
    {
        public CakePreprocessorFormat()
        {
            ForegroundColor = Colors.Orange;
        }
    }
}
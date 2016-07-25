//------------------------------------------------------------------------------
// <copyright file="CakeClassifierClassificationDefinition.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.ComponentModel.Composition;
using Cake.VisualStudio.Helpers;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Cake.VisualStudio.Classifier
{
    /// <summary>
    /// Classification type definition export for CakeClassifier
    /// </summary>
    internal static class CakeClassifierClassificationDefinition
    {
        // This disables "The field is never used" compiler's warning. Justification: the field is used by MEF.
#pragma warning disable 169

        /// <summary>
        /// Defines the "CakeClassifier" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Constants.ClassifierName)]
        private static ClassificationTypeDefinition typeDefinition;

#pragma warning restore 169
    }
}

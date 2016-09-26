// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Cake.VisualStudio.Classifier.Languages;
using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;

namespace Cake.VisualStudio.Classifier
{
    /// <summary>
    /// Classifier that classifies all text as an instance of the "CakeClassifier" classification type.
    /// </summary>
    internal class CakeClassifier : IClassifier
    {
        private readonly IDictionary<List<string>, IClassificationType> _predefinedTypes;
        private Dictionary<List<string>, IClassificationType> _dslTypes;
        private IClassificationType _commentType;

        /// <summary>
        /// Initializes a new instance of the <see cref="CakeClassifier"/> class.
        /// </summary>
        /// <param name="registry">Classification registry.</param>
        internal CakeClassifier(IClassificationTypeRegistryService registry)
        {
            _commentType = registry.GetClassificationType(PredefinedClassificationTypeNames.Comment);
            _predefinedTypes = new Dictionary<List<string>, IClassificationType>
            {
                [BaseLanguage.Quoted] = registry.GetClassificationType(PredefinedClassificationTypeNames.String),
                [BaseLanguage.Identifiers] = registry.GetClassificationType(PredefinedClassificationTypeNames.SymbolDefinition),
                [BaseLanguage.Operators] = registry.GetClassificationType(PredefinedClassificationTypeNames.Operator),
                [BaseLanguage.OtherKeywords] = registry.GetClassificationType(PredefinedClassificationTypeNames.SymbolDefinition),
                [BaseLanguage.Linq] = registry.GetClassificationType(PredefinedClassificationTypeNames.NaturalLanguage),
                [BaseLanguage.Control] = registry.GetClassificationType(PredefinedClassificationTypeNames.Keyword)
            };
            _dslTypes = new Dictionary<List<string>, IClassificationType>
            {
                [CakeLanguage.Keywords] = registry.GetClassificationType(ClassifierNames.Keywords),
                [CakeLanguage.SpecialKeywords] = registry.GetClassificationType(ClassifierNames.SpecialKeywords),
                [CakeLanguage.Preprocessors] = registry.GetClassificationType(ClassifierNames.Preprocessors)
            };
        }

#pragma warning disable 67

        /// <summary>
        /// An event that occurs when the classification of a span of text has changed.
        /// </summary>
        /// <remarks>
        /// This event gets raised if a non-text change would affect the classification in some way,
        /// for example typing /* would cause the classification to change in C# without directly
        /// affecting the span.
        /// </remarks>
        public event EventHandler<ClassificationChangedEventArgs> ClassificationChanged;

#pragma warning restore 67

        /// <summary>
        /// Gets all the <see cref="ClassificationSpan"/> objects that intersect with the given range of text.
        /// </summary>
        /// <remarks>
        /// This method scans the given SnapshotSpan for potential matches for this classification.
        /// In this instance, it classifies everything and returns each span as a new ClassificationSpan.
        /// </remarks>
        /// <param name="span">The span currently being classified.</param>
        /// <returns>A list of ClassificationSpans that represent spans identified to be of this classification.</returns>
        public IList<ClassificationSpan> GetClassificationSpans(SnapshotSpan span)
        {
            /* var result = new List<ClassificationSpan>()
            {
                new ClassificationSpan(new SnapshotSpan(span.Snapshot, new Span(span.Start, span.Length)), this.classificationType)
            };

            return result; */

            // create a list to hold the results
            List<ClassificationSpan> classifications = new List<ClassificationSpan>();
            var current = span.GetText();

            var commentFound = false;

            // Note:  Comments should go to the end of the line.
            foreach (var item in BaseLanguage.Comments)
            {
                var reg = new Regex(item, RegexOptions.IgnoreCase);
                var matches = reg.Matches(current);
                for (var i = 0; i < matches.Count; i++)
                {
                    commentFound = true;
                    var m = matches[i];
                    var newSpan = new Span(span.Start.Position + m.Index, current.Length - m.Index);
                    var newSnapshot = new SnapshotSpan(span.Snapshot, newSpan);
                    var newText = newSnapshot.GetText();
                    classifications.Add(new ClassificationSpan(newSnapshot, _commentType));
                }
            }

            if (commentFound)
            {
                return classifications;
            }

            foreach (var dslType in _dslTypes)
            {
                Classify(classifications, current, span, dslType.Key, dslType.Value);
            }

            foreach (var predefinedType in _predefinedTypes)
            {
                Classify(classifications, current, span, predefinedType.Key, predefinedType.Value);
            }

            ////Classify(classifications, current, span, _brightScriptLanguage.Quoted,
            ////              _stringType);
            ////Classify(classifications, current, span, _brightScriptLanguage.KeyWords,
            ////              _keywordType);
            ////Classify(classifications, current, span, _brightScriptLanguage.IdentifierTypes,
            ////              _identifierType);
            ////Classify(classifications, current, span, _brightScriptLanguage.Numeric,
            ////              _numericType);
            return classifications;
        }

        private void Classify(
            List<ClassificationSpan> classifications,
            string current,
            SnapshotSpan span,
            List<string> matchList,
            IClassificationType classificationType)
        {
            foreach (var item in matchList)
            {
                var reg = new Regex(item);
                var matches = reg.Matches(current);
                for (var i = 0; i < matches.Count; i++)
                {
                    var m = matches[i];
                    var newSpan = new Span(span.Start.Position + m.Index, m.Length);
                    var newSnapshot = new SnapshotSpan(span.Snapshot, newSpan);
                    var newText = newSnapshot.GetText();
                    classifications.Add(new ClassificationSpan(newSnapshot, classificationType));
                }
            }
        }
    }
}
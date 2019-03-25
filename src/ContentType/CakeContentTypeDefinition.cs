// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.ComponentModel.Composition;
using Cake.VisualStudio.Helpers;
using Microsoft.VisualStudio.LanguageServer.Client;
using Microsoft.VisualStudio.Utilities;

namespace Cake.VisualStudio.ContentType
{
    public sealed class CakeContentTypeDefinition
    {
        [Export(typeof(ContentTypeDefinition))]
        [Name(Constants.CakeContentType)]
        [BaseDefinition(Constants.BaseContentType)]
        [BaseDefinition(Constants.RoslynContentType)]
        [BaseDefinition(CodeRemoteContentDefinition.CodeRemoteContentTypeName)]
        public ContentTypeDefinition ICakeContentTypeDefinitionContentType { get; set; }

        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType(Constants.CakeContentType)]
        [FileExtension(Constants.CakeExtension)]
        public FileExtensionToContentTypeDefinition CakeFileExtensionDefinition { get; set; }
    }
}
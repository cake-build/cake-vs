using System.ComponentModel.Composition;
using Cake.VisualStudio.Helpers;
using Microsoft.VisualStudio.Utilities;

namespace Cake.VisualStudio.ContentType
{
    public sealed class CakeContentTypeDefinition
    {
        

        [Export(typeof(ContentTypeDefinition))]
        [Name(Constants.CakeContentType)]
        [BaseDefinition(Constants.BaseContentType)]
        [BaseDefinition(Constants.RoslynContentType)]
        public ContentTypeDefinition ICakeContentTypeDefinitionContentType { get; set; }

        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType(Constants.CakeContentType)]
        [FileExtension(Constants.CakeExtension)]
        public FileExtensionToContentTypeDefinition CakeFileExtensionDefinition { get; set; }
    }
}

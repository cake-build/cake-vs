using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Utilities;

namespace Cake.VisualStudio.ContentType
{
    public class CakeContentTypeDefinition
    {
        [Export(typeof(ContentTypeDefinition))]
        [Name("Cake")]
        [BaseDefinition("CSharp")]
        public ContentTypeDefinition ICakeContentType { get; set; }

        [Export(typeof(FileExtensionToContentTypeDefinition))]
        [ContentType("Cake")]
        [FileExtension(".cake")]
        public FileExtensionToContentTypeDefinition CakeFileExtension { get; set; }
    }
}
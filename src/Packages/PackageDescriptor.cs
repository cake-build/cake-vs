using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Cake.VisualStudio.Packages
{
    class PackageDescriptor
    {
        public PackageDescriptor()
        {
            
        }

        public PackageDescriptor(string id, string version, string source = "http://nuget.org/api/v2")
        {
            Id = id;
            Version = version;
            Source = source;
        }
        internal string Id { get; set; }
        internal string Version { get; set; }
        internal string Source { get; set; } = "http://nuget.org/api/v2";

        internal static IEnumerable<PackageDescriptor> ParseAllFromXml(string xml)
        {
            var doc = XDocument.Parse(xml);
            var packages = doc.XPathSelectElements("//package");
            return packages.Select(p => new PackageDescriptor(p.Attribute("id").Value, p.Attribute("version").Value));
        }
    }
}

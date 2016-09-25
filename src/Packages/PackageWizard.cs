using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using Cake.VisualStudio.Helpers;
using EnvDTE;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.TemplateWizard;
using NuGet.VisualStudio;

namespace Cake.VisualStudio.Packages
{
    public sealed class PackageWizard : IWizard
    {
        [Import]
        internal IVsTemplateWizard Wizard { get; set; }

        [Import]
        internal IVsPackageInstaller PackageInstaller { get; set; }

        public void RunStarted(object automationObject, Dictionary<string, string> replacementsDictionary, WizardRunKind runKind, object[] customParams)
        {
            if (runKind == WizardRunKind.AsNewProject)
            {
                using (var serviceProvider = new ServiceProvider((IServiceProvider)automationObject))
                {
                    var componentModel = (IComponentModel)serviceProvider.GetService(typeof(SComponentModel));
                    using (var container = new CompositionContainer(componentModel.DefaultExportProvider))
                    {
                        container.ComposeParts(this);
                    }
                }

                if (PackageInstaller == null)
                {
                    //MessageBox.Show("NuGet Package Manager not available.");

                    throw new WizardBackoutException("NuGet Package Manager not available.");
                }
                ParameterData = replacementsDictionary["$WizardData%"];
            }
            Wizard.RunStarted(automationObject, replacementsDictionary, runKind, customParams);
        }

        private string ParameterData { get; set; }

        public void ProjectFinishedGenerating(Project project)
        {
            var packages = PackageDescriptor.ParseAllFromXml(ParameterData);
            if (PackageInstaller != null)
            {
                foreach (var package in packages)
                {
                    CakePackage.Dte.ShowStatusBarText($"Installing NuGet package {package.Id}.{package.Version}");
                    PackageInstaller.InstallPackage(package.Source, project, package.Id, package.Version, true);
                }
            }
            Wizard.ProjectFinishedGenerating(project);
        }

        public void ProjectItemFinishedGenerating(ProjectItem projectItem)
        {
            Wizard.ProjectItemFinishedGenerating(projectItem);
        }

        public bool ShouldAddProjectItem(string filePath)
        {
            return Wizard.ShouldAddProjectItem(filePath);
        }

        public void BeforeOpeningFile(ProjectItem projectItem)
        {
            Wizard.BeforeOpeningFile(projectItem);
        }

        public void RunFinished()
        {
            Wizard.RunFinished();
        }
    }
}

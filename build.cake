///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// MODULES
//////////////////////////////////////////////////////////////////////

#module nuget:?package=Cake.DotNetTool.Module&version=0.4.0

//////////////////////////////////////////////////////////////////////
// TOOLS / ADDINS
//////////////////////////////////////////////////////////////////////

#addin "nuget:?package=MagicChunks&version=2.0.0.119"
#tool "dotnet:?package=GitReleaseManager.Tool&version=0.11.0"
#tool "dotnet:?package=GitVersion.Tool&version=5.5.1"

//////////////////////////////////////////////////////////////////////
// EXTERNAL SCRIPTS
//////////////////////////////////////////////////////////////////////

#load "./build/parameters.cake"

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var solutionPath = File("./Cake.VisualStudio.sln");
var solution = ParseSolution(solutionPath);
var projects = solution.Projects;
var projectPaths = projects.Select(p => p.Path.GetDirectory());
var artifacts = "./dist/";
var publishingError = false;

BuildParameters parameters = BuildParameters.GetParameters(Context, BuildSystem);

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
    parameters.SetBuildVersion(
        BuildVersion.CalculatingSemanticVersion(
            context: Context,
            parameters: parameters
        )
    );

    Information("Building version {0} of cake-vs ({1}, {2}) using version {3} of Cake. (IsTagged: {4})",
        parameters.Version.SemVersion,
        parameters.Configuration,
        parameters.Target,
        parameters.Version.CakeVersion,
        parameters.IsTagged);
});

Teardown(ctx =>
{
    // Executed AFTER the last task.
    Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

Task("Test-Pattern")
.Does(() => {
    foreach (var file in GetFiles("./template/**/Properties/AssemblyInfo.cs")) {
        Information("Match: {0}", file.FullPath);
    }
});

Task("Clean")
    .Does(() =>
{
    // Clean solution directories.
    foreach(var path in projectPaths)
    {
        Information("Cleaning {0}", path);
        CleanDirectories(path + "/**/bin/" + configuration);
        CleanDirectories(path + "/**/obj/" + configuration);
    }

    Information("Cleaning common files...");
    CleanDirectory(artifacts);
});

Task("Create-Release-Notes")
    .Does(() =>
{
    GitReleaseManagerCreate(parameters.GitHub.Token, "cake-build", "cake-vs", new GitReleaseManagerCreateSettings {
        Milestone         = parameters.Version.Milestone,
        Name              = parameters.Version.Milestone,
        Prerelease        = true,
        TargetCommitish   = "master"
    });
});

Task("Update-Manifest-Version")
    .WithCriteria(() => parameters.ShouldPublishToMyGet)
    .Does(() =>
{
    BuildVersion.UpdateManifestVersion(
        context: Context,
        path: "./src/Cake.VisualStudio/source.extension.vsixmanifest"
    );

    BuildVersion.UpdateManifestVersion(
        context: Context,
        path: "./src/Cake.VisualStudio.2022/source.extension.vsixmanifest"
    );
});

Task("Restore")
    .Does(() =>
{
    // Restore all NuGet packages.
    Information("Restoring solution...");
    NuGetRestore(solutionPath);
});

Task("Download-Nupkgs")
    .Does(() => 
{
    if (!DirectoryExists("./nupkgs"))
    {
        Information("Creating nupkgs directory...");
        CreateDirectory("./nupkgs");
    }

    var downloads = new Dictionary<string, string>();
    downloads.Add("https://www.nuget.org/api/v2/package/Cake.Core/0.38.5", "./nupkgs/cake.core.0.38.5.nupkg");
    downloads.Add("https://www.nuget.org/api/v2/package/Cake.Testing/0.38.5", "./nupkgs/cake.testing.0.38.5.nupkg");
    downloads.Add("https://www.nuget.org/api/v2/package/xunit/2.4.1", "./nupkgs/xunit.2.4.1.nupkg");
    downloads.Add("https://www.nuget.org/api/v2/package/xunit.abstractions/2.0.3", "./nupkgs/xunit.abstractions.2.0.3.nupkg");
    downloads.Add("https://www.nuget.org/api/v2/package/xunit.assert/2.4.1", "./nupkgs/xunit.assert.2.4.1.nupkg");
    downloads.Add("https://www.nuget.org/api/v2/package/xunit.core/2.4.1", "./nupkgs/xunit.core.2.4.1.nupkg");
    downloads.Add("https://www.nuget.org/api/v2/package/xunit.extensibility.core/2.4.1", "./nupkgs/xunit.extensibility.core.2.4.1.nupkg");
    downloads.Add("https://www.nuget.org/api/v2/package/xunit.extensibility.execution/2.4.1", "./nupkgs/xunit.extensibility.execution.2.4.1.nupkg");
    downloads.Add("https://www.nuget.org/api/v2/package/xunit.runner.visualstudio/2.4.3", "./nupkgs/xunit.runner.visualstudio.2.4.3.nupkg");

    foreach (var download in downloads)
    {
        if (!FileExists(download.Value))
        {
            Information("Downloading {0}", download.Key);
            DownloadFile(download.Key, download.Value);
        }
    }
});

Task("Build")
    .IsDependentOn("Clean")
    .IsDependentOn("Restore")
    .IsDependentOn("Download-Nupkgs")
    .IsDependentOn("Update-Manifest-Version")
    .Does(() =>
{
    Information("Building solution...");
    MSBuild(solutionPath, settings =>
        settings.SetPlatformTarget(PlatformTarget.MSIL)
            .SetMSBuildPlatform(MSBuildPlatform.x86)
            .UseToolVersion(MSBuildToolVersion.VS2019)
            .SetVerbosity(Verbosity.Quiet)
            .WithTarget("Build")
            .SetConfiguration(configuration));
});

Task("Post-Build")
    .IsDependentOn("Build")
    .Does(() =>
{
    CopyFile("./src/Cake.VisualStudio/bin/" + configuration + "/Cake.VisualStudio.vsix", artifacts + "/Cake.VisualStudio.2019.vsix");
    CopyFile("./src/Cake.VisualStudio.2022/bin/" + configuration + "/Cake.VisualStudio.vsix", artifacts + "/Cake.VisualStudio.2022.vsix");
});

Task("Publish-GitHub-Release")
    .WithCriteria(() => parameters.ShouldPublish)
    .Does(() =>
{
    var buildResultDir = Directory(artifacts);
    var packageFile2019 = File("Cake.VisualStudio.2019.vsix");
    var packageFile2022 = File("Cake.VisualStudio.2022.vsix");

    GitReleaseManagerAddAssets(parameters.GitHub.Token, "cake-build", "cake-vs", parameters.Version.Milestone, buildResultDir + packageFile2019);
    GitReleaseManagerAddAssets(parameters.GitHub.Token, "cake-build", "cake-vs", parameters.Version.Milestone, buildResultDir + packageFile2022);
    GitReleaseManagerClose(parameters.GitHub.Token, "cake-build", "cake-vs", parameters.Version.Milestone);
})
.OnError(exception =>
{
    Information("Publish-GitHub-Release Task failed, but continuing with next Task...");
    publishingError = true;
});

Task("Upload-Artifact")
    .IsDependentOn("Post-Build")
    .WithCriteria(() => parameters.ShouldPublishToAppVeyor)
    .Does(() =>
{
    AppVeyor.UploadArtifact(artifacts + "Cake.VisualStudio.2019.vsix");
    AppVeyor.UploadArtifact(artifacts + "Cake.VisualStudio.2022.vsix");
});

Task("Publish-Extension")
    .IsDependentOn("Post-Build")
    .WithCriteria(() => parameters.ShouldPublishToMyGet)
    .Does(() =>
{
    var client = MyGetClient.GetClient(parameters.MyGet.Url, parameters.MyGet.Key, s => Context.Verbose(s));
    Information("Uploading VSIXs to {0}...", parameters.MyGet.Url);
    var response = client.UploadVsix(GetFile(artifacts + "Cake.VisualStudio.2019.vsix"));
    Information("VSIX Upload (2019) {0}", response.IsSuccessStatusCode ? "succeeded." : "failed with reason '" + response.ReasonPhrase + "'.");
    response = client.UploadVsix(GetFile(artifacts + "Cake.VisualStudio.2022.vsix"));
    Information("VSIX Upload (2019) {0}", response.IsSuccessStatusCode ? "succeeded." : "failed with reason '" + response.ReasonPhrase + "'.");
});

Task("Default")
    .IsDependentOn("Post-Build");

Task("AppVeyor")
    .IsDependentOn("Upload-Artifact")
    .IsDependentOn("Publish-Extension");

RunTarget(target);


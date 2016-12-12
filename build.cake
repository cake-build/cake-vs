///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

//////////////////////////////////////////////////////////////////////
// TOOLS
//////////////////////////////////////////////////////////////////////

#tool "nuget:?package=gitreleasemanager&version=0.6.0"
#tool "nuget:?package=GitVersion.CommandLine&version=3.4.1"

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
    GitReleaseManagerCreate(parameters.GitHub.UserName, parameters.GitHub.Password, "cake-build", "cake-vs", new GitReleaseManagerCreateSettings {
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
		path: "./src/source.extension.vsixmanifest"
	);
});

Task("Restore")
	.Does(() =>
{
	// Restore all NuGet packages.
	Information("Restoring solution...");
	NuGetRestore(solutionPath);
});

Task("Build")
	.IsDependentOn("Clean")
	.IsDependentOn("Restore")
	.Does(() =>
{
	Information("Building solution...");
	MSBuild(solutionPath, settings =>
		settings.SetPlatformTarget(PlatformTarget.MSIL)
            .SetMSBuildPlatform(MSBuildPlatform.x86)
			.WithProperty("TreatWarningsAsErrors","true")
			.SetVerbosity(Verbosity.Quiet)
			.WithTarget("Build")
			.SetConfiguration(configuration));
});

Task("Post-Build")
	.IsDependentOn("Build")
	.Does(() =>
{
    CopyFileToDirectory("./src/bin/" + configuration + "/Cake.VisualStudio.vsix", artifacts);
});

Task("Publish-GitHub-Release")
    .WithCriteria(() => parameters.ShouldPublish)
    .Does(() =>
{
    var buildResultDir = Directory(artifacts);
    var packageFile = File("Cake.VisualStudio.vsix");

    GitReleaseManagerAddAssets(parameters.GitHub.UserName, parameters.GitHub.Password, "cake-build", "cake-vs", parameters.Version.Milestone, buildResultDir + packageFile);
    GitReleaseManagerClose(parameters.GitHub.UserName, parameters.GitHub.Password, "cake-build", "cake-vs", parameters.Version.Milestone);
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
	AppVeyor.UploadArtifact(artifacts + "Cake.VisualStudio.vsix");
});

Task("Publish-Extension")
    .IsDependentOn("Post-Build")
    .WithCriteria(() => parameters.ShouldPublishToMyGet)
    .Does(() => 
{
	var client = MyGetClient.GetClient(parameters.MyGet.Url, parameters.MyGet.Key);
	client.UploadVsix(GetFile(artifacts + "Cake.VisualStudio.vsix"));
});

Task("Default")
	.IsDependentOn("Post-Build");

Task("AppVeyor")
	.IsDependentOn("Upload-Artifact")
    .IsDependentOn("Publish-Extension");

RunTarget(target);

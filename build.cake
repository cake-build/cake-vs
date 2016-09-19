///////////////////////////////////////////////////////////////////////////////
// ARGUMENTS
///////////////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var configuration = Argument("configuration", "Release");

///////////////////////////////////////////////////////////////////////////////
// GLOBAL VARIABLES
///////////////////////////////////////////////////////////////////////////////

var solutionPath = File("./Cake.VisualStudio.sln");
var solution = ParseSolution(solutionPath);
var projects = solution.Projects;
var projectPaths = projects.Select(p => p.Path.GetDirectory());
var artifacts = "./dist/";

var accountVar = "Marketplace_Account";
var tokenVar = "Marketplace_Token";

///////////////////////////////////////////////////////////////////////////////
// SETUP / TEARDOWN
///////////////////////////////////////////////////////////////////////////////

Setup(ctx =>
{
	// Executed BEFORE the first task.
	Information("Running tasks...");
});

Teardown(ctx =>
{
	// Executed AFTER the last task.
	Information("Finished running tasks.");
});

///////////////////////////////////////////////////////////////////////////////
// TASKS
///////////////////////////////////////////////////////////////////////////////

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

Task("Publish")
    .IsDependentOn("Post-Build")
    .WithCriteria(AppVeyor.IsRunningOnAppVeyor)
    .Does(() => 
{
    AppVeyor.UploadArtifact(artifacts + "Cake.VisualStudio.vsix");
});

Task("Default")
	.IsDependentOn("Publish");

RunTarget(target);

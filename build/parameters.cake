#load "./version.cake"
#load "./myget.cake"

public class BuildParameters
{
    public string Target { get; private set; }
    public string Configuration { get; private set; }
    public bool IsLocalBuild { get; private set; }
    public bool IsRunningOnUnix { get; private set; }
    public bool IsRunningOnWindows { get; private set; }
    public bool IsRunningOnAppVeyor { get; private set; }
    public bool IsPullRequest { get; private set; }
    public bool IsMasterCakeVsRepo { get; private set; }
    public bool IsMasterCakeVsBranch { get; private set; }
    public bool IsTagged { get; private set; }
    public bool IsPublishBuild { get; private set; }
    public bool IsReleaseBuild { get; private set; }
    public bool SkipGitVersion { get; private set; }
    public BuildCredentials GitHub { get; private set; }
    public MyGetFeed MyGet {get; private set; }
    public BuildVersion Version { get; private set; }

    public bool ShouldPublish
    {
        get
        {
            return !IsLocalBuild && !IsPullRequest && IsMasterCakeVsRepo
                && IsMasterCakeVsBranch && IsTagged;
        }
    }

    public bool ShouldPublishToMyGet
    {
        get
        {
            return IsRunningOnAppVeyor && !IsPullRequest && IsMasterCakeVsRepo;
        }
    }

    public bool ShouldPublishToAppVeyor
    {
        get
        {
            return IsRunningOnAppVeyor && IsMasterCakeVsRepo;
        }
    } 

    public void SetBuildVersion(BuildVersion version)
    {
        Version  = version;
    }

    public static BuildParameters GetParameters(
        ICakeContext context,
        BuildSystem buildSystem
        )
    {
        if (context == null)
        {
            throw new ArgumentNullException("context");
        }

        var target = context.Argument("target", "Default");

        return new BuildParameters {
            Target = target,
            Configuration = context.Argument("configuration", "Release"),
            IsLocalBuild = buildSystem.IsLocalBuild,
            IsRunningOnUnix = context.IsRunningOnUnix(),
            IsRunningOnWindows = context.IsRunningOnWindows(),
            IsRunningOnAppVeyor = buildSystem.AppVeyor.IsRunningOnAppVeyor,
            IsPullRequest = buildSystem.AppVeyor.Environment.PullRequest.IsPullRequest,
            IsMasterCakeVsRepo = StringComparer.OrdinalIgnoreCase.Equals("cake-build/cake-vs", buildSystem.AppVeyor.Environment.Repository.Name),
            IsMasterCakeVsBranch = StringComparer.OrdinalIgnoreCase.Equals("master", buildSystem.AppVeyor.Environment.Repository.Branch),
            IsTagged = (
                buildSystem.AppVeyor.Environment.Repository.Tag.IsTag &&
                !string.IsNullOrWhiteSpace(buildSystem.AppVeyor.Environment.Repository.Tag.Name)
            ),
            GitHub = new BuildCredentials (
                userName: context.EnvironmentVariable("CAKEVS_GITHUB_USERNAME"),
                password: context.EnvironmentVariable("CAKEVS_GITHUB_PASSWORD")
            ),
            MyGet = new MyGetFeed (
                feedUrl: context.EnvironmentVariable("MYGET_VSIX_API_URL"),
                apiKey: context.EnvironmentVariable("MYGET_VSIX_API_KEY")
            ),
            IsPublishBuild = new [] {
                "ReleaseNotes",
                "Create-Release-Notes"
            }.Any(
                releaseTarget => StringComparer.OrdinalIgnoreCase.Equals(releaseTarget, target)
            ),
            IsReleaseBuild = new string[] {
            }.Any(
                publishTarget => StringComparer.OrdinalIgnoreCase.Equals(publishTarget, target)
            ),
            SkipGitVersion = StringComparer.OrdinalIgnoreCase.Equals("True", context.EnvironmentVariable("CAKE_SKIP_GITVERSION"))
        };
    }
}

public class BuildCredentials
{
    public string UserName { get; private set; }
    public string Password { get; private set; }

    public BuildCredentials(string userName, string password)
    {
        UserName = userName;
        Password = password;
    }
}
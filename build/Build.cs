using System;
using System.Linq;
using Nuke.Common;
using Nuke.Common.CI.AppVeyor;
using Nuke.Common.Execution;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[CheckBuildProjectConfigurations]
[UnsetVisualStudioEnvironmentVariables]
class Build : NukeBuild
{
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode

    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Configuration to build - Default is 'Debug' (local) or 'Release' (server)")]
    readonly Configuration Configuration = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    [Parameter("Nuget API key for publishing packages")] readonly string NugetKey;

    [Solution] readonly Solution Solution;
    [GitRepository] readonly GitRepository GitRepository;
    [GitVersion] readonly GitVersion GitVersion;

    AbsolutePath SourceDirectory => RootDirectory / "src";
    AbsolutePath OutputDirectory => RootDirectory / "out";

    Target Clean => _ => _
        .Before(Restore)
        .Executes(() =>
        {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
        });

    Target Restore => _ => _
        .Executes(() =>
        {
            DotNetRestore(s => s
                .SetProjectFile(Solution));
        });

    Target Compile => _ => _
        .DependsOn(Restore)
        .Executes(() =>
        {
            DotNetBuild(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .SetAssemblyVersion(GitVersion.AssemblySemVer)
                .SetFileVersion(GitVersion.AssemblySemFileVer)
                .SetInformationalVersion(GitVersion.InformationalVersion)
                .EnableNoRestore());
        });

    Target Test => _ => _
        .DependsOn(Compile)
        .Executes(() =>
        {
            DotNetTest(s => s
                .SetProjectFile(Solution)
                .SetConfiguration(Configuration)
                .EnableNoBuild());
        });

    Target Pack => _ => _
        .DependsOn(Test)
        .OnlyWhenDynamic(() => IsLocalBuild || AppVeyor.Instance.RepositoryTag)
        .Executes(() =>
        {
            DotNetPack(s => s
                .SetProject(Solution)
                .SetOutputDirectory(OutputDirectory)
                .SetVersion(GitVersion.AssemblySemVer)
                .SetConfiguration(Configuration)
                .EnableNoBuild());
        });

    Target Publish => _ => _
        .DependsOn(Pack)
        .OnlyWhenDynamic(() => IsLocalBuild || AppVeyor.Instance.RepositoryTag,
                         () => AppVeyor.Instance != null && AppVeyor.Instance.RepositoryBranch == "master",
                         () => AppVeyor.Instance != null && !string.IsNullOrWhiteSpace(AppVeyor.Instance.RepositoryTagName))
        .Executes(() =>
        {
            DotNetNuGetPush(s => s
                .SetApiKey(NugetKey)
                .SetSource("https://api.nuget.org/v3/index.json")
                .SetTargetPath(OutputDirectory));
        });

    Target CompleteWorkflow => _ => _
        .Triggers(Publish);
}

using System;
using System.IO;
using System.Linq;
using Nuke.Common;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Utilities.Collections;
using static Nuke.Common.ControlFlow;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Logger;

[UnsetVisualStudioEnvironmentVariables]
class SiteBuild : NukeBuild
{
    public static int Main () => Execute<SiteBuild>(build => build.UpdateSeeds);

    [Parameter("News source address")]
    readonly string NewsUrl;

    [PathExecutable] readonly Tool PowerShell;
    [PackageExecutable(
        packageId: "Wyam",
        packageExecutable: "Wyam.exe",
        Version = "1.4.1")]
    readonly Tool Wyam;

    AbsolutePath InputDirectory => RootDirectory / "input";

    AbsolutePath OutputDirectory => RootDirectory / "output";

    AbsolutePath InputNewsDirectory => InputDirectory / "News";

    AbsolutePath WyamConfigFile => RootDirectory / "config.wyam";

    AbsolutePath SeedsDirectory => RootDirectory / ".." / "Seeds";

    Target Dump => _ => _
        .Executes(() =>
        {
            Info($"root: {RootDirectory}");
            Info($"input: {InputDirectory}");
            Info($"output: {OutputDirectory}");
            Info($"wyam: {WyamConfigFile}");
            Info($"seeds: {SeedsDirectory}");
        });

    Target Clean => _ => _
        .Executes(() =>
        {
            EnsureCleanDirectory(OutputDirectory);
        });

    Target Build => _ => _
        .DependsOn(Clean)
        .DependsOn(Dump)
        .Executes(() =>
        {
            Wyam($"--input \"{InputDirectory}\" --output \"{OutputDirectory}\" --config \"{WyamConfigFile}\" \"{RootDirectory}\"", workingDirectory: RootDirectory);
        });

    Target UpdateSeeds => _ => _
        .DependsOn(Build)
        .Executes(() =>
        {
            OutputDirectory.GlobDirectories("*").ForEach(UpdateSeed);
            var statusFile = SeedsDirectory / "Get-SubGitStatus.ps1";
            PowerShell($"-NoProfile -ExecutionPolicy Unrestricted -File \"{statusFile}\" -Path \"{SeedsDirectory}\"", workingDirectory: SeedsDirectory);

            void UpdateSeed(AbsolutePath inputSeedDirectory)
            {
                var seedName = Path.GetFileName(inputSeedDirectory);
                Info($"::::::::::::: {seedName} :::::::::::::");

                var outputSeedDirectory = SeedsDirectory / seedName;
                Assert(DirectoryExists(outputSeedDirectory), $"Output seed directory {outputSeedDirectory} not found. Run: Boombr Sync-CommunityEnv");

                CleanDirectory(outputSeedDirectory, ExceptGitDirectory);

                Info($"Copy Seed directory: {inputSeedDirectory}");
                CopyDirectoryRecursively(inputSeedDirectory, outputSeedDirectory, DirectoryExistsPolicy.Merge);
            }

            static bool ExceptGitDirectory(string path)
            {
                return
                    !path.EndsWith("/.git") &&
                    !path.Contains("/.git/") &&
                    !path.EndsWith("\\.git") &&
                    !path.Contains("\\.git\\");
            }
        });

    void CleanDirectory(AbsolutePath directory, Func<string, bool> filter)
    {
        Info($"Cleaning directory '{directory} with filter'...");

        Directory
            .GetFiles(directory)
            .Where(filter)
            .ForEach(DeleteFile);

        Directory
            .GetDirectories(directory)
            .Where(filter)
            .ForEach(DeleteDirectory);
    }

    Target News => _ => _
        .Requires(() => NewsUrl)
        .Executes(() =>
        {
            Info($"Creating News based on: {NewsUrl}");
            var url = new Uri(NewsUrl);
            var writer = new NewsWriter(InputNewsDirectory, Environment.UserName);

            writer.BootstrapUrl(url);
        });
}

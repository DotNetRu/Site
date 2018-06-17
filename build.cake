#tool nuget:?package=Wyam&version=1.4.1
#addin nuget:?package=Cake.Wyam&version=1.4.1

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var isLocal = BuildSystem.IsLocalBuild;
var isRunningOnAppVeyor = AppVeyor.IsRunningOnAppVeyor;
var isPullRequest = AppVeyor.Environment.PullRequest.IsPullRequest;
var buildNumber = AppVeyor.Environment.Build.Number;

var outputDir = Directory("./output");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Dump")
    .Does(() =>
{
    Information($"isLocal = {isLocal}");
    Information($"isRunningOnAppVeyor = {isRunningOnAppVeyor}");
    Information($"isPullRequest = {isPullRequest}");
    Information($"buildNumber = {buildNumber}");
});

Task("Clean")
    .Does(() =>
    {
        CleanDirectory(outputDir);
    });

Task("Build")
    .Description("Generates the site.")
    .IsDependentOn("Clean")
    .Does(() =>
    {
        Wyam(new WyamSettings
        {
            //UpdatePackages = true,
            ConfigurationFile = File("./config.wyam"),
            OutputPath = outputDir
        });
    });

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Dump")
    .IsDependentOn("Build");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

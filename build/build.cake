#tool nuget:?package=Wyam&version=1.4.1
#addin nuget:?package=Cake.Wyam&version=1.4.1

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var root = Directory(MakeAbsolute(Directory("..")).FullPath);
var inputDir = root + Directory("input");
var outputDir = root + Directory("output");
var wyamFile = root + File("config.wyam");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Dump")
    .Does(() =>
{
    Information($"root [{root.GetType().Name}]: {root}");
    Information($"input [{inputDir.GetType().Name}]: {inputDir}");
    Information($"output [{outputDir.GetType().Name}]: {outputDir}");
    Information($"wyam [{wyamFile.GetType().Name}]: {wyamFile}");
});

Task("Clean")
    .Does(() =>
{
    CleanDirectory(outputDir);
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    Wyam(new WyamSettings
    {
        RootPath = root,
        ConfigurationFile = wyamFile,
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

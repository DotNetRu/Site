#tool nuget:?package=Wyam&version=1.4.1
#addin nuget:?package=Cake.Wyam&version=1.4.1

#load "./sitemeta.cake"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var userName = EnvironmentVariable("USERNAME");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var root = Directory(MakeAbsolute(Directory("..")).FullPath);
var inputDir = root + Directory("input");
var outputDir = root + Directory("output");
var newsDir = inputDir + Directory("News");
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

Task("News")
    .Does(() =>
{
    var urlText = Argument<string>("url");
    Information($"Creating News based on: {urlText}");

    var url = new Uri(urlText);
    var writer = new NewsWriter(Context, newsDir, userName);

    writer.BootstrapUrl(url);
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

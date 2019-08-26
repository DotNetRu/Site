#tool nuget:?package=Wyam&version=1.4.1
#addin nuget:?package=Cake.Wyam&version=1.4.1

#load "./NewsWriter.cake"

//////////////////////////////////////////////////////////////////////
// ARGUMENTS
//////////////////////////////////////////////////////////////////////

var target = Argument("target", "Default");
var userName = EnvironmentVariable("USERNAME");

//////////////////////////////////////////////////////////////////////
// PREPARATION
//////////////////////////////////////////////////////////////////////

var root = Directory(MakeAbsolute(Directory("..")).FullPath);
var inputGeneratorDir = root + Directory("input");
var outputGeneratorDir = root + Directory("output");
var inputGeneratorNewsDir = inputGeneratorDir + Directory("News");
var wyamFile = root + File("config.wyam");
var seedDir = Directory(System.IO.Path.GetFullPath(root + Directory("../Seeds")));

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

Task("Dump")
    .Does(() =>
{
    Information($"root [{root.GetType().Name}]: {root}");
    Information($"inputGeneratorDir [{inputGeneratorDir.GetType().Name}]: {inputGeneratorDir}");
    Information($"outputGeneratorDir [{outputGeneratorDir.GetType().Name}]: {outputGeneratorDir}");
    Information($"seedDir [{seedDir.GetType().Name}]: {seedDir}");
});

Task("Clean")
    .Does(() =>
{
    CleanDirectory(outputGeneratorDir);
});

Task("Build")
    .IsDependentOn("Clean")
    .Does(() =>
{
    Wyam(new WyamSettings
    {
        RootPath = root,
        ConfigurationFile = wyamFile,
        OutputPath = outputGeneratorDir
    });
});

Task("News")
    .Does(() =>
{
    var urlText = Argument<string>("url");
    Information($"Creating News based on: {urlText}");

    var url = new Uri(urlText);
    var writer = new NewsWriter(Context, inputGeneratorNewsDir, userName);

    writer.BootstrapUrl(url);
});

Task("Update-Seeds")
    .Description("Update Seed repositories from site generated output")
    .IsDependentOn("Build")
    .DoesForEach(GetDirectories($"{outputGeneratorDir}/*"), inputSeedDir =>
{
    var seedName = inputSeedDir.GetDirectoryName();
    Information($"::::::::::::: {seedName} :::::::::::::");

    var outputSeedDir = seedDir + Directory(seedName);
    if (!DirectoryExists(outputSeedDir))
    {
        throw new Exception($"Output seed directory {outputSeedDir} not found. Run: Boombr Sync-CommunityEnv");
    }

    CleanGitDirectory(outputSeedDir);

    Information($"Copy Seed directory: {inputSeedDir}");
    CopyDirectory(inputSeedDir, outputSeedDir);
})
.Finally(() =>
{
    // StartPowershellFile("./Get-SubGitStatus.ps1", args =>
    // {
    //     args
    //         .Append("Path", inputSeedDir);
    // });
    StartProcess("powershell", new ProcessSettings
    {
        Arguments = $"-NoProfile -ExecutionPolicy Unrestricted -File ./Get-SubGitStatus.ps1 -Path \"{seedDir}\""
    });
});

//////////////////////////////////////////////////////////////////////
// PRIVATE FUNCTIONS
//////////////////////////////////////////////////////////////////////

void CleanGitDirectory(DirectoryPath directory)
{
    Information($"Clear Git directory: {directory}");

    bool isNotGitPath(Cake.Core.IO.Path path)
    {
        return
            !path.FullPath.EndsWith("/.git") &&
            !path.FullPath.Contains("/.git/");
    }

    GetPaths($"{directory}/*")
        .Where(isNotGitPath)
        .ToList()
        .ForEach(DeletePath);
}

void DeletePath(Cake.Core.IO.Path path)
{
    var fullPath = path.FullPath;
    if (FileExists(fullPath))
    {
        Verbose($"  Delete file: {fullPath}");
        DeleteFile(fullPath);
    }
    else
    {
        Verbose($"  Delete directory: {fullPath}");
        DeleteDirectory(fullPath, new DeleteDirectorySettings
        {
            Recursive = true,
            Force = true
        });
    }
}

//////////////////////////////////////////////////////////////////////
// TASK TARGETS
//////////////////////////////////////////////////////////////////////

Task("Default")
    .IsDependentOn("Dump")
    .IsDependentOn("Update-Seeds");

//////////////////////////////////////////////////////////////////////
// EXECUTION
//////////////////////////////////////////////////////////////////////

RunTarget(target);

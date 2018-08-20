Param
(
    [String]$CakeVersion = "0.26.1",
    [String]$ToolsDir = "$PSScriptRoot\tools",
    [String]$ToolsProj = "$ToolsDir\build.csproj",
    [String]$BuildFile = "$PSScriptRoot\build.cake",
    [String]$Target = 'Default',
    [String]$Verbosity = 'normal',
    [string[]]$ScriptArgs = @()
)

$CAKE_DIR = "$ToolsDir\Cake.CoreCLR.$CakeVersion"
$CAKE_DLL = "$CAKE_DIR\cake.coreclr\$CakeVersion\Cake.dll"

If (!(Test-Path $ToolsDir))
{
    New-Item -ItemType Directory -Force -Path $ToolsDir
}

if (!(Test-Path $ToolsProj))
{
    $projectFileContents = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>netcoreapp2.0</TargetFramework></PropertyGroup></Project>'
    Out-File -InputObject $projectFileContents -FilePath $ToolsProj
}

dotnet add "$ToolsProj" package cake.coreclr -v "$CakeVersion" --package-directory "$CAKE_DIR"
 
if (!(Test-Path $CAKE_DLL))
{
    Write-Error "Could not find Cake assembly '$CAKE_DLL'"
}
else
{
    dotnet "$CAKE_DLL" "$BuildFile" --target="$Target" --verbosity="$Verbosity" $ScriptArgs
}

exit $LASTEXITCODE
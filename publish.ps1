param(
    [string] $Architecture = "x64",
    [string] $Version = "0.1.0.0"
)

$ErrorActionPreference = "Stop";

Write-Output "Start building withRuntime...";

dotnet publish src/KanaPlayer.Windows/KanaPlayer.Windows.csproj -c Release -r "win-$Architecture" -o "build/$Version/withRuntime" -p:Platform=$Architecture -p:PublishReadyToRun=true -p:SelfContained=true -p:AssemblyVersion=$Version -p:Configuration=Release;

Write-Output "Start building withoutRuntime...";

dotnet publish  src/KanaPlayer.Windows/KanaPlayer.Windows.csproj -c Release -r "win-$Architecture" -o "build/$Version/withoutRuntime" -p:Platform=$Architecture -p:PublishReadyToRun=true -p:SelfContained=false -p:AssemblyVersion=$Version -p:Configuration=Release;

Write-Output "Build Finished";

[Console]::ReadKey()
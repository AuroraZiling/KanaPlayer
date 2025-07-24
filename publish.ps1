param(
    [string] $Architecture = "x64",
    [string] $Version = "0.1.0.0"
)

$ErrorActionPreference = "Stop";

Write-Output "Start building launcher...";

Set-Location src/KanaPlayer.Launcher;
xmake;
Set-Location ../..;

Write-Output "Start building withRuntime...";

dotnet publish src/KanaPlayer.Windows/KanaPlayer.Windows.csproj -c Release -r "win-$Architecture" -o "build/$Version/withRuntime/KanaApp" -p:Platform=$Architecture -p:PublishReadyToRun=true -p:SelfContained=true -p:AssemblyVersion=$Version -p:Configuration=Release;

Copy-Item -Path ".\src\KanaPlayer.Launcher\build\windows\x64\release\KanaLauncher.exe" -Destination ".\build\$Version\withRuntime\KanaPlayer.exe"

Write-Output "Start building withoutRuntime...";

dotnet publish  src/KanaPlayer.Windows/KanaPlayer.Windows.csproj -c Release -r "win-$Architecture" -o "build/$Version/withoutRuntime/KanaApp" -p:Platform=$Architecture -p:PublishReadyToRun=true -p:SelfContained=false -p:AssemblyVersion=$Version -p:Configuration=Release;

Copy-Item -Path ".\src\KanaPlayer.Launcher\build\windows\x64\release\KanaLauncher.exe" -Destination ".\build\$Version\withoutRuntime\KanaPlayer.exe"

Write-Output "Build Finished";

[Console]::ReadKey()
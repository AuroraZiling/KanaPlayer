param(
    [string] $Architecture = "x64",
    [string] $Version = "0.2.0"
)

$ErrorActionPreference = "Stop";

Write-Output "Start building launcher...";

Set-Location src/KanaPlayer.Launcher;
xmake;
Set-Location ../..;

Write-Output "Start building withRuntime...";

dotnet publish src/KanaPlayer.Windows/KanaPlayer.Windows.csproj -c Release -r "win-$Architecture" -o "build/$Version/withRuntime/KanaApp" -p:SelfContained=true -p:AssemblyVersion=$Version -p:Configuration=Release;

Copy-Item -Path ".\src\KanaPlayer.Launcher\build\windows\x64\release\KanaLauncher.exe" -Destination ".\build\$Version\withRuntime\KanaPlayer.exe"

Write-Output "Start building withoutRuntime...";

dotnet publish  src/KanaPlayer.Windows/KanaPlayer.Windows.csproj -c Release -r "win-$Architecture" -o "build/$Version/withoutRuntime/KanaApp" -p:Platform=$Architecture -p:SelfContained=false -p:AssemblyVersion=$Version -p:Configuration=Release;

Copy-Item -Path ".\src\KanaPlayer.Launcher\build\windows\x64\release\KanaLauncher.exe" -Destination ".\build\$Version\withoutRuntime\KanaPlayer.exe"

Write-Output "Build Finished";

Write-Output "Creating zip packages...";

$zipOutputDir = "build/$Version"
if (!(Test-Path $zipOutputDir)) {
    New-Item -ItemType Directory -Path $zipOutputDir -Force
}

$withRuntimeSource = "build/$Version/withRuntime"
$withRuntimeZip = "$zipOutputDir/KanaPlayer-$Version-win-$Architecture-runtime.zip"
Compress-Archive -Path "$withRuntimeSource\*" -DestinationPath $withRuntimeZip -Force

$withoutRuntimeSource = "build/$Version/withoutRuntime"
$withoutRuntimeZip = "$zipOutputDir/KanaPlayer-$Version-win-$Architecture.zip"
Compress-Archive -Path "$withoutRuntimeSource\*" -DestinationPath $withoutRuntimeZip -Force

Write-Output "Zip packages created:";
Write-Output "  - $withRuntimeZip";
Write-Output "  - $withoutRuntimeZip";

[Console]::ReadKey()
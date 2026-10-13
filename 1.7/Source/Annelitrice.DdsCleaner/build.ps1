param(
    [string]$RimWorldDirectory = '',
    [string]$HarmonyAssembly = '',
    [string]$OutputDirectory = ''
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
if(-not $RimWorldDirectory) { $RimWorldDirectory = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\..\..\..')) }
if(-not $OutputDirectory) { $OutputDirectory = Join-Path $PSScriptRoot 'bin\Release' }
$gameAssembly = Join-Path $RimWorldDirectory 'Assembly-CSharp.dll'
if(-not (Test-Path -LiteralPath $gameAssembly)) { throw "RimWorld 1.7 Assembly-CSharp.dll not found: $gameAssembly. Pass -RimWorldDirectory." }
if(-not $HarmonyAssembly) {
    $steamApps = [IO.Path]::GetFullPath((Join-Path $RimWorldDirectory '..\..'))
    $candidates = @(
        (Join-Path $steamApps 'workshop\content\294100\2009463077\1.7\Assemblies\0Harmony.dll'),
        (Join-Path $RimWorldDirectory 'Mods\Harmony\1.7\Assemblies\0Harmony.dll'),
        (Join-Path $RimWorldDirectory 'Mods\Harmony\Current\Assemblies\0Harmony.dll')
    )
    $HarmonyAssembly = $candidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
}
if(-not $HarmonyAssembly -or -not (Test-Path -LiteralPath $HarmonyAssembly)) { throw 'Harmony 1.7 not found. Pass -HarmonyAssembly pointing to its 0Harmony.dll.' }
$dotnet = (Get-Command dotnet -ErrorAction Stop).Source
$sdkLines = @(& $dotnet --list-sdks)
if($LASTEXITCODE -ne 0) { throw 'Could not list installed .NET SDKs.' }
$sdks = foreach($line in $sdkLines) {
    if($line -match '^(\S+)\s+\[(.+)\]$') {
        $sdkVersion = $Matches[1]
        [pscustomobject]@{Version=[version]($sdkVersion -replace '-.*$',''); Compiler=(Join-Path $Matches[2] "$sdkVersion\Roslyn\bincore\csc.dll")}
    }
}
$compiler = ($sdks | Sort-Object Version -Descending | Select-Object -First 1).Compiler
if(-not (Test-Path -LiteralPath $compiler)) { throw 'A .NET SDK with the Roslyn C# compiler is required.' }
[IO.Directory]::CreateDirectory($OutputDirectory) | Out-Null
$outputAssembly = Join-Path $OutputDirectory 'Annelitrice.DdsCleaner.dll'
# Compile against the runtime that ships with the game. This works with an SDK9
# compiler even when a .NET10 SDK is not installed; no NuGet packages are used.
$referenceNames = @(
    'Assembly-CSharp.dll','UnityStubs.dll','Waylith.UnityCompat.dll',
    'System.Private.CoreLib.dll','System.Runtime.dll','System.Collections.dll',
    'System.Linq.dll','System.IO.FileSystem.dll','System.IO.FileSystem.Primitives.dll',
    'System.Xml.ReaderWriter.dll','System.Xml.XmlDocument.dll','System.Private.Xml.dll'
)
$arguments = [Collections.Generic.List[string]]::new()
foreach($arg in @('/nostdlib+','/target:library','/langversion:latest','/optimize+','/deterministic+','/warn:4',('/out:"' + $outputAssembly + '"'))) { $arguments.Add($arg) }
foreach($name in $referenceNames) {
    $path = Join-Path $RimWorldDirectory $name
    if(-not (Test-Path -LiteralPath $path)) { throw "Game reference missing: $path" }
    $arguments.Add('/reference:"' + $path + '"')
}
$arguments.Add('/reference:"' + $HarmonyAssembly + '"')
foreach($source in @('AssemblyInfo.cs','DdsCleanup.cs','DdsDiscovery.cs','DdsCleanerMod.cs')) { $arguments.Add('"' + (Join-Path $PSScriptRoot $source) + '"') }
$responseFile = Join-Path $OutputDirectory 'compile.rsp'
[IO.File]::WriteAllLines($responseFile,$arguments,[Text.UTF8Encoding]::new($false))
& $dotnet $compiler /noconfig ('@' + $responseFile)
if($LASTEXITCODE -ne 0) { throw 'DDS cleaner compilation failed.' }
Write-Output "Built: $outputAssembly"
Write-Output 'To deploy, copy only Annelitrice.DdsCleaner.dll into this mod version''s Assemblies folder, and copy the Languages folders alongside it. Do not copy game or Harmony DLLs.'

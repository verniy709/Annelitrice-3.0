$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest
$sourceDirectory = Split-Path $PSScriptRoot -Parent
$dotnet = (Get-Command dotnet -ErrorAction Stop).Source
$dotnetDirectory = Split-Path $dotnet -Parent
$sdks = foreach($line in @(& $dotnet --list-sdks)) {
    if($line -match '^(\S+)\s+\[(.+)\]$') {
        $sdkVersion=$Matches[1]
        [pscustomobject]@{Version=[version]($sdkVersion -replace '-.*$','');Compiler=(Join-Path $Matches[2] "$sdkVersion\Roslyn\bincore\csc.dll")}
    }
}
$compiler=($sdks | Sort-Object Version -Descending | Select-Object -First 1).Compiler
$packs=Get-ChildItem -LiteralPath (Join-Path $dotnetDirectory 'packs\Microsoft.NETCore.App.Ref') -Directory | Where-Object Name -match '^\d+\.\d+\.\d+$' | Sort-Object { [version]$_.Name } -Descending
$pack=$packs | Select-Object -First 1
$framework='net' + ([version]$pack.Name).Major + '.0'
$references=Join-Path $pack.FullName "ref\$framework"
foreach($suite in @('Engine','UI')) {
    $output=Join-Path $PSScriptRoot "bin\$suite"
    [IO.Directory]::CreateDirectory($output) | Out-Null
    $assembly=Join-Path $output "DdsCleaner.$suite.Tests.dll"
    $arguments=[Collections.Generic.List[string]]::new()
    foreach($arg in @('/nologo','/nostdlib+','/target:exe','/langversion:latest',('/out:"' + $assembly + '"'))) { $arguments.Add($arg) }
    foreach($ref in Get-ChildItem -LiteralPath $references -Filter '*.dll') { $arguments.Add('/reference:"' + $ref.FullName + '"') }
    foreach($source in @('DdsCleanup.cs','DdsDiscovery.cs')) { $arguments.Add('"' + (Join-Path $sourceDirectory $source) + '"') }
    if($suite -eq 'UI') { $arguments.Add('"' + (Join-Path $sourceDirectory 'DdsCleanerMod.cs') + '"') }
    foreach($source in Get-ChildItem -LiteralPath (Join-Path $PSScriptRoot $suite) -Filter '*.cs') { $arguments.Add('"' + $source.FullName + '"') }
    $response=Join-Path $output 'compile.rsp'
    [IO.File]::WriteAllLines($response,$arguments,[Text.UTF8Encoding]::new($false))
    & $dotnet $compiler /noconfig ('@' + $response)
    if($LASTEXITCODE -ne 0) { throw "$suite test compilation failed." }
    $config=@{runtimeOptions=@{tfm=$framework;framework=@{name='Microsoft.NETCore.App';version=$pack.Name}}} | ConvertTo-Json -Depth 4
    [IO.File]::WriteAllText([IO.Path]::ChangeExtension($assembly,'runtimeconfig.json'),$config,[Text.UTF8Encoding]::new($false))
    & $dotnet $assembly
    if($LASTEXITCODE -ne 0) { throw "$suite regression tests failed." }
}

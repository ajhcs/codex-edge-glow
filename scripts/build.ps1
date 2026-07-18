[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot 'src\CodexEdgeGlow\CodexEdgeGlow.csproj'
$output = Join-Path $repoRoot "artifacts\bin\$Configuration"
New-Item -ItemType Directory -Path $output -Force | Out-Null

$msbuild = Get-Command msbuild.exe -ErrorAction SilentlyContinue
if (-not $msbuild) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (Test-Path $vswhere) {
        $candidate = & $vswhere -latest -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
        if ($candidate) { $msbuild = Get-Item $candidate }
    }
}

if ($msbuild) {
    & $msbuild.FullName $project /restore /m /p:Configuration=$Configuration /p:Platform=AnyCPU
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
} else {
    $compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
    if (-not (Test-Path $compiler)) { throw 'MSBuild or the .NET Framework C# compiler is required.' }
    $sources = Get-ChildItem (Join-Path $repoRoot 'src\CodexEdgeGlow') -Recurse -Filter '*.cs' | ForEach-Object FullName
    $references = @(
        'System.dll', 'System.Core.dll', 'System.Drawing.dll', 'System.Windows.Forms.dll',
        'System.Xml.dll', 'System.Xaml.dll', 'System.Web.Extensions.dll'
    ) | ForEach-Object { "/reference:$_" }
    $frameworkAssemblies = @('WindowsBase.dll', 'PresentationCore.dll', 'PresentationFramework.dll')
    foreach ($assembly in $frameworkAssemblies) {
        $match = Get-ChildItem "$env:WINDIR\Microsoft.NET\assembly" -Recurse -Filter $assembly -ErrorAction SilentlyContinue | Select-Object -First 1
        if (-not $match) { throw "Could not locate $assembly." }
        $references += "/reference:$($match.FullName)"
    }
    $optimize = if ($Configuration -eq 'Release') { '/optimize+' } else { '/debug+' }
    & $compiler /nologo /target:winexe $optimize "/win32icon:$repoRoot\assets\logo.ico" "/out:$output\codex-edge-glow.exe" $references $sources
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

$binary = Join-Path $output 'codex-edge-glow.exe'
if (-not (Test-Path $binary)) { throw "Build completed without producing $binary." }
$hash = (Get-FileHash -Algorithm SHA256 $binary).Hash
Write-Host "Built $binary"
Write-Host "SHA256 $hash"


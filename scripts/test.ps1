[CmdletBinding()]
param([switch]$CI)

$ErrorActionPreference = 'Stop'
$repoRoot = Split-Path -Parent $PSScriptRoot
& (Join-Path $PSScriptRoot 'build.ps1') -Configuration Release
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$sourceRoot = Join-Path $repoRoot 'src\CodexEdgeGlow'
$testRoot = Join-Path $repoRoot 'tests'
$testOutput = Join-Path $repoRoot 'artifacts\tests'
New-Item -ItemType Directory -Path $testOutput -Force | Out-Null
$sources = Get-ChildItem $sourceRoot -Recurse -Filter '*.cs' | ForEach-Object FullName
$references = @('System.dll','System.Core.dll','System.Drawing.dll','System.Windows.Forms.dll','System.Xml.dll','System.Xaml.dll','System.Web.Extensions.dll') | ForEach-Object { "/reference:$_" }
foreach ($assembly in @('WindowsBase.dll','PresentationCore.dll','PresentationFramework.dll')) {
    $match = Get-ChildItem "$env:WINDIR\Microsoft.NET\assembly" -Recurse -Filter $assembly -ErrorAction Stop | Select-Object -First 1
    $references += "/reference:$($match.FullName)"
}

function Build-Harness([string]$name, [string]$main, [string]$file) {
    & $compiler /nologo /target:exe /optimize+ "/out:$testOutput\$name.exe" $references "/main:$main" $sources (Join-Path $testRoot $file)
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Build-Harness 'visual-regression' 'CodexEdgeGlow.VisualRegressionHarness' 'VisualRegressionHarness.cs'
& "$testOutput\visual-regression.exe"
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

if (-not $CI) {
    Build-Harness 'live-preview-stress' 'CodexEdgeGlow.LivePreviewStressHarness' 'LivePreviewStressHarness.cs'
    & "$testOutput\live-preview-stress.exe"
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
    & (Join-Path $testRoot 'DpiPreviewRegression.ps1') -Executable (Join-Path $repoRoot 'artifacts\bin\Release\codex-edge-glow.exe')
    if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
}

Write-Host 'All requested checks passed.'


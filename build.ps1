$ErrorActionPreference = "Stop"
$source = Join-Path $PSScriptRoot "native\LegacyBridge.cs"
$hostOutput = Join-Path $PSScriptRoot "native\LegacyBridgeHost.exe"
$viewerOutput = Join-Path $PSScriptRoot "native\LegacyBridgeViewer.exe"
$compilerCandidates = @(
  (Join-Path $env:WINDIR "Microsoft.NET\Framework64\v4.0.30319\csc.exe"),
  (Join-Path $env:WINDIR "Microsoft.NET\Framework\v4.0.30319\csc.exe")
)
$compiler = $compilerCandidates | Where-Object { Test-Path $_ } | Select-Object -First 1
if (-not $compiler) { throw "Could not find the Windows .NET Framework compiler." }
$references = @("/reference:System.Windows.Forms.dll", "/reference:System.Drawing.dll", "/reference:System.Web.Extensions.dll")
& $compiler /nologo /target:exe /out:$hostOutput @references $source
if ($LASTEXITCODE -ne 0) { throw "Host compilation failed with exit code $LASTEXITCODE." }
& $compiler /nologo /target:winexe /platform:x86 /out:$viewerOutput @references $source
if ($LASTEXITCODE -ne 0) { throw "Viewer compilation failed with exit code $LASTEXITCODE." }
Write-Host "Host and viewer compiled to $PSScriptRoot\native"

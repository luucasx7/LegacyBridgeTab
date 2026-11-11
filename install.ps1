$ErrorActionPreference = "Stop"
$ExtensionId = "hdikmfkbeihgblnheodfgkglddnohfaj"
if ($ExtensionId -notmatch '^[a-p]{32}$') {
  throw "Invalid fixed extension ID. Check manifest.json."
}
$sourceHost = Join-Path $PSScriptRoot "native\LegacyBridgeHost.exe"
$sourceViewer = Join-Path $PSScriptRoot "native\LegacyBridgeViewer.exe"
if (-not (Test-Path $sourceHost) -or -not (Test-Path $sourceViewer)) {
  throw "The helper programs have not been built yet. Run build.ps1 first."
}
$installDir = Join-Path $env:LOCALAPPDATA "Programs\LegacyBridge"
New-Item -ItemType Directory -Force -Path $installDir | Out-Null
$hostPath = Join-Path $installDir "LegacyBridgeHost.exe"
$viewerPath = Join-Path $installDir "LegacyBridgeViewer.exe"
Copy-Item -Force $sourceHost $hostPath
Copy-Item -Force $sourceViewer $viewerPath
$manifestPath = Join-Path $installDir "com.codex.legacybridge.json"
$manifest = @{
  name = "com.codex.legacybridge"
  description = "Legacy Bridge native messaging host"
  path = $hostPath
  type = "stdio"
  allowed_origins = @("chrome-extension://$ExtensionId/")
} | ConvertTo-Json -Depth 4
$utf8NoBom = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($manifestPath, $manifest, $utf8NoBom)
$registryPath = "HKCU:\Software\Google\Chrome\NativeMessagingHosts\com.codex.legacybridge"
New-Item -Force -Path $registryPath | Out-Null
Set-Item -Path $registryPath -Value $manifestPath
$featurePath = "HKCU:\Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION"
New-Item -Force -Path $featurePath | Out-Null
New-ItemProperty -Path $featurePath -Name "LegacyBridgeViewer.exe" -PropertyType DWord -Value 11001 -Force | Out-Null
Write-Host "Installation completed for extension $ExtensionId"
Write-Host "Restart Chrome to load the helper program."

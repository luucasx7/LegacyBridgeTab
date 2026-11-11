$ErrorActionPreference = "Stop"
$registryPath = "HKCU:\Software\Google\Chrome\NativeMessagingHosts\com.codex.legacybridge"
if (Test-Path $registryPath) { Remove-Item -Path $registryPath -Recurse -Force }
$featurePath = "HKCU:\Software\Microsoft\Internet Explorer\Main\FeatureControl\FEATURE_BROWSER_EMULATION"
if (Test-Path $featurePath) {
  Remove-ItemProperty -Path $featurePath -Name "LegacyBridgeViewer.exe" -ErrorAction SilentlyContinue
}
$installDir = Join-Path $env:LOCALAPPDATA "Programs\LegacyBridge"
if (Test-Path $installDir) { Remove-Item -Path $installDir -Recurse -Force }
Write-Host "Legacy Bridge removido."

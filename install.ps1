$ErrorActionPreference = "Stop"
$ExtensionId = "hdikmfkbeihgblnheodfgkglddnohfaj"
if ($ExtensionId -notmatch '^[a-p]{32}$') {
  throw "ID fixo da extensão inválido. Verifique o manifest.json."
}
$sourceHost = Join-Path $PSScriptRoot "native\LegacyBridgeHost.exe"
$sourceViewer = Join-Path $PSScriptRoot "native\LegacyBridgeViewer.exe"
if (-not (Test-Path $sourceHost) -or -not (Test-Path $sourceViewer)) {
  throw "Os auxiliares ainda não foram compilados. Execute build.ps1 primeiro."
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
Write-Host "Instalação concluída para a extensão $ExtensionId"
Write-Host "Reinicie o Chrome para carregar o programa auxiliar."

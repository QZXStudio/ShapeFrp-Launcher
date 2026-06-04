# ============================================================
# QZXFrp Windows 一键构建（EXE + MSI）
# 零外部依赖，dotnet build 全自动。
# 输出: QZXFrp.Bootstrapper\bin\Release\QZXFrp.Bootstrapper.exe
#       QZXFrp.Setup\bin\Release\QZXFrp.Setup.msi
# ============================================================
$ErrorActionPreference = "Stop"
$Root = "$PSScriptRoot"

Write-Host "=== QZXFrp 构建 ===" -ForegroundColor Cyan

dotnet build "$Root\QZXFrp.Bootstrapper\QZXFrp.Bootstrapper.wixproj" -c Release
if ($LASTEXITCODE -ne 0) { exit 1 }

$exe = "$Root\QZXFrp.Bootstrapper\bin\Release\QZXFrp.Bootstrapper.exe"
$msi = "$Root\QZXFrp.Setup\bin\Release\QZXFrp.Setup.msi"
if (Test-Path $exe) { $s = [math]::Round((Get-Item $exe).Length/1MB,1); Write-Host "`n📦 EXE: $exe ($s MB)" -ForegroundColor White }
if (Test-Path $msi) { $s = [math]::Round((Get-Item $msi).Length/1MB,1); Write-Host "📦 MSI: $msi ($s MB)" -ForegroundColor White }

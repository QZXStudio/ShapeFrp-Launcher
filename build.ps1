# ============================================================
# QZXFrp Windows 构建 (MSI + EXE)
# ============================================================
$ErrorActionPreference = "Stop"
$Root = "$PSScriptRoot"

Write-Host "=== QZXFrp 构建 ===" -ForegroundColor Cyan

# 1. 先构建 MSI
Write-Host "`n[1/2] 构建 MSI ..." -ForegroundColor Green
dotnet build "$Root\QZXFrp.Setup\QZXFrp.Setup.wixproj" -c Release
if ($LASTEXITCODE -ne 0) { throw "MSI 构建失败" }

# 2. 再构建 EXE
Write-Host "`n[2/2] 构建 EXE ..." -ForegroundColor Green
dotnet build "$Root\QZXFrp.Bootstrapper\QZXFrp.Bootstrapper.wixproj" -c Release -p:MsiPath=""
if ($LASTEXITCODE -ne 0) { throw "EXE 构建失败" }

$exe = "$Root\QZXFrp.Bootstrapper\bin\Release\QZXFrp.Bootstrapper.exe"
$msi = "$Root\QZXFrp.Setup\bin\Release\QZXFrp.Setup.msi"
Write-Host "`n📦 产物:" -ForegroundColor Cyan
if (Test-Path $exe) { Write-Host "  EXE: $exe ($([math]::Round((Get-Item $exe).Length/1MB,1)) MB)" }
if (Test-Path $msi) { Write-Host "  MSI: $msi ($([math]::Round((Get-Item $msi).Length/1MB,1)) MB)" }

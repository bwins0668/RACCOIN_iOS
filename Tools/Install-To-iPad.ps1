# ============================================
# RACCOIN iOS - iPad 安装脚本 (Windows)
# ============================================
# 前置要求:
#   1. 安装 iTunes (包含 Apple 驱动)
#   2. 下载 libimobiledevice: https://github.com/libimobiledevice-win32/imobiledevice-net/releases
#   3. 或使用 Sideloadly: https://sideloadly.io/
# ============================================

param(
    [string]$IpaPath = "",
    [switch]$UseSideloadly
)

Write-Host "=== RACCOIN iOS - iPad 安装工具 ===" -ForegroundColor Cyan

# 检查 iPad 连接
Write-Host "`n[1/4] 检查 iPad 连接..." -ForegroundColor Yellow

$ideviceId = Get-Command idevice_id -ErrorAction SilentlyContinue
if ($ideviceId) {
    $devices = & idevice_id -l 2>$null
    if ($devices) {
        Write-Host "  已连接设备: $devices" -ForegroundColor Green
        & ideviceinfo -k DeviceName 2>$null | ForEach-Object { Write-Host "  设备名称: $_" }
        & ideviceinfo -k ProductType 2>$null | ForEach-Object { Write-Host "  设备型号: $_" }
    } else {
        Write-Host "  未检测到 iPad，请确保:" -ForegroundColor Red
        Write-Host "    - iPad 已通过 USB 连接"
        Write-Host "    - 已在 iPad 上信任此电脑"
        Write-Host "    - iTunes 已安装"
        exit 1
    }
} else {
    Write-Host "  未找到 idevice_id 工具" -ForegroundColor Yellow
    Write-Host "  请下载 libimobiledevice: https://github.com/libimobiledevice-win32/imobiledevice-net/releases"
    Write-Host "  或使用 Sideloadly GUI: https://sideloadly.io/"
}

# 查找 IPA 文件
Write-Host "`n[2/4] 查找 IPA 文件..." -ForegroundColor Yellow

if (-not $IpaPath) {
    # 尝试从 GitHub Actions 下载目录查找
    $downloadDir = "$env:USERPROFILE\Downloads"
    $ipaFiles = Get-ChildItem $downloadDir -Filter "*.ipa" -ErrorAction SilentlyContinue | Sort-Object LastWriteTime -Descending
    
    if ($ipaFiles) {
        Write-Host "  找到以下 IPA 文件:" -ForegroundColor Green
        for ($i = 0; $i -lt $ipaFiles.Count; $i++) {
            Write-Host "    [$i] $($ipaFiles[$i].Name) ($([math]::Round($ipaFiles[$i].Length/1MB, 2)) MB)"
        }
        
        if ($ipaFiles.Count -eq 1) {
            $IpaPath = $ipaFiles[0].FullName
        } else {
            $choice = Read-Host "  选择要安装的 IPA (输入编号)"
            $IpaPath = $ipaFiles[[int]$choice].FullName
        }
    } else {
        Write-Host "  未找到 IPA 文件" -ForegroundColor Red
        Write-Host "  请先从 GitHub Actions 下载构建产物:"
        Write-Host "    1. 打开 https://github.com/YOUR_USERNAME/RACCOIN_iOS/actions"
        Write-Host "    2. 点击最新的成功构建"
        Write-Host "    3. 下载 RACCOIN-iOS-IPA 工件"
        exit 1
    }
}

if (-not (Test-Path $IpaPath)) {
    Write-Host "  IPA 文件不存在: $IpaPath" -ForegroundColor Red
    exit 1
}

Write-Host "  使用 IPA: $IpaPath" -ForegroundColor Green

# 安装方式选择
Write-Host "`n[3/4] 选择安装方式..." -ForegroundColor Yellow

if ($UseSideloadly) {
    Write-Host "  使用 Sideloadly 安装..." -ForegroundColor Cyan
    Write-Host "  请手动操作:"
    Write-Host "    1. 打开 Sideloadly (https://sideloadly.io/)"
    Write-Host "    2. 拖入 IPA 文件: $IpaPath"
    Write-Host "    3. 输入 Apple ID"
    Write-Host "    4. 点击 Start"
} else {
    # 使用 ideviceinstaller
    $installer = Get-Command ideviceinstaller -ErrorAction SilentlyContinue
    if ($installer) {
        Write-Host "  使用 ideviceinstaller 安装..." -ForegroundColor Cyan
        & ideviceinstaller -i $IpaPath
        if ($LASTEXITCODE -eq 0) {
            Write-Host "  安装成功！" -ForegroundColor Green
        } else {
            Write-Host "  安装失败，错误代码: $LASTEXITCODE" -ForegroundColor Red
        }
    } else {
        Write-Host "  未找到 ideviceinstaller" -ForegroundColor Yellow
        Write-Host ""
        Write-Host "  === 推荐安装方式 ===" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "  方式 1: Sideloadly (最简单)"
        Write-Host "    下载: https://sideloadly.io/"
        Write-Host "    直接拖入 IPA 即可安装"
        Write-Host ""
        Write-Host "  方式 2: AltStore"
        Write-Host "    下载: https://altstore.io/"
        Write-Host "    支持自动刷新签名"
        Write-Host ""
        Write-Host "  方式 3: iMazing (付费但功能强大)"
        Write-Host "    下载: https://imazing.com/"
    }
}

# 启动应用
Write-Host "`n[4/4] 完成!" -ForegroundColor Green
Write-Host "  在 iPad 上找到 RACCOIN 图标启动游戏" -ForegroundColor Cyan
Write-Host ""
Write-Host "  如果应用闪退，可能是签名问题:"
Write-Host "    - 免费 Apple ID 签名有效期 7 天"
Write-Host "    - 7 天后需要重新安装"
Write-Host "    - 付费开发者账号 ($99/年) 签名有效期 1 年"

# ============================================
# RACCOIN iOS - GitHub Secrets 准备脚本
# 运行此脚本生成需要添加到 GitHub 的 Secrets
# ============================================

Write-Host "=== RACCOIN iOS - GitHub Secrets 准备工具 ===" -ForegroundColor Cyan

$certDir = "C:\Users\lvgua\Desktop\iPad_certs_extracted"

# 1. 编码 P12 证书
$p12File = Get-ChildItem $certDir -Filter "*.p12" | Select-Object -First 1
if ($p12File) {
    $p12Base64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($p12File.FullName))
    Write-Host "`n[IOS_P12_BASE64] 已生成 (长度: $($p12Base64.Length))" -ForegroundColor Green
    $p12Base64 | Set-Clipboard
    Write-Host "  已复制到剪贴板，请粘贴到 GitHub Secret: IOS_P12_BASE64" -ForegroundColor Yellow
    Write-Host "  按任意键继续..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}

# 2. 编码 Provisioning Profile
$ppFile = Get-ChildItem $certDir -Filter "*.mobileprovision" | Select-Object -First 1
if ($ppFile) {
    $ppBase64 = [Convert]::ToBase64String([IO.File]::ReadAllBytes($ppFile.FullName))
    Write-Host "`n[IOS_PROVISION_PROFILE_BASE64] 已生成 (长度: $($ppBase64.Length))" -ForegroundColor Green
    $ppBase64 | Set-Clipboard
    Write-Host "  已复制到剪贴板，请粘贴到 GitHub Secret: IOS_PROVISION_PROFILE_BASE64" -ForegroundColor Yellow
    Write-Host "  按任意键继续..."
    $null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
}

# 3. P12 密码
Write-Host "`n[IOS_P12_PASSWORD] 密码为: 1" -ForegroundColor Green
Write-Host "  请手动添加到 GitHub Secret: IOS_P12_PASSWORD" -ForegroundColor Yellow

# 4. Unity 许可证说明
Write-Host @"

=== Unity 许可证设置 ===
对于公开仓库，可以使用 Unity Personal 许可证：

方法 1: 使用 UNITY_LICENSE 环境变量
  1. 在本地 Unity Hub 激活 Unity 6000.3.0f1
  2. 找到许可证文件:
     Windows: C:\ProgramData\Unity\Unity_lic.ulf
  3. 将文件内容编码为 Base64 并添加到 Secret: UNITY_LICENSE

方法 2: 使用 UNITY_EMAIL + UNITY_PASSWORD
  直接添加你的 Unity 账号邮箱和密码到 Secrets

=== 需要添加的 GitHub Secrets 列表 ===
  IOS_P12_BASE64            - P12 证书 Base64
  IOS_P12_PASSWORD          - P12 密码 (值为: 1)
  IOS_PROVISION_PROFILE_BASE64 - 描述文件 Base64
  UNITY_LICENSE             - Unity 许可证 (或下面两个)
  UNITY_EMAIL               - Unity 账号邮箱
  UNITY_PASSWORD            - Unity 账号密码

"@ -ForegroundColor Cyan

Write-Host "完成！" -ForegroundColor Green

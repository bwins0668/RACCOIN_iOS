# RACCOIN iOS - GitHub Actions 自动化构建指南

## 快速开始

### 第一步：创建 GitHub 仓库

1. 打开 https://github.com/new
2. 仓库名: `RACCOIN_iOS` (或任意名称)
3. 选择 **Public** (公开仓库免费无限制)
4. **不要** 勾选 README/gitignore/license
5. 点击 Create repository

### 第二步：推送代码

```powershell
cd "G:\迅雷云盘\RACCOIN_iOS"
git branch -m main
git commit -m "Initial commit: RACCOIN iOS port"
git remote add origin https://github.com/你的用户名/RACCOIN_iOS.git
git push -u origin main
```

### 第三步：配置 GitHub Secrets

进入仓库 → Settings → Secrets and variables → Actions → New repository secret

需要添加以下 Secrets:

| Secret 名称 | 值 | 说明 |
|-------------|-----|------|
| `IOS_P12_BASE64` | (运行 Tools/Prepare-Secrets.ps1 获取) | P12 证书 Base64 |
| `IOS_P12_PASSWORD` | `1` | 证书密码 |
| `IOS_PROVISION_PROFILE_BASE64` | (运行 Tools/Prepare-Secrets.ps1 获取) | 描述文件 Base64 |
| `UNITY_EMAIL` | 你的 Unity 账号邮箱 | Unity 登录 |
| `UNITY_PASSWORD` | 你的 Unity 账号密码 | Unity 登录 |

**获取证书 Base64:**
```powershell
# 运行准备脚本 (会自动复制到剪贴板)
.\Tools\Prepare-Secrets.ps1
```

### 第四步：触发构建

1. 进入仓库 → Actions 标签页
2. 选择 "RACCOIN iOS Build" 工作流
3. 点击 "Run workflow" → 选择 branch: main → 点击绿色按钮

### 第五步：下载并安装

构建完成后 (约 30-60 分钟):

1. 进入 Actions → 点击成功的构建
2. 下载 `RACCOIN-iOS-IPA` 工件
3. 解压得到 `.ipa` 文件
4. 运行安装脚本:
```powershell
.\Tools\Install-To-iPad.ps1
```

或手动使用 [Sideloadly](https://sideloadly.io/) 安装。

---

## 工作流程说明

```
┌─────────────────────────────────────────────────────────────┐
│                    GitHub Actions 流程                       │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  push/手动触发                                              │
│       │                                                     │
│       ▼                                                     │
│  ┌─────────────────┐                                        │
│  │  build-ios      │  Ubuntu + Unity Docker                 │
│  │  (15-30 min)    │  输出: Xcode 项目                      │
│  └────────┬────────┘                                        │
│           │                                                 │
│           ▼                                                 │
│  ┌─────────────────┐                                        │
│  │ sign-and-export │  macOS + Xcode                         │
│  │  (10-20 min)    │  输出: 签名 IPA                        │
│  └────────┬────────┘                                        │
│           │                                                 │
│           ▼                                                 │
│  ┌─────────────────┐                                        │
│  │    release      │  仅 tag 推送时                         │
│  │  (创建 Release) │  输出: GitHub Release                  │
│  └─────────────────┘                                        │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 常见问题

### Q: Unity 许可证错误
A: 公开仓库可以使用免费的 Unity Personal 许可证。确保 UNITY_EMAIL 和 UNITY_PASSWORD 正确。

### Q: 代码签名失败
A: 检查:
- P12 证书是否过期
- Provisioning Profile 是否包含你的 iPad UDID
- Team ID 是否正确 (WB5752S5M6)

### Q: 构建超时
A: Unity iOS 构建较慢，首次可能需要 45+ 分钟。后续构建有缓存会快很多。

### Q: IPA 安装后闪退
A: 免费 Apple ID 签名的应用 7 天后过期，需要重新安装。

---

## 本地工具

| 工具 | 用途 | 下载 |
|------|------|------|
| Sideloadly | 安装 IPA 到 iPad | https://sideloadly.io/ |
| AltStore | 自动刷新签名 | https://altstore.io/ |
| iMazing | 高级设备管理 | https://imazing.com/ |
| libimobiledevice | 命令行工具 | https://github.com/libimobiledevice-win32 |

---

## 文件结构

```
RACCOIN_iOS/
├── .github/workflows/
│   └── build-ios.yml          # GitHub Actions 工作流
├── Assets/
│   ├── Editor/
│   │   └── BuildScript.cs     # Unity 构建脚本
│   ├── Scenes/                # 6 个游戏场景
│   ├── Scripts/               # 26 个 C# 脚本 (~7700 行)
│   └── Resources/             # 资源目录
├── Packages/
│   └── manifest.json          # Unity 包配置
├── ProjectSettings/           # Unity 项目设置
└── Tools/
    ├── Prepare-Secrets.ps1    # 证书准备脚本
    └── Install-To-iPad.ps1    # iPad 安装脚本
```

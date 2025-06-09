# GameFrameX.ChangedLog

一个功能强大的自动化变更日志生成工具，支持基于Git标签生成结构化的变更日志文档。

[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/download)
[![License](https://img.shields.io/badge/license-MIT-green.svg)](LICENSE)
[![Docker](https://img.shields.io/badge/docker-supported-blue.svg)](https://hub.docker.com/)

## 🚀 功能特性

- 🏷️ **基于Git标签自动生成变更日志** - 自动识别Git标签并生成对应版本的变更记录
- 📝 **智能提交分组** - 根据提交消息中的`[类型]`标记自动分组（如`[feat]`、`[fix]`、`[docs]`等）
- 🔗 **自动生成提交链接** - 为每个提交生成GitHub链接，方便查看详细信息
- 📅 **时间顺序排列** - 按时间倒序排列版本，最新版本在前
- 🔄 **提交合并** - 相同消息内容的提交会自动合并，避免重复记录
- ⚙️ **灵活配置** - 支持自定义输出路径和仓库路径
- 🐳 **Docker支持** - 提供完整的Docker化解决方案
- 🌍 **环境变量配置** - 支持通过环境变量进行配置

## 📋 系统要求

- .NET 8.0 或更高版本
- Git（用于访问仓库历史）
- Docker（可选，用于容器化部署）

## 🛠️ 安装与使用

### 方式一：本地运行

#### 1. 克隆项目
```bash
git clone https://github.com/gameframex/GameFrameX.ChangedLog.git
cd GameFrameX.ChangedLog
```

#### 2. 构建项目
```bash
# 使用解决方案文件构建
dotnet build GameFrameX.ChangedLog.sln

# 或直接构建项目
dotnet build src/ChangelogGenerator.csproj
```

#### 3. 运行程序
```bash
# 使用默认设置（当前目录作为仓库，输出到CHANGELOG.md）
dotnet run --project src/ChangelogGenerator.csproj

# 指定仓库路径和输出文件
dotnet run --project src/ChangelogGenerator.csproj -- --repository /path/to/your/repo --output /path/to/output/CHANGELOG.md
```

### 方式二：Docker运行

#### 1. 使用Docker Compose（推荐）
```bash
# 修改docker-compose.yml中的仓库路径
# 然后运行
docker-compose up
```

#### 2. 手动构建和运行Docker镜像
```bash
# 构建镜像
docker build -f src/Dockerfile -t changelog-generator .

# 运行容器
docker run -v /path/to/your/repo:/app/repository -v ./output:/app/output changelog-generator
```

### 方式三：作为GitHub Action使用

在您的仓库中创建`.github/workflows/changelog.yml`文件：

```yaml
name: Generate Changelog

on:
  push:
    tags:
      - 'v*'

jobs:
  changelog:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Generate Changelog
        run: |
          git clone https://github.com/gameframex/GameFrameX.ChangedLog.git changelog-tool
          cd changelog-tool
          dotnet run --project src/ChangelogGenerator.csproj -- --repository ${{ github.workspace }} --output ${{ github.workspace }}/CHANGELOG.md
      
      - name: Commit Changelog
        run: |
          git config --local user.email "action@github.com"
          git config --local user.name "GitHub Action"
          git add CHANGELOG.md
          git commit -m "Update CHANGELOG.md for ${{ github.ref_name }}" || exit 0
          git push
```

## ⚙️ 配置选项

### 命令行参数

| 参数 | 描述 | 默认值 |
|------|------|--------|
| `--repository` | Git仓库路径 | `./repository` |
| `--output` | 输出文件路径 | `CHANGELOG.md` |

### 环境变量

| 环境变量 | 描述 | 默认值 |
|----------|------|--------|
| `CHANGELOG_REPOSITORY_PATH` | Git仓库路径 | `.` |
| `CHANGELOG_OUTPUT_PATH` | 输出文件路径 | `CHANGELOG.md` |

> 注意：环境变量的优先级低于命令行参数

## 📝 提交消息格式

为了获得最佳的变更日志效果，建议使用以下提交消息格式：

```
[类型] 简短描述

详细描述（可选）
```

### 支持的类型标记

- `[feat]` - 新功能
- `[fix]` - 错误修复
- `[docs]` - 文档更新
- `[style]` - 代码格式调整
- `[refactor]` - 代码重构
- `[test]` - 测试相关
- `[chore]` - 构建过程或辅助工具的变动

### 示例

```bash
git commit -m "[feat] 添加用户认证功能"
git commit -m "[fix] 修复登录页面样式问题"
git commit -m "[docs] 更新API文档"
```

## 📁 项目结构

```
GameFrameX.ChangedLog/
├── src/                          # 源代码目录
│   ├── Models/                   # 数据模型
│   │   ├── CommitInfo.cs        # 提交信息模型
│   │   └── GitTag.cs            # Git标签模型
│   ├── Services/                # 服务类
│   │   ├── GitService.cs        # Git操作服务
│   │   └── ChangelogService.cs  # 变更日志生成服务
│   ├── Program.cs               # 程序入口
│   ├── ChangelogGenerator.csproj # 项目文件
│   └── Dockerfile               # Docker构建文件
├── tests/                       # 测试目录
├── .github/workflows/           # GitHub Actions工作流
├── docker-compose.yml           # Docker Compose配置
├── GameFrameX.ChangedLog.sln    # 解决方案文件
└── README.md                    # 项目说明文档
```

## 🧪 运行测试

```bash
# 运行所有测试
dotnet test

# 运行特定测试项目
dotnet test tests/ChangelogGenerator.Tests.csproj

# 运行测试并生成覆盖率报告
dotnet test --collect:"XPlat Code Coverage"
```

## 📄 生成的变更日志示例

```markdown
# Changelog

## [v2.1.0] - 2024-01-15

### feat
- 添加用户认证功能 ([a1b2c3d](../../commit/a1b2c3d))
- 支持多语言界面 ([e4f5g6h](../../commit/e4f5g6h))

### fix
- 修复登录页面样式问题 ([i7j8k9l](../../commit/i7j8k9l))
- 解决数据库连接超时问题 ([m0n1o2p](../../commit/m0n1o2p))

### docs
- 更新API文档 ([q3r4s5t](../../commit/q3r4s5t))

## [v2.0.0] - 2024-01-01

### feat
- 重构核心架构 ([u6v7w8x](../../commit/u6v7w8x))

* *This Changelog was automatically generated by [GameFrameX.ChangedLog](https://github.com/gameframex/GameFrameX.ChangedLog)*
```

## 🤝 贡献指南

我们欢迎任何形式的贡献！请遵循以下步骤：

1. Fork 本仓库
2. 创建您的特性分支 (`git checkout -b feature/AmazingFeature`)
3. 提交您的更改 (`git commit -m '[feat] 添加某个很棒的功能'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 打开一个 Pull Request

## 📜 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。

## 🔗 相关链接

- [GameFrameX 框架](https://github.com/gameframex)
- [问题反馈](https://github.com/gameframex/GameFrameX.ChangedLog/issues)
- [功能请求](https://github.com/gameframex/GameFrameX.ChangedLog/issues/new?template=feature_request.md)

## 📞 支持

如果您在使用过程中遇到任何问题，请通过以下方式获取帮助：

- 📋 [提交Issue](https://github.com/gameframex/GameFrameX.ChangedLog/issues)
- 💬 [讨论区](https://github.com/gameframex/GameFrameX.ChangedLog/discussions)

---

⭐ 如果这个项目对您有帮助，请给我们一个星标！

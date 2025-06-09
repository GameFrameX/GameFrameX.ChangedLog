# Changelog Generator

一个用于自动生成基于Git标签的变更日志的GitHub Action工具。

## 功能特性

- 🏷️ 基于Git标签自动生成变更日志
- 📝 支持按提交类型分组（功能、修复、文档等）
- 🔗 自动生成提交链接
- 📅 按时间顺序排列版本
- 🚀 支持未发布更改检测
- ⚙️ 可配置输出路径和仓库路径

## 使用方法

### 作为GitHub Action使用

该工具会在推送新标签时自动运行，生成或更新 `Changelog.md` 文件。

### 本地使用

```bash
# 构建项目
dotnet build src/ChangelogGenerator.csproj

# 运行（使用默认设置）
dotnet run --project src/ChangelogGenerator.csproj

# 指定输出文件和仓库路径
dotnet run --project src/ChangelogGenerator.csproj -- --output CHANGELOG.md --repository /path/to/repo
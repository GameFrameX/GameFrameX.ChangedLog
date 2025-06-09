# 使用官方的 .NET 8 SDK 镜像作为构建环境
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# 复制项目文件
COPY src/*.csproj ./src/
RUN dotnet restore src/ChangelogGenerator.csproj

# 复制所有源代码
COPY . .

# 构建应用程序
RUN dotnet publish src/ChangelogGenerator.csproj -c Release -o out

# 使用官方的 .NET 8 运行时镜像
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app

# 安装 Git（LibGit2Sharp 需要）
RUN apt-get update && apt-get install -y git && rm -rf /var/lib/apt/lists/*

# 复制构建的应用程序
COPY --from=build /app/out .

# 设置默认环境变量
ENV CHANGELOG_REPOSITORY_PATH=/repo
ENV CHANGELOG_OUTPUT_PATH=/output/CHANGELOG.md

# 创建输出目录
RUN mkdir -p /output

# 设置入口点
ENTRYPOINT ["dotnet", "ChangelogGenerator.dll"]
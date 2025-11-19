# ========== 构建阶段 ==========
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /app

# 先复制项目文件并恢复
COPY ScrapeFlow.csproj ./
RUN dotnet restore

# 再复制所有源码
COPY . .

# 发布
RUN dotnet publish ScrapeFlow.csproj -c Release -o out

# 安装 Playwright 并下载浏览器
ENV PLAYWRIGHT_BROWSERS_PATH=/app/out/.playwright
RUN dotnet tool install --global Microsoft.Playwright.CLI
ENV PATH="${PATH}:/root/.dotnet/tools"
WORKDIR /app/out
RUN pwsh ./playwright.ps1 install chromium


# ========== 运行阶段 ==========
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

WORKDIR /app

# 安装系统依赖（略，保持不变）
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        libglib2.0-0 \
        libnss3 \
        libnspr4 \
        libatk1.0-0 \
        libatk-bridge2.0-0 \
        libx11-xcb1 \
        libxcb-dri3-0 \
        libxcomposite1 \
        libxdamage1 \
        libxext6 \
        libxfixes3 \
        libxrandr2 \
        libgbm1 \
        libgtk-3-0 \
        libasound2 \
        libosmesa6 \
        libgl1-mesa-glx \
        libgl1-mesa-dri \
        libu2f-udev \
        libvulkan1 \
        libxshmfence1 \
        libelf1 \
        udev \
        iproute2 \
        dbus-user-session \
    && rm -rf /var/lib/apt/lists/*

# 复制发布输出
COPY --from=build /app/out .

ENV PLAYWRIGHT_BROWSERS_PATH=/app/.playwright

# 启动
ENTRYPOINT ["dotnet", "ScrapeFlow.dll"]
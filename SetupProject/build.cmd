@echo off
echo Building Netwrix Auditor MCP Server Setup...

REM Clean previous builds
echo Cleaning previous builds...
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj
if exist CustomActions\bin rmdir /s /q CustomActions\bin
if exist CustomActions\obj rmdir /s /q CustomActions\obj
if exist ..\bin rmdir /s /q ..\bin
if exist ..\obj rmdir /s /q ..\obj

REM First build the main project
echo Building and publishing main project...
cd ..
dotnet restore NetwrixAuditorMCPServer.csproj
dotnet publish NetwrixAuditorMCPServer.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:PublishReadyToRun=true -p:IncludeNativeLibrariesForSelfExtract=true
if errorlevel 1 (
    echo Main project publish failed!
    cd SetupProject
    exit /b 1
)
cd SetupProject

REM Then build the custom actions project
echo Building custom actions...
cd CustomActions
dotnet restore
dotnet build -c Release
if errorlevel 1 (
    echo Custom actions build failed!
    cd ..
    exit /b 1
)
cd ..

REM Finally build the installer project
echo Building installer...
dotnet restore SetupProject.wixproj
dotnet build SetupProject.wixproj -c Release
if errorlevel 1 (
    echo Installer build failed!
    exit /b 1
)

echo.
echo Build complete!
echo MSI file is located at: bin\x64\Release\NetwrixAuditorMCPServer-Setup.msi
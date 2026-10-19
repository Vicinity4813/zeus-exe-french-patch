@echo off
rem Builds "dist\Zeus EXE French Patch.exe" with the C# compiler that ships with Windows (.NET Framework 4).
rem Compile "dist\Zeus EXE French Patch.exe" avec le compilateur C# fourni avec Windows (.NET Framework 4).
setlocal
cd /d "%~dp0"
set "CSC=%WINDIR%\Microsoft.NET\Framework\v4.0.30319\csc.exe"
if not exist "%CSC%" (
    echo .NET Framework 4 compiler not found: %CSC%
    exit /b 1
)
if not exist dist mkdir dist
set "ICON="
if exist src\patcher.ico set "ICON=/win32icon:src\patcher.ico"
"%CSC%" /nologo /target:winexe /platform:x86 /optimize+ /codepage:65001 %ICON% /win32manifest:src\app.manifest ^
    "/out:dist\Zeus EXE French Patch.exe" /r:System.Drawing.dll /r:System.Windows.Forms.dll ^
    src\*.cs
if errorlevel 1 exit /b 1
echo Built: dist\Zeus EXE French Patch.exe

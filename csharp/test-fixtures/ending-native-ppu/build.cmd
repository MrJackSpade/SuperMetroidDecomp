@echo off
rem Builds the ending PPU probe into ignored csharp/test-temp with the Visual Studio C++ tools.
rem cl is called directly so the repository's C# Directory.Build settings do not apply.
setlocal
set "vswhere=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"
for /f "usebackq delims=" %%i in (`"%vswhere%" -latest -products * -requires Microsoft.VisualStudio.Component.VC.Tools.x86.x64 -property installationPath`) do set "vs=%%i"
if not defined vs (echo Visual Studio C++ tools were not found.& exit /b 1)
call "%vs%\VC\Auxiliary\Build\vcvars64.bat" >nul || exit /b 1
pushd "%~dp0..\..\.."
if not exist csharp\test-temp\ending-native-ppu mkdir csharp\test-temp\ending-native-ppu
cl /nologo /O2 /D_CRT_SECURE_NO_WARNINGS csharp\test-fixtures\ending-native-ppu\probe.c upstream-sm\src\snes\ppu.c /Fe:csharp\test-temp\ending-native-ppu\probe.exe /Fo:csharp\test-temp\ending-native-ppu\
set "result=%ERRORLEVEL%"
popd
exit /b %result%

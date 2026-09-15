@echo off
setlocal
pushd "%~dp0..\..\.."
cl /nologo /O2 /I upstream-sm/src csharp/native/DraygonCrystalAudit/main.c upstream-sm/src/snes/cpu.c /Fe:csharp/native/DraygonCrystalAudit/audit.exe /Fo:csharp/native/DraygonCrystalAudit/
set "auditResult=%ERRORLEVEL%"
popd
exit /b %auditResult%

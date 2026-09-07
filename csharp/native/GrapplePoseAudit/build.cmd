@echo off
setlocal
pushd "%~dp0..\..\.."
cl /nologo /O2 /I upstream-sm/src csharp/native/GrapplePoseAudit/main.c upstream-sm/src/snes/cpu.c /Fe:csharp/native/GrapplePoseAudit/audit.exe /Fo:csharp/native/GrapplePoseAudit/
set "auditResult=%ERRORLEVEL%"
popd
exit /b %auditResult%

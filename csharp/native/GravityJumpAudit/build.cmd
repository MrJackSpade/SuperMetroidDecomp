@echo off
setlocal
pushd "%~dp0..\..\.."
cl /nologo /O2 /I upstream-sm/src csharp/native/GravityJumpAudit/main.c upstream-sm/src/snes/cpu.c /Fe:csharp/native/GravityJumpAudit/audit.exe /Fo:csharp/native/GravityJumpAudit/
set "auditResult=%ERRORLEVEL%"
popd
exit /b %auditResult%

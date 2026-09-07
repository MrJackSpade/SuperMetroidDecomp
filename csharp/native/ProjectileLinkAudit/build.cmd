@echo off
setlocal
pushd "%~dp0..\..\.."
cl /nologo /O2 /I upstream-sm/src csharp/native/ProjectileLinkAudit/main.c upstream-sm/src/snes/cpu.c /Fe:csharp/native/ProjectileLinkAudit/audit.exe /Fo:csharp/native/ProjectileLinkAudit/
set "auditResult=%ERRORLEVEL%"
popd
exit /b %auditResult%

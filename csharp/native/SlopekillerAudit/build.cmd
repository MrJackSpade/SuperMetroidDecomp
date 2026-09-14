@echo off
setlocal
pushd "%~dp0..\..\.."
cl /nologo /O2 /I upstream-sm/src csharp/native/SlopekillerAudit/main.c upstream-sm/src/snes/cpu.c /Fe:csharp/native/SlopekillerAudit/audit.exe /Fo:csharp/native/SlopekillerAudit/
set "auditResult=%ERRORLEVEL%"
popd
exit /b %auditResult%

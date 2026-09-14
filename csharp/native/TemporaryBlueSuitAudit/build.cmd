@echo off
setlocal
pushd "%~dp0..\..\.."
cl /nologo /O2 /I upstream-sm/src csharp/native/TemporaryBlueSuitAudit/main.c upstream-sm/src/snes/cpu.c /Fe:csharp/native/TemporaryBlueSuitAudit/audit.exe /Fo:csharp/native/TemporaryBlueSuitAudit/
set "auditResult=%ERRORLEVEL%"
popd
exit /b %auditResult%

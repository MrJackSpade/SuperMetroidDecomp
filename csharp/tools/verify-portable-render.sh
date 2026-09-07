#!/bin/sh
# Run inside the .NET SDK Linux container with the repository mounted read-only
# at /source. All build outputs remain in the disposable container's /work.
set -eu
mkdir -p /work
tar -C /source --exclude=bin --exclude=obj -cf /work/source.tar \
    csharp/Directory.Build.props \
    csharp/src/SuperMetroid.Core \
    csharp/src/SuperMetroid.Verification \
    csharp/tools/ProductionMagicNumberAudit.cs \
    'Super Metroid.smc'
tar -C /work -xf /work/source.tar
cd /work
dotnet --info
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --render-contract

# Portable rendering qualification (#321)

Run from the repository root. The private retail fixture `Super Metroid.smc` is
required. No extracted audio assets, Windows desktop, D3D assembly, or SDK shader
compiler is required by this suite.

```powershell
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --render-contract
```

For an actual Linux build and execution on a Windows host with Docker's Linux
engine running:

```powershell
docker run --rm --mount "type=bind,source=$((Get-Location).Path),target=/source,readonly" mcr.microsoft.com/dotnet/sdk:10.0.400@sha256:e1ffd2a92ae84c1291bc1b6887501f8af98e6331e7af6d4c8d37168c5e87a64c sh /source/csharp/tools/verify-portable-render.sh
```

The script stages only Core, Verification, the shared build properties, a compile-
linked audit helper, and the ROM into disposable container storage. It excludes
existing `bin` and `obj` directories. The repository mount is read-only, so neither
player saves nor Windows build outputs can be changed. `--rm` removes the container
and its temporary build files after execution; the downloaded SDK image remains
cached. There is no upload of the repository or ROM to a service.

## Recorded result — September 7, 2026

Release build and execution passed on Ubuntu 24.04.4 LTS, linux-x64, SDK 10.0.400,
MSBuild 18.9.6, runtime .NET 10.0.11, using the image digest above. The same command
also passed on Windows 10.0.26200 with .NET 10.0.11.

The gate includes all existing snapshot/reference-renderer tests at the beginning
of Verification: viewport parity (120), constructed/retail title (101/131), pause
(128), file/options (284), frontend (251), intro (275), Ceres flight (108), Ceres/
Zebes/game-over (143/101), ordinary gameplay (96), color windows (238), messages
(725), eye windows (933), room FX (252), mixed Mode 7 (48), ending (444), file maps
(502), and attract (315). The runtime overlay and 160-frame gameplay/pause slice
also pass. Ownership, immutable retention, codec rejection/round trips, generation
invalidation, and a 250-ms blocked visual consumer are exercised by this command.

These are software/capture correctness checks, not Linux GPU or audio-device tests.
They prove independent non-Windows build/execution of Core, its render contract,
and software reference renderer. They do not make the WinForms desktop portable,
prove full simulation/PCM parity across every backend, or satisfy the remaining
Windows D3D qualification gates. No Linux hardware renderer is claimed.

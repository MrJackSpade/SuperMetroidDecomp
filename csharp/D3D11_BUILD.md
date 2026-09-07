# Direct3D11 backend build and current scope

The isolated Windows assembly implements device ownership and integer compute for
solid packets, 4-bpp backgrounds, 2-bpp planes/viewports, raw OAM sprites,
fixed-color addition, signed Mode 7 projection and ordered brightness. Compound gameplay/effects, presentation, desktop
selection and scheduling are still pending.
The console verification project never substitutes the CPU reference for GPU work.

## Reproducible inputs

- .NET SDK 10.0.400, as used for current project verification.
- Vortice.Direct3D11 exactly 3.8.3, with checked-in `packages.lock.json` files.
  Use `dotnet restore csharp/src/SuperMetroid.RenderVerification --locked-mode`.
  The resolved Vortice packages are 3.8.3, Mathematics 2.1.0, and SharpGen Runtime/COM
  2.4.2-beta. The beta is an upstream transitive runtime dependency, explicitly
  recorded here rather than represented as entirely stable dependencies.
- Windows SDK directory 10.0.26100.0, x64 FXC file version 10.0.26100.8249,
  SHA256 `005EFF830845789C7EFB2831A0B41950EE6954E9BCD93BAF50DE67AD537728B2`.
  `ShaderBuild.targets` verifies the compiler bytes and compiles source before
  resource discovery with `/Ges /WX /O3 /T cs_5_0`. The bytecode is embedded in the
  assembly, not compiled at runtime or loaded from an untracked binary.
  Other serviced compiler versions fail the build and need an explicit reviewed
  tool-pin update. The SDK/compiler is not redistributed in this repository.

Run `dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --solid-smoke`
and repeat with Debug. Native error boxes are disabled and managed exceptions go
to stderr with exit code one. There is no interactive window or game launch.

## Dependency and ownership review

[Vortice 3.8.3](https://www.nuget.org/packages/Vortice.Direct3D11/3.8.3) provides
managed D3D11/DXGI COM bindings, not an application-specific native renderer DLL.
Its package source revision is `9e609cb9439c9872aa1b339f177e40ec96f77239`.
Vortice/Mathematics and SharpGen package manifests declare MIT licenses; attribution
and license text are included in `D3D11_THIRD_PARTY_NOTICES.txt` and copied to output.
Native D3D11/DXGI runtime and display-driver behavior remain Windows dependencies.

The device, immediate context, shaders and views have explicit disposal. Constructor
failure releases previously created resources. Calls on a different managed thread
are rejected before submitting context work, consistent with Microsoft's
[D3D11 threading rules](https://learn.microsoft.com/en-us/windows/win32/direct3d11/overviews-direct3d-11-render-multi-thread-intro).
This is ownership enforcement, not the still-pending simulation/render scheduling split.

Hardware mode explicitly enumerates a non-software DXGI adapter and creates the
device with that adapter. A default driver-type request was experimentally found
to select Microsoft Basic Render Driver in this environment; that route is not used.
WARP must be explicitly requested and is reported separately. No automatic fallback.

Current smoke coverage: exact RGBA for 64 solid/fade/alpha combinations on each of
RTX 3090 and WARP, plus wrong-thread rejection, in Debug and Release. These results
are functional checks, not performance gates or a claim that the game renders on GPU.

`--tile-smoke` additionally checks 108 geometry/priority/scroll/transparency/color
cases plus a focused packed-byte regression on both devices. `--compare <frame.smframe>
--device hardware|warp` compares a portable fixture on the explicitly selected device.
Failures write the packet, expected/actual/difference PNGs, first pixel, mismatch count
and bounding rectangle into a unique `csharp/test-temp/render-comparison` directory.
Unsupported layer types fail before GPU submission; there is no raster fallback.

`--obj-smoke` checks 197 exact comparisons per device in Debug and Release:
all eight size modes, varying name-table selection, zero through 128 modeled sprites,
mixed BG/OBJ priority insertion and deliberately overlapping sprites with transparent
holes. The GPU resolves the first opaque OAM pixel before filtering its BG priority,
so filtering cannot resurrect a later hidden sprite. Raw OAM bytes are uploaded;
no CPU sprite raster is supplied. These remain correctness checks, not GPU timings.

`--mode7-smoke` checks 241 exact comparisons per device in Debug and Release,
including signed matrix/offset extremes, transparent overflow, character-zero fill,
disabled background, OBJ composition and ordered-layer composition. Projection and
interleaved tile/character sampling execute on GPU from raw memory/registers. These
comparisons establish software-reference parity, not independent cartridge fidelity.
This includes 48 mixed-mode gameplay HUD/floor cases. Physical scanline clipping
keeps projection coordinates intact; the floor composites from backdrop with its
own BG/OBJ ordering, and HUD color zero is opaque. Retail scene-matrix coverage
and overlays/effects on these bands remain to be verified as the backend expands.

`--window-smoke` checks 66 exact scanline color-add comparisons on each device in
Debug and Release. Coverage includes empty and single-pixel windows at both edges,
inclusive endpoints, byte saturation, repeated overlays, fades and both Ceres haze
captures. Window endpoint/color data is uploaded, not a CPU-rendered overlay. Other
effect layer kinds and retail effect-scene coverage remain pending.

The first tile comparison caught a pinned-FXC optimization problem in dynamic byte
extraction. `/Od` passed; disassembly and shader probes showed `/O3` selecting the
wrong byte. Two explicit byte-selection steps preserve optimized compilation and
pass the archived fixture in `test-fixtures/issue-321-tile-byte-selection`.

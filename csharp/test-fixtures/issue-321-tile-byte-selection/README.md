# Packed VRAM byte-selection regression

`frame.smframe` is constructed memory from seed 32111, not ROM or player data.
SHA256: `F8DA4405D5E641553B64B862B5A5A790C2470AD95DAC454F74EE78B0D30D2BAE`.
`before.json` records the original mismatch: 14,377 pixels, first at (0,0).

With the pinned FXC and `/O3`, the original dynamic byte-extraction expression
miscompiled an inlined `row+17` access. The first tile is `$EC04`; its row address
is `$C08E`. Plane three lives at `$C09F` (byte three of the packed uint), but the
bytecode selected byte one. Instrumentation read 233 instead of 144, selecting
palette index 58 instead of 50. Disabling optimization made the original shader
pass, isolating this from memory upload and texture/readback behavior.

The implementation now selects the two byte-position bits in separate steps.
The optimized shader passes this exact preserved packet on RTX 3090 and WARP.
`TileSmokeTests.VerifyFourthPlaneByteSelection` also provides a small explicit
red-versus-blue regression independent of the random stream.

From the repository root:

```
dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --compare csharp/test-fixtures/issue-321-tile-byte-selection/frame.smframe --device hardware
dotnet run --project csharp/src/SuperMetroid.RenderVerification -c Release -- --compare csharp/test-fixtures/issue-321-tile-byte-selection/frame.smframe --device warp
```

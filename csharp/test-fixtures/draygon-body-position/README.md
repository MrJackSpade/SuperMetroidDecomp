# Draygon body positioning — #381

The player's slot 0 was preserved locally as `slot-0-named.smstate` before replay.
SHA-256: `9A06B29B7EADC0CF6E96412C4591AADFF2DFD94129FC7E11BAB79D477BAE1A9B`.
The private ROM/SRAM-bearing binary is not included in this commit.

Run from the repository root:

```
dotnet run --no-launch-profile --project csharp/src/SuperMetroid.DesktopVerification -c Release -- --draygon-position-audit
```

Before the fix, frame 0 produced body `(303,287)`, BG2 `(65024,384)`,
versus native `(64916,65275)`. The native graphics-drawn hook at `$A5:9342`
was never published to the runtime's background register mirrors.

The regression checks 600 real encounter frames against the hook's unsigned
camera/body/displacement arithmetic, then checks the accepted-NMI render capture
against the preceding frame's writes. It exercises 594 distinct body positions.
Selected rendered frames are written to `csharp/test-temp/issue-381-draygon`.
Cross-checked with pinned `upstream-sm/src/sm_a5.c` (`Draygon_Func_36`) and
`upstream-disassembly/src/bank_A5.asm` (`EnemyGraphicsDrawnHook_Draygon_SetBG2XYScroll`).

## Repeated report: moving appendages

The first fix/test incorrectly sampled the camera before movement. Expanded the
assertion to use the final draw-time camera shared by appendage OAM. This failed
at frame 0: early-hook `(64916,65275)` versus draw-time `(64918,65272)`.
The hook now runs after the enemy drawing queue, matching `$A0:884D`, rather
than during enemy AI. All 600 frames pass with the draw-time reference.

## Held grapple — #382

`--draygon-grapple-audit` uses this encounter to reach a grab and feed normal
grapple selection/fire input. Before the fix, held pose `$BC` remained inactive.
It now fires and extends. A controlled attachable endpoint then exercises the
production connection and cancellation paths, asserting that the held pose,
coordinates and Draygon ownership survive both operations. This checks the
native `$90:DDD8` admission, `$9B:B98C` held connection and `$9B:C856` cancellation;
it is not a full turret-electrocution battle replay.

## Terrain priority investigation — #385

The position audit now independently decodes the captured BG1 and BG2 bitplanes
every 30 frames, excludes OBJ/color math, and checks every pixel inside the native
body display band against the Mode-1 BG ladder in `upstream-sm/src/snes/ppu.c`:
BG1 high, BG2 high, BG1 low, BG2 low (front to back).

The 600-frame encounter produced 16,186 opaque body/terrain overlap pixels, all
correctly won by terrain, with zero compositor differences. Pinned bank A5 body
streams (for example `ExtendedTilemap_Draygon_0` at `$A5:B108`) contain low-priority
tile words, and native `$A0:96CA` copies them without changing their priority.

This is diagnostic evidence, not a fix or proof of complete scene parity. The
test uses port-captured memories and scrolls; it does not independently replay
the cartridge's tilemap production or identify which terrain scatter the player
meant. Keep #385 open pending that comparison; do not force BG2 above BG1.

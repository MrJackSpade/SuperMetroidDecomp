# Zebetite player candidate: original CPU consumer (#443)

This is an incomplete parity diagnostic, not a passing ten-missile technique test.
The native consumer executes the pinned cartridge through `RunAsmCode`; it does
not use translated C behavior as its oracle. It depends on the ROM-loading helper
in `native-release-probe.h` and the existing private MOV1 export.

## Capture

Run DebugRunner `--zebetite-player-export ROM DIRECTORY` first. Keep its room data,
metadata and traces private. Include `native-release-probe.h`, then
`native-zebetite-player-probe.h`, after `StateRecorder` in native `sm_rtl.c`.
Temporarily dispatch a four-argument native executable invocation to
`DiagnosticZebetitePlayer(rom, seed, output)` from `main`. Build Release x64 using
the project's native toolchain. Remove the temporary hooks and rebuild afterward.

Use each `isolated-{724,728,732}.movement-seed` as the seed. The consumer restores
the documented fixed frame-60 candidate metadata: camera 641, ten missiles,
999 energy, generation one, and the linked actors at physical indices 128/384.
The MOV1 seed provides Samus movement and the room collision layer. Inputs are
the same frames 60–119 as the managed exporter, with right+jump ending at frame 99.

Compare each native CSV with its corresponding isolated JSONL:

```powershell
pwsh -File compare-zebetite-player.ps1 -ManagedTrace PATH.jsonl -NativeTrace PATH.csv
```

The comparator fails on any exported field mismatch. It does not accept an
animation mismatch as a successful parity baseline.

## Observed result

All three 60-frame captures match movement, animation pose/timer, camera integer
positions, ammunition and both barrier health values. Start X=732 also matches
the exported first-projectile type/position for all frames. X=724 and X=728 fail:
at frame 96 managed code has already converted the missile into an explosion;
the CPU still has a missile. On frame 97 the CPU clears it while the managed
explosion remains. No production correction is included here.

Source inspection narrows the next investigation: `$A0:A143` marks enemy-hit
projectiles through the direction high nibble; the missile pre-instruction then
clears such a projectile. The managed ordinary-enemy resolver uses
`TryStartEnemyImpact`. Check the Zebetite-specific handoff before changing this
shared resolver. `$93:8254` and `$93:834D` draw projectiles but do not advance their
instruction streams, so the consumer's omitted draw stage does not itself explain
this particular type/lifetime discrepancy.

## Limits

The consumer omits live FX, PLMs, other actors, audio and drawing. Managed actor
omission has separately been checked for these selected observations, not for
every game subsystem. Camera subpixels, all projectile internal fields, and full
state restoration are not compared by this first consumer. It does not establish
the repeated ten-shot controller sequence, native rendered parity, or full-room
equivalence. Keep #443 open.

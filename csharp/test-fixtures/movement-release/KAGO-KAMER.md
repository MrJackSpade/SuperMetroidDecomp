# Kamer controller/carry comparison (#455, partial)

Uses the ROM and native/disassembly pins in `KAGO-KZAN.md`. Original CPU execution
runs enemy initialization, active-list construction and EnemyMain between Samus
alpha and beta, followed by animation, pose processing and timer decrement.
The C# comparison loads the same actor through the production population loader
and runs complete runtime frames. No player save slots are used.

## Matrix

288 independent 120-frame cases, 34,560 frames:

- Vertical Kamer `$D5FF` and horizontal Kamer `$D83F`.
- Starting below/beside the platform, or riding it.
- Both facings; turn, morph and unmorph patterns; input delay 0 through 11.
- Same input patterns, health 999, Morph Ball only and animation/history seed as
  `KAGO-KZAN.md`. No debug cheats or liquids; no RNG-dependent actors.
- Synthetic 144x80 room. Non-rider floor row 32, Samus (1024,491), or Y505
  when starting in ball. Rider floor row 48, Samus Y483 (ball Y497).
- Vertical platform starts at (1024,448) below-test, (1024,512) rider-test.
  Initialization `$0110/$0010` selects downward/upward one-pixel speed;
  extra properties `$0404`, parameters `$8000/$0008` select 128-pixel travel
  and eight-tick initial wait.
- Horizontal platform starts (992,488) beside-test, (1024,512) rider-test;
  initialization/extra properties zero, parameters `$0001/$2810` select rightward
  one-pixel speed and the authored sinking acceleration limit.
- Both use properties `$A800` (solid, instruction processing, offscreen updates).

Compared fields: all 17 Samus fields from `KAGO-KZAN.md`, platform fixed X/Y,
external carry X/Y whole/fraction, and the platform spritemap. Named assertions
also verify unchanged health/no knockback, first-motion timing, first horizontal
carry, and the prospective-running displacement during upward carry.

## Reproduced and corrected

After matching the fixture populations, 16,128 frames differed:

1. Vertical-shutter Initial incorrectly spent a frame selecting a wait function.
   Original `$A2:EF09` calls the selected function immediately and retains Initial
   until activation installs a moving function. Corrected shared vertical-shutter
   dispatch; the first physical motion is frame 8, not 9. Remaining mismatches: 2,388.
2. Runtime required vertical collision before the prospective-running one-pixel
   probe. Clear upward platform carry publishes no pose override, so native still
   executes that probe. Corrected admission without admitting the downward
   missing-floor override. Both-facing rider witness at frame 15: X `03FC.C000`
   or `0403.4000`, Y `01DB.0000`, carry Y `FFFF.0000`.

All 34,560 frames now match. Two independent captures have SHA-256
`DA7113BBF6B97D8C95B422A584F118A6580E8D26D2EFE48CCA02071B78151E1D`.
Accepted CSV is in `kago-kamer-native-capture.zip`. Apply the headless entrypoint
patch and rebuild the native host before capturing again; temporary hooks were
removed after capture. Console-only mode suppresses both SDL error dialog paths.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --kago-kamer-audit 'Super Metroid.smc' PATH/kago-kamer-455-v1.csv
```

Full core verification passed. Retail shutter and ocean-platform audits passed;
existing Kzan (5,760), normal contact (5,400), arm pumping (14,400), and Quick Drop
timeline (24,640) comparisons remain zero-mismatch: 84,760 compared frames total.

The old shutter audit required two unrelated stale expectation corrections:
custom Power Bomb callbacks activate shutters without consulting generic damage
vulnerability (`$A0` dispatcher / `$A2:F0B6`, `$A2:F41A`), and normal enemy death
clears its slot instead of retaining the Deleted property (`$A0:A3AF`). These
were assertion corrections, not new production behavior. Initial-pointer
expectations were also updated for the corrected vertical dispatch above.

## Not completion evidence yet

The technique reference, Kagoing revision 2906, describes passage through the
platform. These rider and side/below approaches do not yet establish a successful
airborne platform passage versus its adjacent failed timing. Simply walking off
a platform and later falling beneath it is not sufficient. #455 therefore remains
open without awaiting-player-validation; retain the remaining passage requirement
alongside these verified shared-code corrections.

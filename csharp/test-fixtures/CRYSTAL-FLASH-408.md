# Crystal Flash parity investigation (#408)

Status: incomplete technique audit. Do not label the entire issue ready for player
validation based on the timer correction below.

## Hit-immunity reset omission

Pinned bank-$90 disassembly explicitly clears SamusInvincibilityTimer at:

- $90:D66C: successful activation, followed by knockback timer/direction clears.
- $90:D6A1: tenth raise call, immediately after switching movement handlers.
- $90:D6D6: after every ammo-handler call, followed by knockback timer at $D6D9.

The third clear is outside the individual ammo handlers' mod-eight NMI checks.
It applies even when no ammunition is consumed, including the final drain that
changes phase. The first nine raise calls do not execute it. The pinned C
translation agrees at `Hdmaobj_CrystalFlash`, `SamusMoveHandler_CrystalFlashStart`
and `SamusMoveHandler_CrystalFlashMain`.

The C# translation omitted all three immunity clears and the main-loop knockback
timer clear. Existing tests started with zero hit immunity, concealing the defect.
The production implementation now performs those exact writes at those boundaries,
without changing the INI health floor or granting Crystal Flash immunity.

The synthetic production-state regression now starts with immunity 96 and proves
rejected activation preserves it, while successful activation clears it. Before
the fix the latter assertion fails (expected 0, actual 96). It injects immunity
77 across the raising phase, proves nine calls preserve it and the tenth clears
it, and injects both timers for a no-drain main frame and all thirty drain calls.
All must clear on those calls, including phase completion. Existing resource,
palette, HDMA bubble, pose/history, and finish-animation assertions also pass.

Focused command:

```
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --crystal-flash
```

This is a first-pass, mechanically explicit source correction reproduced with a
constructed fixture. It is not a cartridge-CPU trace or a complete real-room damage
comparison. Do not conflate these timers with proof of every contact-damage path.

## Remaining acceptance work

Compare actual bank-$88 Power Bomb cleanup entry and the whole sequence against
the pinned cartridge, with cheats off. Cover every resource/input threshold and
adjacent failure, exact explosion-center positioning and vertical speed, the
ten-capacity refill-after-placement route, contact damage, animation and control
ownership through completion. Capture native/managed initial states and identical
input schedules. Wiki page retrieval timed out during this pass; no unverified
wiki claim was made an expected value.

## Original-CPU cleanup admission matrix

The new `movement-release/native-crystal-flash-probe.h` executes original
$88:8B4E, including its call into $90:D5A2, for 18 boundary cases in both facings.
The native loader restores the retail cartridge bytes after the upstream harness's
startup patches. CPU/RAM reset before every case; no cheats, player save or GUI.

Cases include health 49/50/51/0; each ammo family just below ten; nonempty reserve;
whole/fractional vertical velocity; one-pixel X/Y offsets; differing X/Y subpixels;
missing Down; extra Jump; an empty owned reserve; and current Power Bombs ten with
capacity ten. That last case establishes the admission check uses current ammo;
it does NOT yet reproduce collecting the refill after placement.

Managed comparison runs the real Power Bomb animation to cleanup via
`SamusBombProjectileSystem.StepFrame`, not direct Crystal Flash initiation.
It compares pose, armed flag, both hit timers, health and all three ammo counts.
All **36** cases match. In particular, whole-pixel offsets fail, subpixel offsets
do not; failed admission retains immunity 96/knockback 5, while success clears
them and retains the Power Bomb armed flag until the later Crystal Flash handoff.

Native CSV SHA256:
`CCD507BE8423FEC78122CD95458577F21F58624510578184BF51EE25FF22A5F3`.
The comparator checks this accepted capture identity and every row. Trace remains
local at `csharp/test-temp/crystal-flash-408-native.csv`, not published.

Apply `movement-release/native-crystal-flash-entrypoint.patch` inside the pinned
`upstream-sm` checkout. Build `src/sm.vcxproj` Release/x64 with v145 and absolute
SolutionDir ending in `upstream-sm\`. Then run from repository root:

```
upstream-sm/build/bin-x64-Release/sm.exe --diagnostic-crystal-flash "Super Metroid.smc" NEW_NATIVE.csv
dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --crystal-flash-native "Super Metroid.smc" NEW_NATIVE.csv
```

The native output is exclusive-create. Temporary main/sm_rtl integration was
removed and normal native executable rebuilt after capture. This slice adds no
production fix; the prior timer correction now agrees with actual CPU activation.
Still outstanding: full input/resource boundary sweep, frame-by-frame entire
technique, real refill collection and contact-damage/animation/control comparison.

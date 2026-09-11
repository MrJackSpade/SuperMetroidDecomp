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

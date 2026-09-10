# Kraid belly-platform wall investigation (#521)

Affected player version: 0.1.1. This is an investigation, not a reproduced defect.

The pinned disassembly's `Function_KraidLint_FireLint` at A7:B89B-B906
subtracts the speed from the actor every update. Below X=56 it sets property
0400 (ignore Samus collision); below X=32 it sets invisibility, selects horizontal
realignment, sets a 300-frame timer, and schedules production again. There is no
stationary wall-lodging branch. Its final call checks Samus standing on the
platform and applies horizontal carrying displacement.

`RoomEnemySystem.KraidParts.cs` implements those flight/reset branches. The new
`KraidLintFlightAudit` observes the three real enemy records during the existing
retail-room Kraid encounter, without changing their state. It checks every firing
update's 16.16 displacement, the exact visibility/collision thresholds, continued
flight before disappearance, and reset function/timer/next-function.

Run with the project's pinned NTSC ROM:

```
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --kraid-audit "Super Metroid.smc"
```

Result: firing frames [417,345,343], wall-boundary frames [40,40,40], and five
complete disappear/reset cycles for each of the three actors. The complete
existing encounter audit also passes. Build: zero warnings/errors.

This is managed encounter evidence cross-checked against source, not an original
CPU capture or rendered comparison. The broader ticket's standability/carry and
wall appearance still need a focused comparison before the investigation is
finished. No production code changed and the ticket remains open without the
player-validation label.

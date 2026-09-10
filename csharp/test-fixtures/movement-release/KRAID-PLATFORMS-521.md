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
CPU capture or rendered comparison. The original flight-test commit changed no
production code.

## Follow-up: missing rider displacement

Inspecting the support branch exposed a separate omission: the managed firing
routine never received Samus and omitted A7:B8E2-B906 entirely. A focused fixture
loads the retail population, idles the body, removes unrelated parts, and seeds
one firing platform plus Samus at exact contact boundaries. Before the fix,
slot 2 at X=128 with relative position (-28,-32) produced extra X displacement
00000000 instead of FFFC8000 (minus 3.5 pixels).

The firing routine now receives Samus and publishes the native carry after moving
the platform. The common A0:ABE7 support helper was extracted unchanged from the
work-robot implementation and shared, rather than duplicating the collision math.
The signed whole-word clamp preserves fractional borrow and also retains native
16-bit comparison behavior. It still runs on the final hide/reset frame, just as
the cartridge does; this must not be gated on the new invisibility/property bits.

Run `--kraid-lint-contact-audit "Super Metroid.smc"` through DebugRunner.
900 cases cover all three parts, ordinary flight and both wall thresholds,
horizontal edge exclusion, biased vertical contact, fractional accumulation,
the -16-pixel whole-word clamp, and a signed-overflow comparison boundary.
These are one-frame production enemy dispatches, not full player landings or
proof that downstream movement consumes the displacement correctly.

The full Kraid encounter, work-robot audit, and core verification suite also pass.
The broader ticket remains open without player-validation status: full support/
carry movement and wall appearance still need the focused native comparison.

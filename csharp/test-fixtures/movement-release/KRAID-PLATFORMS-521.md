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
At that checkpoint the investigation stayed open for the full support/carry check.

## Original-CPU and runtime acceptance

The follow-up runs the full gameplay loop with constructed empty space over a flat
floor and the retail Kraid population. One firing lint is isolated while the body
idles. For each of the three parts, 16 neutral-input frames maintain Samus's support
height and move her exactly 3.5 pixels left each frame. The production loop therefore
consumes the carry correctly; it is not merely published to an unused field.

`native-kraid-lint-probe.h` executes original ROM routine A7:B89B through its RTL,
including the original A0:ABE7 support helper. It uses the same 900 boundary seeds.
The bounded headless harness restores original ROM bytes after host initialization;
it does not use the upstream translated C implementation as the oracle. Temporary
entrypoint hooks are reversed after capture. No GUI or player saves are involved.

Archived trace: `kraid-lint-521-v1.zip`, containing `kraid-lint-521-v1.csv`.
CSV SHA-256: `37F892136FF1B3E254BE00990E536E96B2C9E623C5AE546114F242CAF26ACB92`.
ROM SHA-256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native host pin: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Disassembly pin: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.

Pass the extracted CSV as the optional final argument to
`--kraid-lint-contact-audit ROM CSV`. The hash-gated comparison verifies all 900
records: carry, position/subposition, wall visibility/contact bits, function, and
wall reset timer/next function. All match. Existing full-flight encounter checks
still cover five natural launch/disappear/reset cycles of each platform.

Conclusion: this ROM has no stationary wall-lodging phase. The missing rider carry
is fixed and ready for player confirmation. No pixel-level native screenshot
comparison was performed; visibility is checked through the native property bits.

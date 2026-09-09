# Chainsaw firing CPU trace (#396)

Run `DiagnosticChainsawFire` before SDL startup, temporarily including
`native-release-probe.h` and `native-chainsaw-fire-probe.h` in sm_rtl.c.
Use the existing retail loader, which restores the ROM after harness patches.
Suppress explicit SDL Die/Warning dialogs during diagnostics, then restore the
hooks and rebuild the normal executable. This probe does not write saves.

The synthetic room is 16 by 16 empty blocks, one screen in each dimension, with
no interactive enemies. Samus stands at (128,128), facing right, with beam word
$000D and a newly pressed Shoot. Run native $90:B887, then eight $90:AECE updates.
The Power Bomb flag is tested at zero and $8000; this is a flag-isolation test,
not a fully spawned Power Bomb or a combat/rendering reproduction.

Both cases initialize identically:

```
count=1 cooldown=0 type=800D damage=150 dir=2 xy=139/123
pre=B0AC list=9027 radius=8/12 speed=FFC0/0000
```

With the Power Bomb flag clear, update zero deletes the shot: count/type/damage,
position, list, sprite and radii become zero; preinstruction becomes B169. The
subsequent instruction handler leaves the cleared instruction timer at FFFF.
All eight sampled updates remain cleared.

With flag $8000, all eight updates retain count 1, type 800D, damage 150, position
(139,123) and callback B0AC. The instruction timer is 1 in each sample:

| Update | Next list | Spritemap | Radius X/Y |
| --- | --- | --- | --- |
| 0 | 902F | AF4C | 8/12 |
| 1 | 9037 | AF62 | 8/12 |
| 2 | 903F | AF78 | 8/16 |
| 3 | 9047 | AFA2 | 8/16 |
| 4 | 904F | AFCC | 8/20 |
| 5 | 9057 | AFF6 | 8/20 |
| 6 | 905F | B020 | 8/23 |
| 7 | 9067 | B04A | 8/23 |

B0AC is a misaligned code entry: the preceding JSL bank byte becomes opcode
94, executing STY $60,X, then falling through to the Power Bomb callback B0AE.
Its C157 helper clears a zero-variable projectile when the Power Bomb flag is
zero. This explains the observed lifetime, rather than ordinary Wave motion.
The original samples reset Y to zero before every handler call. This masked
the register-store side effect; see the retained-Y correction below.

No enemy damage classification, rendered visibility, charged-shot behavior,
door/gate reactions, complete animation lifetime or multi-shot limit is proved
by this trace. Those remain required before declaring #396 ready for validation.

## Production reproduction

`dotnet run --project csharp/src/SuperMetroid.Verification -c Release -- --chainsaw-firing`
currently fails at the shot-allocation assertion: expected slot zero, received
null. This is a deliberate opt-in red reproduction, not part of the default
green suite until implementation is complete. It asserts admission before
deletion so the existing silent rejection cannot masquerade as correct lifetime.

The actual uncharged callback table is $90:B96E (the B9 6E B9 load in
FireUnchargedBeam); the charged table is $90:BA3E (B9 3E BA). The comments on
the pinned C static arrays name their enclosing functions B887/B986, not the
table addresses. Normal callbacks are AEF3 (non-Wave), B0E4 (uncharged Wave or
Ice/Wave), and B0C3 (other Wave/charged Wave). Index 13 of B96E overreads the
FireChargedBeam instruction bytes to obtain B0AC. Future implementation must
preserve that table behavior rather than choosing a Wave callback from bit zero.

## Retained-Y correction: cached PPU register writes

The probe now carries CPU Y from firing into HandleProjectile and between
isolated handler invocations. The earlier phrase "scratch" was incorrect:
`variables.h` maps $60 onward to cached PPU registers, uploaded by bank $80.
These are rendering state, not disposable arithmetic temporaries.

FireUnchargedBeam returns Y=$0034: SetInitialProjectileSpeed indexes the speed
row with four times beam combination 13. On the first projectile update, the
misaligned STY stores $0034 to $60/$61 even when the shot is subsequently deleted.
With Power Bomb active, successive isolated updates store $9027, $902F, $9037,
$903F, $9047, $904F, $9057: the previous instruction handler left Y pointing at
its consumed record (not the next record). Lifetimes and positions match the
earlier trace. This chained routine experiment does not establish Y at every
full gameplay-frame entry, where intervening routines can change it.

A separate direct B0AC experiment supplies Y=$1234 and each even ordinary-slot
index. Native stores $34/$12 into exactly these byte pairs:

| Slot | WRAM pair | Cached PPU registers |
| --- | --- | --- |
| 0 | $60/$61 | W12SEL / W34SEL |
| 1 | $62/$63 | WOBJSEL / WH0 |
| 2 | $64/$65 | WH1 / WH2 |
| 3 | $66/$67 | WH3 / WBGLOG |
| 4 | $68/$69 | WOBJLOG / TM |

Thus slot four can affect main-screen layer selection. A faithful port cannot
discard this store, substitute an invented constant Y, or claim visual parity
based only on the stationary projectile and damage word. Current rendering
models effect-specific windows; the cached-register write, its NMI handoff,
and relevant CPU-Y provenance still require integration and validation.

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

## Register publication and competing producers (source trace)

The pinned source establishes the following ordering; this is a source trace,
not a new full-frame CPU reproduction:

- `RunOneFrameOfGameInner` calls `HdmaObjectHandler` before dispatching the
  gameplay state. When HDMA objects are enabled, that handler runs
  `LayerBlendingHandler` after the object programs.
- A nonzero low byte of the blending configuration calls
  `InitializeLayerBlending` ($88:8075). This clears W12SEL/W34SEL/WOBJSEL,
  TMW/TSW and assigns TM/TS, but does **not** clear the window edges or logic
  registers. A frame-wide reset of every window byte would therefore be wrong.
- Gameplay's `Samus_HandleHudSpecificBehaviorAndProjs` updates the selected
  weapon and then calls `HandleProjectile`. Its misaligned callback can write
  cached registers after the blending initialization; initialization is not
  evidence that the callback's writes are invisible in that frame.
- Accepted `Vector_NMI` calls `NmiUpdateIoRegisters` ($80:91EE), which uploads
  the cached window registers and copies reg_TM to gameplay_TM. A lag NMI
  skips that upload. The ordinary gameplay IRQ restores gameplay_TM after
  the HUD's separate BG3-only designation.

Window selection and screen admission must remain independent. In particular,
the three Power Bomb blending routines at $88:8219/$88:823E/$88:8263 all set
TMW to zero, while retaining TSW=4. A changed W12SEL alone consequently does
not prove visible main-screen clipping during that configuration. Slot four's
TM overwrite is a separate effect and does not require TMW. Other effects,
subscreen composition and later producers still need their actual register
values traced; do not assume the isolated probe's Power Bomb flag establishes
an entire live HDMA configuration.

Current port integration gap: `RunNmi` latches scroll/OAM/effect snapshots,
but no literal cached window register owner is present. `CaptureOrdinaryBase`
derives main-screen layers from door/boss state and leaves the packet's new
window fields at defaults. Merely setting packet windows from the projectile
would bypass both this accepted-NMI boundary and competing producers. The
software/GPU primitive tests prove register interpretation, not this handoff.

`GameplayWindowRegisterCache` now models the contiguous shadow-byte region,
selective window/screen initialization and accepted/lagged NMI publication.
`--hardware-windows` checks all five native STY targets against literal byte
addresses, preservation of neighboring bytes, the gameplay_TM gap, high-bit
retention, and snapshot stability across later writes and lag. This is a tested
cache component, not yet the live runtime owner: the existing effect producers,
runtime NMI and capture still require integration. It does not supply CPU Y or
make the opt-in Chainsaw firing reproduction pass.

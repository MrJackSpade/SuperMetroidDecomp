# Issue 527: manual reserve input and transfer

Affected player version: **0.1.1**. This is the first implementation slice of the
reserve-refill presentation investigation, not closure of the entire report.

## Reproduced missing behavior

The equipment input dispatcher returned without handling A whenever its selected
category was reserve tanks. Down also skipped the manual transfer item. A focused
test using the real pause menu failed before the fix: changing AUTO to MANUAL
left mode at 1 instead of 2.

## Native translation

- $82:AC70 dispatches the existing tank item before handling D-pad movement.
- $82:AE8B toggles AUTO/MANUAL with A and writes the mode label.
- $82:AC8B handles reserve-item navigation, including AUTO skipping transfer.
- $82:AF4F starts manual refill on A, rounds its sound-delay word up to eight,
  then transfers the ROM-defined amount at $82:BF04 each selected-item call.
- The sound is library three $2D, maximum queue six, on transfer calls 0, 8, ...
- Moving away retains the delay but stops advancing transfer; returning resumes
  without another A press. This native quirk is intentionally retained.
- Reaching maximum health discards all remaining reserve energy, as retail does.
  Exhaustion clears both selector bytes and the transfer-delay word.

Definitions are in `PauseReserveTransferRomData`; behavior is in the scoped
`PauseMenuState.ReserveTransfer.cs` partial. The pending delay is serialized.
Older pause layouts restore with no pending transfer and an explicit warning;
all remaining field identities/order still pass through the strict graph reader.

Source comparison: pinned `upstream-disassembly/src/bank_82.asm` and the project's
retail ROM. No separate emulator playback or player confirmation is claimed.

## Tests

Run Verification in Release with `--pause-reserve-manual`, or run its full suite.
The focused test uses actual pause input, owned Charge Beam/reserve capacity, and
no host cheats. It checks AUTO/MANUAL mode changes, transfer selection, exact
health/reserve progression, transfer-before-navigation ordering, suspend/resume,
per-frame supply tile words, refill sound IDs/cadence in the actual audio queue,
retail full-health depletion, completion selection, timer serialization and
legacy field mapping. It does not claim audible PCM or rendered tank animation.

## Remaining work before issue 527 is ready

- Automatic-recovery presentation and frame ordering, separately from manual.
- Cartridge playback comparison of the reported visual/timing behavior.

Issue 527 remains open **without** awaiting-player-validation until that work is
implemented and verified. This slice restores missing controls, not full parity.

## Follow-up: rendered energy arrow

The missing arrow was reproduced by comparing the actual pause frame with the
native ten tile-palette writes and two color writes. AUTO phase zero failed
before implementation. The production path now ports $82:AD0A-$AE89: AUTO uses
the runtime's accepted-NMI byte counter modulo 32, the manual transfer item uses
solid enabled colors, and leaving tanks or completing manual refill disables the
arrow. Equipment-page entry retains $82:AC1A's nonempty-reserve solid enable.
The native tank dispatcher owns these updates; unrelated categories do not
invent another animation clock. Rendering remains side-effect free.

`--pause-reserve-arrow` (also in the full suite) compares complete rendered frames
against independently patched native palette/tile writes for all 32 AUTO phases,
MANUAL mode, transfer selection, active transfer, completion, and leaving tanks.
It also requires actual pixel differences inside the arrow, so changing unused
palette words cannot satisfy the test. The local phase-15 PNG was visually
inspected; screenshots are ignored/local, not published. This is a software
render comparison, not external-emulator playback or player confirmation.

## Follow-up: reserve tank strip and fill flicker

The missing tank strip was reproduced in an empty-tank rendered comparison:
pixel (24,97) should have been gray but was black. The equipment OAM pass now
ports $82:B2AA-$B3D8: full tank maps, native fourteen-energy partial-fill maps,
empty tanks, and a final cap, with ROM X/Y origins and OBJ palette three.
The low partial-fill dither uses accepted-NMI bit two and the native comparison
of **twice** the fill quotient against seven. Rendering never advances the phase.

`--pause-reserve-tanks` (also in the full suite) compares the rendered strip and
cap against independent native spritemap construction for 128 capacity/supply/
phase cases, including zero capacity, 0/100/200/300/400 boundaries, low fill and
14-energy boundaries. Repeated redraws must be identical, and a serialized pause
must retain nonzero NMI phase. The local 199-energy PNG was visually inspected;
it remains ignored and unpublished.

The native palette setup at $82:B3F9 advances an internal counter but ultimately
hardcodes palette three. No extra palette cycling is invented. The visible fill
flicker uses the independent NMI counter, now passed through stable and fading
pause states. Older pause states warn and restore phase zero until the next
accepted frame, retaining the earlier manual-transfer migration as needed.

## Follow-up: live pause HUD publication

The real frontend paused dispatcher reproduced the stale AUTO indicator: after
switching to MANUAL and accepting the next NMI, its tile remained $3C33 instead
of the native blank $2C0F. Pause retained only its initial gameplay HUD copy and
never ran the normal HUD counter/upload routine after input.

State $0F now follows $82:90F2/$90F6: dispatch pause input, clear the six AUTO
cells for AUTO-to-MANUAL ($82:AF33), update the existing HUD, and queue its normal
DMA. AUTO uses the existing $80:998B/$9997 tables. The isolated pause PPU mirrors
only the mutable HUD range **after** the next accepted NMI, including the first
unpause fade frame; it does not display queued writes early or recopy gameplay
FX into the deliberately cleared pause background.

`--pause-reserve-hud` (also in the full suite) constructs a paused boundary and
uses real frontend input, HUD writers and DMA. It checks all six AUTO cells,
exact per-frame health tile words, actual rendered health-digit changes, empty
AUTO restoration, preservation of other HUD cells/FX rows, and the last pending
upload across unpause. Targeted and full verification and the Windows Release
build pass. This is not external-emulator playback or player confirmation.

## Follow-up: automatic completion frame ordering

The automatic frontend cleared the global freeze only after its completion-frame
gameplay pass, although the refill helper had already unlocked Samus. Native
$82:DC18-$DC28 clears freeze, selects state eight and restores Samus handlers
before calling gameplay. The frontend now performs that handoff inside the
accepted-NMI callback, before the gameplay systems run.

`--reserve-auto-frontend` (also in the full suite) constructs the real automatic
recovery boundary with two reserve energy and a pending shared projectile cooldown.
The nonfinal frame must keep the freeze, input lock and cooldown. The final frame
must clear both locks and advance the cooldown from 10 to 9; before the fix it
remained 10. Merely checking the final freeze flag or Samus movement was insufficient:
both could pass even while the projectile pass incorrectly remained frozen.
Existing transfer tests separately cover one-point cadence, sound requests,
exhaustion and maximum-health depletion. Cartridge playback/presentation comparison
is still outstanding; this regression verifies the specific completion-order defect.

## Original cartridge CPU comparison

The headless `native-reserve-refill-probe.h` now executes the restored original
65816 routines, not the upstream C translations: manual $82:AF4F, automatic
$82:DC31, HUD $80:9B44 and manual tank drawing $82:B2AA. Both native NMI counters
advance. Fresh constructed cases use supplies 2/21/99/199, manual health 20 or
automatic health zero, maximum health 99 and reserve capacity 200.

All **402** refill frames match C# health, reserve supply, manual delay and HUD
digits; automatic cases also match all six AUTO cells through depletion. All
**181** manual frames match rendered tank-strip pixels using the original
routine's OAM output as the independent expected sprites. The separately animated
selector remains identical on both sides because it overlaps the strip while
transfer is selected. Initial probe-only mismatches were corrected by retaining
that selector and advancing the separate native byte NMI counter; no speculative
production change was made to accommodate them.

To regenerate locally, apply `movement-release/native-reserve-refill-entrypoint.patch`
inside `upstream-sm`, build its Release x64 target, then run its executable with
`--diagnostic-reserve-refill ROM NEW_OUTPUT.csv`. Output is exclusive-create and
includes a companion `.oam.csv`. Run Verification with
`--reserve-native-trace NEW_OUTPUT.csv`, then reverse only the temporary entrypoint
patch. The probe returns before SDL initialization and sends errors to the console.
No native output, ROM data or screenshot is published. Local results are under
`csharp/test-temp/reserve-native-527/refill-v3.csv`.

This is cartridge **routine execution**, not full-game emulator playback. It
independently validates the refill counters and manual strip across their full
duration, but does not establish all automatic gameplay presentation, controller
handler interactions or audible output. Those remaining checks keep #527 open.

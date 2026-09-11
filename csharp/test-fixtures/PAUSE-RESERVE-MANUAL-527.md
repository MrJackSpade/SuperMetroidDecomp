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

- Rendered reserve-tank fill/animation and energy-arrow palette/tile effects.
- Live pause HUD energy and AUTO-indicator publication during manual changes.
- Automatic-recovery presentation and frame ordering, separately from manual.
- Cartridge playback comparison of the reported visual/timing behavior.

Issue 527 remains open **without** awaiting-player-validation until that work is
implemented and verified. This slice restores missing controls, not full parity.

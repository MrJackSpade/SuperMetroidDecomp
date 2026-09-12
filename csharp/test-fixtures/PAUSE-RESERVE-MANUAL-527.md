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
`csharp/test-temp/reserve-native-527/refill-audio.csv`.

This is cartridge **routine execution**, not full-game emulator playback. It
independently validates the refill counters and manual strip across their full
duration, but does not establish all automatic gameplay presentation, controller
handler interactions or audible output. Those remaining checks keep #527 open.

The native trace now also records the library-three refill request each frame,
draining that queue between calls. All 402 frames match: manual transfer emits
$2D according to its rounded transfer-delay word, automatic recovery according
to the accepted NMI word. The C# manual comparison inspects the real audio queue;
automatic comparison checks the recovery owner's request publication. This does
not validate the frontend ordering of that publication relative to other sound
producers, or audible PCM output. Regenerate older traces before using the new
comparator because the final sound column is required.

During this audit, the native low-health warning latch/producer was found absent
from the translation. It is tracked separately as #560, including the external
check called by automatic reserve state at $82:DC2B and the 30-to-31 threshold
handoff. Refill sound $2D itself is distinct from that warning's $02/$01 commands.

## Automatic warning integration (#560)

The automatic frontend test reproduced the missing external check: after the
first frozen refill frame, the warning latch remained inactive. Automatic state
now executes the $82:DC2B check after publishing preceding gameplay audio requests.
It starts the library-three warning once and stops it exactly as refill crosses
30 to 31 energy. Refill sound $2D was also moved into the pre-gameplay callback,
matching $82:DC31's original position; Power Bomb suppression is honored there.

`--reserve-auto-frontend` now follows 33 actual frontend refill frames and checks
health, persistent warning state and the real audio queue's start/stop requests.
It retains the completion-frame freeze/cooldown regression and verifies Samus
graph serialization plus the explicit older-layout migration. Old states warn
and initialize the unavailable latch inactive on their next admitted check.

This integrates **automatic recovery only**. The separate ordinary beta/gunship
admission paths and audible playback remain work for #560; no claim is made that
all gameplay low-health warnings are now present.

## Ordinary beta and gunship warning integration (#560)

The runtime now accepts a per-frame health-check publication callback beside its
existing echo callback. The ordinary beta seam invokes it after periodic damage,
excluding death, demos, locked Samus and X-ray owners. The frontend connects its
normal gameplay/fade calls to the shared warning owner and audio queue; NMI-only
pause and door-scroll calls do not fabricate checks. The gunship's command-$1A
entry/exit phases admit the dedicated $90:E902 handler despite locked input; its
lifetime is derived from native actor functions, with no duplicate saved flag.
The initial post-Ceres descent does not install this handler.

The real frontend regression verifies healthy-to-critical and critical-to-healthy
commands in ordinary gameplay, omission while generically locked, resumption on
unlock, and the gunship entry exception in a loaded Landing Site. The pause HUD
test also checks that low health while paused does not start the gameplay warning.
No audible playback claim is made yet; #560 remains open for independent native
handler/queue comparison and audible checks.

## Automatic recovery rendered HUD continuation

`--reserve-auto-frontend` additionally checks 33 successive captured gameplay
frames after seeding the pre-transfer HUD through the real upload queue and NMI.
The displayed health words match the cartridge digit table for the preceding
frame's health, including during frozen recovery. Every ones digit has distinct
rendered pixels, and repeated occurrences of the same digit match exactly.
The first post-completion NMI publishes all six empty AUTO indicator cells from
the cartridge table. Existing checks still cover the same-frame completion
unfreeze and low-health warning threshold.

This closes the earlier gap between counter-only automatic checks and actual
HUD presentation. No production change was needed for these properties. The
402-frame cartridge routine comparison, 181 native-OAM tank render comparisons,
and manual frontend HUD regression were rerun successfully. Full-game native
playback and audible output are not established by this added coverage; #527
remains open, without claiming a missing whole-body refill animation.

## Automatic recovery admission through gameplay

The frontend test now starts in ordinary gameplay at zero health with reserves,
rather than calling the recovery owner directly. The completed gameplay frame
must select automatic recovery before Start can admit pause, retain zero health
and the complete reserve supply until the next frame, publish both recovery locks,
and retain a gameplay render packet. Subsequent checks still exercise the first
transfer, frozen intermediate frame, same-frame completion unfreeze, and all 33
displayed health frames. Capture sequence IDs now advance monotonically.

This additional entry coverage passes without a production change. The existing
original-CPU trace still matches all 402 transfer/HUD/sound-request frames and 181
rendered manual-tank frames. Logs: `csharp/test-temp/527-entry.log` and
`csharp/test-temp/527-native-current.log`. This does not add audible PCM or full
native gameplay-loop presentation evidence; those claims remain unproven.

## Reproduced automatic Samus animation-clock mismatch

The next presentation check exposed an actual defect: after normal gameplay
entered recovery, the first frozen frame decremented Samus's animation delay
from9 to8. Native command `$1B` (`$90:F411`) selects command zero (`$90:F109`)
outside demo recording, installing the stationary beta rather than AnimateSamus.
Recovery set only the generic input lock, which suppressed movement but left
the animation phase running.

Recovery now uses the existing stationary-script owner and releases it through
the paired command on completion, before that frame's gameplay pass. This does
not invent a reserve-specific body animation. The frontend regression fails
before the fix and passes afterward, checking both cursor and timer throughout
all nonfinal frames of the 33-energy refill and normal ownership on completion.
Existing displayed HUD and freeze-order assertions remain intact.

Local evidence: `527-animation-before.log`, `527-animation-after.log`,
`527-animation-suite.log`, and `527-animation-windows.log` under test-temp.
This corrects a verified animation-clock property; it does not establish audible
PCM or whole-frame external-emulator parity, so the broader ticket remains open.

## Original CPU confirmation of the stationary animation owner

The reserve probe now writes a required `.samus.csv` companion. For all 221
automatic refill frames (supplies 2/21/99/199), original `$90:F411` selects the
locked alpha/stationary beta, that beta executes while reserves remain, and
completion executes original `$90:F2E0`. The native cursor 3 and timer 9 remain
unchanged through every stationary call. Completion records normal handler
selection before its gameplay call; it does not execute normal beta in this
bounded probe.

The comparator matches native handler IDs to actual C# stationary ownership,
checks the retained native cursor/delay, and requires all 221 rows. All 402 existing
transfer/HUD/sound rows and 181 manual tank render frames still match. Regenerate
older traces lacking the companion; do not silently skip the new comparison.
Current local output is `reserve-native-527/refill-animation.csv` under test-temp;
comparison log is `527-native-animation-compare.log`.

Temporary probe hooks were removed and the ordinary native executable rebuilt.
No ROM, trace, OAM capture, screenshot or audio is published. This independently
confirms the corrected handler behavior, not full-game external playback or PCM.

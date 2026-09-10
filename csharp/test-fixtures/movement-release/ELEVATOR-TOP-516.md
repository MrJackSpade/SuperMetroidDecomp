# Upward Blue Brinstar elevator top-edge reproduction (#516)

Affected version: **0.1.1**. Reproduced; one contributing scheduling defect is
fixed, but the remaining pixel failure is unresolved. No validation label yet.

```powershell
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- `
  --elevator-top-edge-audit 'Super Metroid.smc' csharp/test-temp/elevator-top-516
```

The fixture loads the retail Morph Ball room, equips Morph Ball and five
missiles to select the return state, positions Samus on the actual elevator,
and uses Up inputs to ride until the real door collision publishes the Blue
Brinstar elevator destination. It then runs the production door coroutine,
arrival movement, and thirty settled frames. It does not publish a fabricated
door at the elevator's resting position.

The 231-frame trace captures the transition and arrival. On frame 96 Samus's
world Y is 292, camera/display Y are both zero, and her boots are visibly drawn
near the top of the room, below the HUD. `frame-0096.png` was visually inspected.
This is the reported symptom, not an inference from an endpoint assertion.

The regression blackens Samus's dedicated OBJ palette in an immutable copy of
the same display packet, preserving OAM, tile memory, layer ordering and the
rest of CGRAM. It counts actual composite pixel changes in the top 80 rows
while Samus's center is at least 256 pixels below the viewport origin. It
currently **fails**, with 186 pixel-frame differences, first at frame 96.
The check does not alter gameplay or infer failure from camera alignment alone.

Native source review: `DrawSamusSpritemap` ($81:89AE) writes low-byte Y without
the generic actor renderer's vertical-wrap hiding rule. Therefore simply adding
that hiding rule to Samus's renderer is not justified. The upward transition's
camera offset, final nudge, and arrival timing must be compared against original
CPU execution before changing production code. A stable managed camera at zero
does not prove that zero is the correct cartridge camera position.

Local `csharp/test-temp/elevator-top-516-pixels` preserves the CSV and captured
frames, including the visible feet (96), a later arrival frame (120), and
settled state (224). Screenshots were not committed: the repository is public
and the player has instructed us not to publish screenshots. The Release
build passes; the diagnostic's intentional failure remains visible in the CLI.
## Partial fix: camera tracking during the fade

The pre-fix trace held camera Y=32 at the end of the opening IRQ and through
the music wait. At HandleTransition it abruptly became zero. The destination
OAM build calls the full runtime frame, inadvertently running normal camera
tracking. In contrast, native `$82:E737` runs enemy/draw owners without invoking
`MainScrollingRoutine` (`$90:94EC`). This was a scheduler mismatch, not a missing
Y-clipping rule in Samus's OAM writer.

Normal camera tracking now observes the existing `$0795` door-transition gate.
Despite its legacy elevator-oriented property name, the gate covers all ordinary
doors as well. Camera Y now stays 32 through the fade; the first resumed arrival
frames reduce it 30,28,...,0 rather than snapping it to zero before gameplay.
The audit explicitly asserts unchanged camera Y across HandleTransition and
each destination-fade step.

The same actual-room pixel reproduction drops from 186 to **3** differences,
first at frame 112 rather than 96. It still fails and the ticket remains open
without an awaiting-player-validation label. Do not regard this partial fix as
proof that clipping, camera tracking, or the player's full symptom is resolved.
The remaining three pixels require comparison with native arrival/draw timing.

The Release build, full Core verification, elevator frontend handoff audit, and
forty-frame spin-door native comparison (#517) pass with the scheduling change.
The #516 fade-camera assertion passes; its independent pixel assertion still
fails with the three differences above.

## Corrected audit cadence and residual witness

The earlier audit called `RunNmi` after `StepFrame`, but `StepFrame` already
accepts an NMI in its prologue. The desktop frontend does not make that second
call. This doubled the audit's NMI counter, defeated alternating elevator
visibility, and uploaded freshly built OAM earlier than the normal frontend.
The audit now follows the frontend's single-NMI cadence. Earlier pixel counts
above remain historical observations, not an exact native-timing comparison.

With the corrected cadence, the same 231-frame route still fails: **two**
pixel-frame differences, both at screen (130,32), on frames 113 and 115.
The first witness can be traced to its contributing OAM record by hiding each
palette-four record in a cloned display packet. The trace also includes pose,
animation frame, and NMI counter. This is diagnostic isolation only: neither
live OAM nor production rendering is changed. A native draw/arrival comparison
is still required before deciding whether that residual pixel is a defect.

All PNG output stays under the local test-temp directory. Publish only source,
these textual findings, and numerical diagnostics; never publish screenshots.

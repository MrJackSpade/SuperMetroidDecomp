# Crocomire cutscene camera and corpse collision (#1173)

Affected version: **0.4.4+21b505230fff6a3702ea6dc73c05b84405a66672**.
The player reported seeing left beyond the invisible wall during the cutscene.
The attached session log also records `Enemy $DDBF extended collision frame
$A4:E6BC has no compiled geometry`. The diagnostic contains no saved state or
controller recording.

## Camera

Native `MainAI_Crocomire_DeathSequence_0_NotStarted` at $A4:8C6E writes
Scrolls+4/+5 after the bridge handler, even when that handler starts the death
sequence. Both are blue while Samus is left of X=$0520. At that threshold it
turns screen four red and leaves screen five blue. Later melting states retain
the boundary; $A4:90BD reopens both when the skeleton enters the river.

The port omitted the state-zero scroll writes. With the retail room's eight-cell
scroll layout, its camera could move left from X=$0500 to $04FF after Samus
reached the bridge. `--crocomire-cutscene-camera` reproduced that exact position
failure through production enemy AI and the production camera before the fix.
The fix restores the native writes; there is no hardcoded camera-position clamp.
The regression confirms free movement before the threshold, the X=$0500 left
boundary at the threshold, and the same boundary on the actual collapse frame.

The player's precise cutscene stage was not supplied. This reproduces the
identified melting-sequence boundary omission; it does not claim player
confirmation of the visual report or diagnose a different skeleton-wall symptom.

## Collision

Only the fifty living-body frames had compiled physical records. The thirty-three
corpse frames already had visual compositions, but the shared extended collision
walker rejected them. The reported E6BC frame is a legitimate river skeleton
frame with an empty native hitbox list, not a corrupted instruction pointer.

Added a dedicated corpse collision catalog with all 33 frame component lists and
both native hitbox lists ($A4:E72E and E748). Body collision lookup delegates to
it for corpse records. Some skeleton poses have real rectangles and callbacks;
they are preserved rather than bypassing collision for every corpse frame.

`--crocomire-corpse-collision` reproduced the exact reported exception before
the fix. It now checks every component offset, hitbox identity, rectangle, and
callback against the supported cartridge. It exercises the real collision walker
with a bus rejecting all reads, asserting native no-hit behavior for empty frames
and the correct touch/shot callback for nonempty frames.

## Evidence and confirmation

Source: pinned [InsaneFirebat bank A4](https://github.com/InsaneFirebat/sm_disassembly/blob/362be646929cf8e483f692b73a6561cfc2dc1d0d/src/bank_A4.asm),
bank 8F's Crocomire scroll table at A9D7, and supported NTSC J/U 1.0 ROM SHA256
`12b77c4bc9c1832cee8881244659065ee1d84c70c3d29e6eaf92e6798cc2ca72`.
Reference annotations: [Patrick Johnston bank A4](https://patrickjohnston.org/bank/A4).

Both focused regressions pass. The PLM, queued-VRAM-DMA and enemy-visual static
build gates pass. No artwork schema changes or content re-extraction are needed.
No exploratory gameplay, room sweep or whole-game validation was performed.
Player validation remains pending; issue closure follows the explicit session
instruction to close after implementation, confirmation and commit/push.

## Reopened camera investigation

After the initial commit, the user withdrew the suggested melting-stage answer:
the tester did not specify the stage, and the user suspects the skeleton/wall
sequence on the left. Issue #1173 is reopened; the melting fix must not be treated
as confirmation that this revised interpretation is resolved.

Extended the focused camera fixture to execute the production wait-for-Samus
handoff at $A4:97D3 with the retail scroll layout, then the shared scrolling code.
Existing code closes screen three, retains red screen one, and settles the camera
to screen two at X=$0200. Leftward and rightward targets remain bounded there.
The production $A4:9B65 completion phase reopens the cells and permits scrolling.
This passes without another production change, so it does not reproduce the
reported overrun.

The port currently supplies camera-distance index zero where native Crocomire
writes six. Both values produce the same boundary in this fixture; the pinned
native annotation likewise says the enclosing red cells override that target.
That difference alone is not evidence for another camera fix.

The supplied ZIP contains only a log. A player state/input recording or a video
showing the camera overrun is needed to identify the failing left-side sequence.
The corpse-collision fix remains independently reproduced and verified.

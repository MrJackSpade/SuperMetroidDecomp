# Zebetite player candidate: original CPU consumer (#443)

This is a passing narrow parity diagnostic, not a completed ten-missile technique test.
The native consumer executes the pinned cartridge through `RunAsmCode`; it does
not use translated C behavior as its oracle. It depends on the ROM-loading helper
in `native-release-probe.h` and the existing private MOV1 export.

## Capture

Run DebugRunner `--zebetite-player-export ROM DIRECTORY` first. Keep its room data,
metadata and traces private. Include `native-release-probe.h`, then
`native-zebetite-player-probe.h`, after `StateRecorder` in native `sm_rtl.c`.
Temporarily dispatch a four-argument native executable invocation to
`DiagnosticZebetitePlayer(rom, seed, output)` from `main`. Build Release x64 using
the project's native toolchain. Remove the temporary hooks and rebuild afterward.

Use each `isolated-{724,728,732}.movement-seed` as the seed. The consumer restores
the documented fixed frame-60 candidate metadata: camera 641, ten missiles,
999 energy, generation one, and the linked actors at physical indices 128/384.
The MOV1 seed provides Samus movement and the room collision layer. Inputs are
the same frames 60–119 as the managed exporter, with right+jump ending at frame 99.

Compare each native CSV with its corresponding isolated JSONL:

```powershell
pwsh -File compare-zebetite-player.ps1 -ManagedTrace PATH.jsonl -NativeTrace PATH.csv
```

The comparator fails on any exported field mismatch. It does not accept an
animation mismatch as a successful parity baseline.

## Reproduced mismatch and correction

Before the correction, all three 60-frame captures matched movement, animation
pose/timer, camera integer positions, ammunition and both barrier health values.
Start X=732 also matched the exported first-projectile type/position. X=724 and X=728 failed:
at frame 96 managed code has already converted the missile into an explosion;
the CPU still has a missile. On frame 97 the CPU clears it while the managed
explosion remained.

Source inspection confirmed the handoff: `$A0:A143` marks enemy-hit
projectiles through the direction high nibble; the missile pre-instruction then
clears such a projectile. The managed ordinary-enemy resolver uses
`TryStartEnemyImpact`. The Zebetite callback at `$A6:FDAC` instead calls the native
no-death-check/no-shot-graphic damage routine. The corrected resolver applies the
existing collision prelude for this callback and leaves the projectile's own next
update responsible for deletion. Terrain and unrelated enemy paths are unchanged.
`$93:8254` and `$93:834D` draw projectiles but do not advance their
instruction streams, so the consumer's omitted draw stage does not itself explain
this particular type/lifetime discrepancy.

After correction, all exported fields match across all 180 frames. The focused
`--zebetite-player-setup-audit ROM` also asserts the marked missile's exact impact
position at frame 96 and its cleared state throughout frames 97–119 in both
affected candidates. The existing candidate and adjacent failure controls still
pass. The general `--zebetite-audit ROM` remains passing as well.

## Limits

The consumer omits live FX, PLMs, other actors, audio and drawing. Managed actor
omission has separately been checked for these selected observations, not for
every game subsystem. Camera subpixels, all projectile internal fields, and full
state restoration are not compared by this first consumer. It does not establish
the repeated ten-shot controller sequence, native rendered parity, or full-room
equivalence. Keep #443 open.

## Repeated-input continuation (managed exploration only)

`--zebetite-player-repeat-audit ROM` repeats the original input schedule every
180 frames for 1,800 frames, once with the full population and once omitting
non-Zebetite actors at frame 60. It never resets camera, Samus, health, ammunition,
or the linked halves between shots. Output includes energy so hostile-room
interference cannot be mistaken for a movement discrepancy.

The first isolated landing preserves camera 642 and lower health 900, but upper
health is 901. A blind repeat moves the next landing farther right, exposes the
barrier before the next hit, and fails. Full-population trajectories also diverge
after the previously verified 60-frame interval because other actors interfere;
the short actor-isolation result must not be extrapolated to the full sequence.

`--zebetite-player-second-search ROM` explores 1,300 two-shot schedules, preserving
all live state between shots: 0–12 rightward reposition frames beginning at frame
120, 1–20 left+jump frames beginning at frame 260, and second-shot leads of
0/2/4/6/8 frames relative to frame 258. Right+jump follows until frame 299; each
trial runs to frame 339. None finishes with lower health at or below 800 and eight
missiles remaining. These are failed candidate schedules, not cartridge failures.

This initially suggested investigating an upper-half shot, but the linked callback
copies the struck half's health to its partner on every accepted hit. Upper health
901 does not by itself disqualify continued shots into the lower half. The real
remaining requirement is continuity of the struck half through subsequent hits,
plus the linked death handoff. Original-CPU comparison of a successful repeated
sequence remains outstanding.

## Documented return-hop exploration

The [technique reference](https://wiki.supermetroid.run/10_Missile_Zebetite_Kill)
includes a separate jump/turn sequence between shots. The earlier simple repeat
did not implement that step. `--zebetite-player-return-search ROM` explores it
without editing state after the initial setup (except the documented actor
omission at frame 60).

The 2,340 candidates vary the first right+jump release (99/102/105), return-hop
left input length (1–30), and final right tap (frames 200–250, step two). The return
jump starts at 150, Left is added from 151, and Jump is held through 209. Each
candidate runs through frame 279. A candidate must retain lower health exactly
900 on every frame from 119 onward, retain Samus energy 999, finish grounded and
facing right, and move left of its post-first-hop landing position.

33 candidates satisfy those conditions. For example, first release 105, eleven
left frames and a right tap at 202 end at X=742.5, Y=139, camera X=642, nine
missiles. This is not the initial X=728 state, so it is not yet a closed repeating
sequence. Wrong-facing endings are excluded rather than counted as successes.
The focused one-hit/lifetime regression still passes. The next shot and native
comparison of these longer trajectories remain required; no gameplay change is
established by this search.

## Two-hit continuation

`--zebetite-player-second-hit-search ROM` now continues the release-105,
eleven-left-frame, right-tap-250 return-hop candidate into a second shot. It runs
420 uninterrupted frames per trial; only the previously documented actor omission
at frame 60 changes non-controller state after initialization. Lower health is
checked every frame for any increase, not just compared at the endpoint.

Of 1,920 schedules, nine end with eight missiles and exactly 800 lower health
without regeneration. Three also retain Samus energy 999. The remaining six take
damage later and are not equivalent full-state fixtures. The shortened follow-through
uses eighteen right+jump frames; the earlier forty-total-frame follow-through
caused additional hostile/terrain interference and must not be substituted.

The focused `--zebetite-player-second-hit-audit ROM` runs eight adjacent cases.
After the initial hit and return hop, Down is pressed at 280/281 and Left at 286.
Shoot at 292, Left+Jump at 304–306, then Right+Jump at 307–324 produces the
undamaged 800-HP result. Starting that jump at 303 instead gives 801 HP. Jump 304
with four left frames and jump 305 with three left frames also succeed. The
other focused neighbors fail. The focused diagnostic asserts this candidate
window but is explicitly not an original-CPU oracle.

No live state is reset between the hits. Subsequent shots and the actual ten-hit
death/double-kill handoff remain required.

## Original-CPU two-hit comparison

`--zebetite-player-second-hit-export ROM DIRECTORY` writes the eight focused
managed traces. The native header also exposes
`DiagnosticZebetitePlayerSecondHit(rom, seed, output, jumpDelay)`; supported delays
are 3 (adjacent failure) and 4 (success), with three left-input frames. Use the
original `isolated-728.movement-seed`: its frame-60 state is unchanged. Wire the
function into a temporary headless native entry point as for the first consumer,
then remove the hook and rebuild the ordinary executable afterward.

Compare native output to `second-6-3-3.jsonl` / `second-6-4-3.jsonl` using
`compare-zebetite-player.ps1 -FrameCount 360`. Both comparisons pass all frames
60–419, including the return hop, second missile, landing, and remaining idle
interval. Delay 3 ends at 801 lower HP; delay 4 ends at 800. Both retain Samus
health 999. In addition to the original movement/camera/projectile/ammunition
fields, this mode requires Samus health, both barrier flash timers and both AI
handler words. Missing fields are errors, not implicit zeroes.

The longer harness writes its NMI byte and word counters separately so byte wrap
does not truncate the word after frame 253. Both cases were recaptured after that
correction and still match. Native output SHA-256 (CSV, Windows line endings):

- Delay 3: `983B48E31CE2CD8C7D2D9FFA7844C7B2B49CC8FE60B1386EF712A6A520327852`
- Delay 4: `B1E0E9F7863684739BF43E4A14D08EF3F318734C1BD3B6C22DC3D70AE2D868CB`

This proves the exported two-hit timing properties for the isolated setup, not
full room/audio/render equivalence, ten shots, or the final double-kill sequence.

## Three-hit continuation candidate

`--zebetite-player-third-hit-search ROM` extends the CPU-matched two-hit prefix
through frame 719. It varies the next return-hop start (420–480, step ten), its
left-input duration (10–25), the third jump delay (0–15 after frame 600), and that
jump's left-input duration (three/four frames). No controller-independent state
reset occurs between hits; actor omission remains at frame 60 only.

Of 3,584 schedules, 43 finish with seven missiles and lower health exactly 700
without any intervening health increase. Twelve also keep Samus energy 999.
The initial return start of 450 produced damage during the hop and no exact
700-HP continuation in the first timing grids; delaying/advancing the return is
part of the setup, not grounds to change game collision or damage behavior.

The ten-case `--zebetite-player-third-hit-audit ROM` preserves an undamaged timing
window: return Jump from 460 through 519; Left from 461 for thirteen frames;
Right tap 550; Down 580/581; Left 586; Shoot 592; Left+Jump 604–607;
Right+Jump 608–625; neutral through 719. This ends at 700 lower HP, seven missiles,
999 energy, X=759, camera X=663. Jump delays 4/5/6 work with thirteen return-left
frames; only delay 5 works with fourteen. Focused adjacent cases fail.

The two-hit input schedule is now shared by both fixtures. Re-exporting it still
matches both original 360-frame CPU captures. The three-hit continuation itself
is managed-side evidence only and still requires native comparison before any
parity claim. Later hits and the complete death/double-kill sequence remain open.

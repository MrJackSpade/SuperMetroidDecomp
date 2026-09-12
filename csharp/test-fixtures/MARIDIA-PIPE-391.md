# #391 Maridia elevatube descent investigation

## Status

The incoming-door movement-lock defect is reproduced and corrected. The final
physical approach also verifies both displayed terrain layers throughout ordinary
tube gameplay and arrival in Oasis. Ready for player validation, not closed.
Earlier residual BG1 discrepancies below are historical diagnostic findings:
pre-publishing the door skipped the source camera's physical approach.

Terrain mismatches are observable on the current code, but the initial direct-load
capture inherited an unrelated scroll origin and is not sufficient proof of the
player's defect. A bounded incoming-door control now captures the actual frontend
handoff as well. Neither control is a completed cartridge comparison or fix.
The historical camera correction still carries Samus through the tube and into
Oasis. Do not mark this issue awaiting validation on the basis of state restoration
or camera coordinates alone.

## Restoring the preserved capture

The local, untracked `maridia-041b-pipe-entry/slot-0-named.smstate` predates several
state-layout changes. It is read through the production debugger state reader in
a disposable directory, never through the player's current slot. No state, ROM,
audio, or rendered images are included in this commit.

The initial current-build replay failed on the frontend's 46/47-field mismatch.
After that was reconciled, it exposed additional known historical layouts. Git
history from `b944f1b5` identifies the added/changed members; the reader continues
checking the declaring type, field name, and order of every surviving field.

| Historical layout | Explicit restoration |
| --- | --- |
| Frontend before `3a891459` | Missing pause-fade counter is zero: the next fade step is eligible, then follows native cadence. |
| Enemy owner before `833cc4ee` | Missing per-frame projectile dependency is null until EnemyMain supplies the live owner; composes with the older two-field statue migration. |
| PLM slots before `571b9a42` | Missing Samus Eater held coordinates are zero until an actual plant capture setup. |
| Samus before the independent bomb lock | Omit the nine additions proven against `b944f1b5`; retain the existing previous-draw input rather than mistaking the missing bomb lock for that older latch. |
| Kinematics before prospective-pose contact snapshot | Missing nullable contact mode retains lookup through the saved live Samus owner. |
| Projectile owner/slot/result before charge-combo integration | Restore no pending combo, auxiliary phase zero, and no additional sound-request list; preserve all existing projectile, charge, trajectory, and sound fields. |
| Suit pickup before its entry sound latch | No pending transformation sound; saved transformation phase remains intact. |
| DSP voice before silent-release BRR fallback | Missing nullable fallback retains the saved PCM/envelope state until a normal loop handoff. |
| PCM bank before cross-source loop routing | Construct an empty routing dictionary, preserving the old self-loop behavior until the next bank upload. This limitation is warned about, not presented as exact modern routing. |

The old nineteen-field Shinespark layout needs a different adapter: it stored
crash angular travel/delta separately from the echo coordinates now aliasing them.
The adapter reads the exact old field sequence and retains both removed values
until all echo records are restored. In a live crash phase it transfers those
values to the shared words, sign-extending the old signed byte delta. Outside a
crash it preserves the echo coordinates. Conflicting historical copies cannot
both occupy one native word; restoration warns about this limitation.

Synthetic tests cover the precise surviving field sequences, eight legacy spark
graphs (inactive plus three crash phases, both delta signs), current-format round
trips, rejection of an unknown legacy field, and initialization of the old PCM
self-loop map. The actual saved graph now loads and executes 420 frontend frames.

## Initial direct-load capture (setup limitation)

```text
dotnet run --project csharp/src/SuperMetroid.IntegrationVerification -c Release -- --maridia-pipe-from-north
```

The diagnostic loads the preceding tube through its real northern door setup,
starts Samus at `(128,64)`, then supplies neutral input. The frontend owns the
single transition from `$04/$18` (D408) to `$04/$1B` (D48E).

At frame120 Samus is Y1064 and camera Y938; at frame210 they are Y2335 and Y2209.
The exit reaches Oasis and returns to ordinary gameplay. Captured descent frames
90 and 180 visibly contain horizontal bands across the sand/tube background;
frame180 includes retained purple architectural strips in the surrounding sand.
These frames are local under `csharp/test-temp/issue-391-pipe-north`.

The restored slot is in Oasis, not the source room above the tube. The direct load
retains its BG1 offsets `(60672,861)`. Resetting the initial coordinate origin to
zero (`--maridia-pipe-fresh-origin`) removes all observed BG1/BG2 tile-word
mismatches throughout that direct-load descent. Do not use these original bands
alone to justify a production streamer change.

## Incoming-door control

The following pre-published-door results describe the earlier diagnostic. The
same command now uses the physical approach documented in the final section.

```text
dotnet run --project csharp/src/SuperMetroid.IntegrationVerification -c Release -- --maridia-pipe-incoming-door
```

This constructs a room-local Plasma Spark boundary at authored door block 3014,
Samus `(104,736)`, then queues door `$83:A5AC` through the actual collision
dispatcher. After this setup, all 620 calls use the frontend with neutral input;
there are no further diagnostic position writes. It traverses the tube and its
south exit, not an extended playthrough. Initial source scroll offsets are zero;
the production door handoff establishes the destination offsets itself.

The captured destination BG1 Y offset is 739, BG2 Y offset 224. At frame150,
BG1 has 130 sampled tile-word mismatches; at frame180 it has 606; at frame210
BG2 has 80 as well. Frame270 reaches the tube bottom; by frame390 the frontend
is back in ordinary Oasis gameplay. Logs and every-frame PNGs remain local in
`csharp/test-temp/391-incoming-door.log` and
`csharp/test-temp/issue-391-pipe-north-fresh-origin-door`.

`MaridiaPipeTerrainAudit` compares immutable captured VRAM words against expanded
authored room blocks at displayed coordinates, including the PPU's first visible
scanline. It reports both layers independently. This is an observation, not yet
an authoritative assertion: transition frames can contain intentionally partial
tilemaps, PLMs can modify tiles, and architectural changes are authored scenery.
It does not compare final pixel composition or prove original-cartridge parity.

Next: compare the incoming handoff and movement/streaming cadence with the pinned
cartridge, isolate the erroneous displayed rows, and make that specific assertion
fail before changing production code. In particular, inspect the combined normal
fall and room-main displacement rather than assuming every mismatch is a PPU bug.

## Verified movement-owner correction

The real frontend capture failed at the first ordinary gameplay frame after the
incoming fade: command-zero ownership had been discarded. Both native entry
callbacks `$8F:E26C/$E291` invoke command zero (`$90:F109`), which installs the
stationary alpha/beta pair. Native `$82:E737` does not unlock it. Our fade completion
unconditionally unlocked non-elevator actors, allowing ordinary falling to add to
the room-main displacement. The direct-load test skipped that destructive handoff.

Both entry callbacks now use the existing stationary-script command owner; exit
callbacks use its paired unlock. The common door fade preserves that ownership
while still releasing its temporary input lock for ordinary arrivals. This does
not special-case a room address in the fade or change the native streamer.

The 620-frame frontend regression now asserts ownership after fade and exact
16.16 displacement from the signed 8.8 tube velocity throughout the interior.
Both-direction core checks also assert exact displacement for 180 frames each.
The before/after logs are `391-lock-before.log` and `391-lock-final.log` in local
test-temp. BG2 discrepancies in ordinary descent are eliminated in this capture;
BG1 discrepancies remain (for example a full row at frame240). They must still be
compared to the native tile producer before claiming the reported visual issue is
resolved. The original direct-load offset caveat also remains applicable.

## Final physical door approach

The setup now places Samus at `(128,656)` in Plasma Spark without publishing a
pending door. Normal gravity, down input, and fresh mapped shoot presses open
the authored blue cap; jump input is available after frame60. Inputs stop on
leaving the source room. This is a bounded constructed room fixture, not a replay
of the player's exact approach. No coordinates are changed after initial setup.

The camera reaches Y543 before the physical collision. Downward setup increments
the retained BG1 vertical mirror, yielding destination offset768, rather than the
artificial offset739 obtained by prematurely queuing the door. Both BG1 and BG2
then have zero sampled authored-tile mismatches throughout ordinary tube gameplay.
The only observed transition mismatch is an intentionally partial destination
tilemap before scrolling starts; transition frames are excluded from that assertion.

The finalized 620-frame regression checks stationary ownership, exact room-main
displacement, every sampled visible terrain word on both layers during tube
gameplay, and final ordinary gameplay in Oasis. It fails if the approach never
enters the tube, preventing a closed-door no-op from being mistaken for success.
Final log: `csharp/test-temp/391-natural-final.log`. All-frame PNGs stay local in
the existing incoming-door output directory; frames300 and510 were inspected for
the tube scenery and Oasis landing respectively. No renderer/streamer patch was
needed beyond preserving native movement ownership in `3f9fbfb5`.

This proves the tested physical approach and displayed tilemap contents, not
pixel-for-pixel original-emulator parity for every possible tube entry. Player
confirmation remains required for the original report.

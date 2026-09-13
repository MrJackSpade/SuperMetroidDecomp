# Reflected Super Missile trail crash (#613)

Affected version: player reported `0.3.1 (? latest)` production release.

The private production recording `SuperMetroid-input-20260913-132933-922.smrec`
reproduced the exact $9B:21DB exception at zero-based frame 30450 in room
$8F:B62B. The recording and generated images remain local, not published.

Reflection installs instruction list $93:9F27 with timer one while preserving
the trail timer. Retail $93:81D1 reads the preceding word ($9F1B), which makes
the trail offset access reach reserved B-bus addresses. Bank $9B's native
absolute-indexed word loads see CPU open bus there. Independent strict byte
reads in the port instead threw. The fix models operand-driven MDR and word
read ordering only for the proven reserved $2184-$21FF window; other missing
hardware accesses remain strict. Projectile reflection and animation timing
are unchanged. The upstream C timer-one lookahead is not in the pinned ROM.

References: pinned disassembly bank $90:BE00, $93:81D1/$9F27 and
$9B:A44A/$A464/$A47E/$A497; https://snes.nesdev.org/wiki/Open_bus.

Verification:
- Original recording failed at frame 30450 before the fix and completes all
  30451 calls afterward.
- DebugRunner `--reflected-super-trail-audit ROM` checks production reflection,
  exact trail coordinates, animation progression, and strict unrelated reads.
- Core verification covers mirrored/unmirrored banks, operand MDR, and mixed
  mapped/open-bus word accesses. Full core verification passes.
- Windows Release build passes with zero warnings/errors.

Awaiting player confirmation; reproduction success does not close the issue.

## v0.3.2 recurrence: $9B:226B

The installed production DLL identifies itself as 0.3.2.0, commit 2719b558.
The original correction covered reserved B-bus space but omitted the adjacent
unpopulated A-bus expansion window. The original direction-one regression was
insufficient. Before the second production change, the expanded real projectile
test reproduced $226B for direction 4/5 and also failed on directions 2, 3, 6,
7, 8 at $220B/$223B/$229B/$22CB/$22FB. Directions 0, 1, 9 already passed.

The fix models $2200-$3FFF as undriven for this game's unenhanced LoROM mapping,
not for arbitrary enhancement-chip cartridges. Operand MDR and low-to-high data
ordering are unchanged. All ten direction entries now assert trail coordinates,
list advancement, and trail cadence. Memory tests cover nonzero MDR, the
$21FF/$2200 boundary, the $3FFF/$4000 boundary, and unmirrored banks.
CPU/APU register accesses remain strict. Mapping reference (Anomie's hardware
document): https://wiki.superfamicom.org/memory-mapping.

This repeat is reproduced with the production projectile fixture; it is not
claimed as a replay of the new short recording, which has no debugger-state seed.

## Local progress recovery

DebugRunner `--recover-recording-state RECORDING ROM AUDIO_DIR NEW_DIRECTORY LAST_FRAME`
replays the recorded SRAM/options/inputs with actual managed audio acknowledgements
and captured gameplay rendering. It exports slot 1 after the inclusive input index,
then reloads it and verifies the remaining recorded inputs. An existing destination
is refused. `--verify-recovered-state` separately checks an existing export.

For this report, index 30449 restores the frame immediately before the failure in
$8F:B62B. Recovery was tested using the downloaded v0.3.2 Windows Core/Diagnostics
assemblies, including the next formerly failing input and retained gameplay scene.
The final state is generated with those production assemblies to avoid build-ID
warnings. No live slot is overwritten and no private recovery data is published.

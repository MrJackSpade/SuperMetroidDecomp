# Movement-release cartridge probe and runtime regression

## Falling-entry correction (#312)

The isolated `$90:9B1F` recurrence below is valid for mode two, but does **not**
establish that mode two is valid after walking off a ledge. A room-seeded original
CPU comparison exposed the missing `$91:F60D` falling-pose initializer: entry sets
mode zero without extra dash speed, or two if either extra-speed word is nonzero.
The managed walk-off previously retained the grounded release mode.

`--walk-off-momentum-audit ROM` reproduces that defect through the full runtime on
a finite synthetic ledge in both directions, dry and underwater. It failed before
the fix and now asserts stationary horizontal position throughout neutral-input
falling. Verification also covers zero/nonzero whole and fractional dash speed,
all three incoming modes, and preservation of base/extra speed on pose entry.

### Repeating the room-seeded CPU comparison

The door audit accepts `ROM source destination X Y [Xfraction Yfraction items
[seed-prefix]]`, with numeric arguments in hex. It prints four transition variants
and optionally exports one private `.movement-seed` per variant. Use the supplied
integration patch, build the native runner, and execute:

```powershell
./upstream-sm/build/bin-x64-Release/sm.exe --room-release-probe ROM seed-prefix-True-True.movement-seed
```

Capture native and managed console output, then run `compare-room-release.ps1`
with `-NativeTrace` and `-ManagedTrace`. It compares twenty frames of exact X, Y,
pose, base speed, and acceleration mode. All twenty matched for the locally
reconstructed reported doorway entry after the fix. Private recording, seed,
and cartridge-derived room data remain local and are not included here.

Scope: the seed restores running Samus and room blocks, not enemies, live PLM
execution, or moving liquid surfaces. Only compare an interval independent of
those actors. This verifies the excessive post-ledge momentum defect, not the
separate report about automatic travel distance during the door transition.

## Short-tap trajectory comparison (#314)

The same native command also prints 720 `TAP` records: dry/submerged, both turn
directions, presses lasting one to three frames, and 60 recorded frames per case.
Each case starts standing and warms up for 24 neutral frames. The actual cartridge
CPU runs input, movement, animation, pose transition, and collision/pose checks.

Capture its console output, then compare with the production managed dispatcher:

```powershell
dotnet csharp/src/SuperMetroid.DebugRunner/bin/Release/net10.0/SuperMetroid.DebugRunner.dll --short-tap-comparison-audit "Super Metroid.smc" native-trace.log
```

All 720 samples currently match **exact X position, pose, and base speed**. This
is stronger than checking eventual facing, but it does not reproduce #314's
physical-controller discrepancy. The comparison bypasses WinMM polling, UI
scheduling, and display presentation. Do not mark the player report fixed based
on this result. Animation frame/timer values are printed for diagnostics but are
not asserted by this comparison. No player recording or cartridge bytes are
included in this fixture.

### Host catch-up input regression

`SuperMetroid.DesktopVerification --input-batch-audit` separately exercises the exact batch owner
used by `PlayableGameControl`. Its synthetic clock advances 20 ms inside each
frame while a gamepad press lasts from 10 to 30 ms. The original once-per-batch
poll returned `[0,0,0]`; per-frame polling returns `[0,Left,0]`. The regression
also checks release, zero-frame batches, and stopping on an exhausted replay.
It opens no windows and needs no ROM or physical controller.

This fixes a demonstrated dropped-input path under catch-up load. It does not
prove that this was the only cause of the player's underwater-only observation;
player confirmation remains necessary. A pulse entirely between two individual
frame polls remains outside this polling adapter's guarantees.

This console-only experiment constructs a flat floor and running-left Samus at
full base speed, then releases all input in dry and submerged variants. It has
no player recording, SRAM, or ROM bytes. Supply a local cartridge separately.

## Status / limitations

The dry-control initialization was corrected: ordinary air starts at `$90:9F55`,
not the preceding `$90:9F49` standalone grapple record. Both previous-movement
bytes now agree with the running pose. The extra one-pixel pose-stage movement
was traced to `$91:EADE` / `$91:EB48` and is **real cartridge behavior**, not a
fixture error. The disassembly explicitly identifies the retained forward move.

The C# runtime passed only matched input poses to this check, omitting a running
pose retained by no-input fallback. `--running-release-audit ROM` reproduces this
through `StepFrame`: before the fix, its first X was `$00C5.4000` instead of the
CPU's `$00C4.4000`. After the fix, 100 exact per-frame X offsets, base speeds, and
retained probe displacements pass for both directions in dry and water fixtures.
The extended test also retains direction while releasing Dash: it stays at the
native base running speed without the no-input fallback's extra pixel.

For left-facing water, the first four CPU X values are `$00C4.4000`, `$00C0.8800`,
`$00BC.D800`, `$00B9.3000`. Dry values are `$00C4.4000`, `$00C1.0000`, `$00BE.4000`,
`$00BC.0000`. These are synthetic experiment results, not player-recording data.

The probe explicitly restores cartridge bytes after `SnesInit`: the upstream
comparison harness patches carry instructions, which must not silently become
the reference for a retail comparison.

The full no-input stop distances are `$0010.3000` pixels dry and `$00D3.6400`
underwater. Continuing direction for 100 frames travels `$0113.0000` in either
medium. Each sample recenters X without resetting velocity/pose, then accumulates
accepted displacement, so the finite floor cannot terminate a coast artificially.
The managed scaffold reloads its room normally to clear the fresh-Ceres elevator
arrival coroutine; leaving that live would teleport Samus during a long test.

Scope: this proves the release-path defect and the tested full-stop / retained-
direction trajectories. It does not prove the original pipe-exit speed (#312),
turnaround/tap responsiveness (#314), or every initial momentum/equipment state.
#313 is ready for player confirmation, not closed.

## Running

Apply `integration.patch` from the repository root with `git -C upstream-sm
apply ../csharp/test-fixtures/movement-release/integration.patch`, build the
native Release x64 target, then run:

```powershell
& './upstream-sm/build/bin-x64-Release/sm.exe' --movement-release-probe 'Super Metroid.smc'
```

The command is handled before SDL initialization and disables Windows critical
error/fault dialogs. No emulator window is opened. The printed `MOVE` and
`RELEASE` stages distinguish movement from later animation/pose displacement.
Reverse only this integration patch when finished; never reset unrelated native
work. The patch is deliberately not part of the normal application build.

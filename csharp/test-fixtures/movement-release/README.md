# Movement-release cartridge probe and runtime regression

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
CPU's `$00C4.4000`. After the fix, four exact X positions, base speeds, and retained
one-pixel probe displacements pass for both directions in dry and water fixtures.

For left-facing water, the first four CPU X values are `$00C4.4000`, `$00C0.8800`,
`$00BC.D800`, `$00B9.3000`. Dry values are `$00C4.4000`, `$00C1.0000`, `$00BE.4000`,
`$00BC.0000`. These are synthetic experiment results, not player-recording data.

The probe explicitly restores cartridge bytes after `SnesInit`: the upstream
comparison harness patches carry instructions, which must not silently become
the reference for a retail comparison.

Scope: this proves the no-input release-path defect, not the original pipe-exit
speed (#312), full stopping trajectories, or release of Dash while direction is
still held. The player's exact meaning of release in #313 remains ambiguous.
Do not declare either whole ticket resolved from these four-frame fixtures.

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

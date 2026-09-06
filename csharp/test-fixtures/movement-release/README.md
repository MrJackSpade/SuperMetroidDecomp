# Movement-release cartridge probe — diagnostic, not a passing regression

This console-only experiment constructs a flat floor and running-left Samus at
full base speed, then releases all input in dry and submerged variants. It has
no player recording, SRAM, or ROM bytes. Supply a local cartridge separately.

## Status / limitations

**Do not use this probe's current trajectory as a golden expected result.**
The dry control unexpectedly clears base speed on the first movement call, and
the submerged trace gains a further one-pixel left shift between movement and
the end of the pose/animation calls. The starting state or callable-routine
boundaries need investigation before this can establish gameplay parity.

The probe explicitly restores cartridge bytes after `SnesInit`: the upstream
comparison harness patches carry instructions, which must not silently become
the reference for a retail comparison. This alone did not correct the control.

Next: isolate each native call, validate its entry prerequisites, and establish
a stable dry-floor control before comparing the managed runtime. Do not clamp
door-exit speed based on this experiment. Issues #312/#313 remain unresolved.

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

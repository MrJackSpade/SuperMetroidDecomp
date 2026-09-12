# Shaktool digging — #605

Affected version: 0.2.1. Immutable production capture:
`shaktool-release-021/slot-0.smstate` (room `$8F:D8C5`).

## State restoration prerequisites

The first current-build replay failed before gameplay: the saved room-load
callback's return type named assembly version0.2.1, while the unchanged compiled
signature named the current version. Comparing resolved allowed types instead of
assembly-qualified strings preserves exact signatures without rejecting a version
change. A synthetic return/parameter test also verifies that a genuinely changed
parameter remains rejected. No compiler-closure alias was added: the source
room-load callbacks and ordinal are unchanged.

The next load exposed two missing per-frame pose/camera accumulators. The prior
count-based migration incorrectly removed health-warning and pose-history fields
instead. The explicit migration omits the two new accumulators (plus the already
supported stationary script owner), initializing null/zero: no invented pending
camera correction. All remaining field names and order are still checked. A
regression compares the complete selected0.2.1 field sequence and retains existing
health/pose state.

## Reproduction, before digging changes

```text
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --shaktool-dig-state "Super Metroid.smc" csharp/test-fixtures/shaktool-release-021/slot-0.smstate
```

Loads through the production reader using a disposable temporary slot; never
overwrites live slots. Runs2400 neutral-input frames through `Game.Step`, logs
the seven linked segments every120 frames, and renders private local screenshots
at0/1200/2400 to `csharp/test-temp/shaktool-dig-605`.

Observed: Shaktool moves/reverses but **zero foreground words change**. The actual
sand at column16, rows5–10 is `$A110`, BTS `$0F`; enclosing rows4/11 are ordinary
solid tiles. These are not guessed or substituted terrain fixtures.

Pinned native `$A0:C2C0` masks BTS with `$7F` and uses table `$A0:C2DA`: entry15
spawns PLM `$84:D094` and returns no collision. `$84:B3D4` immediately clears the
collision nibble; `$84:CD53` queues sound and draws the timed crumble sequence.
The current shared horizontal/vertical enemy probes instead treat all spike
blocks as solid. Implement and verify the missing shared reaction, not a
Shaktool-specific wall bypass. Digging is **not fixed yet**; no player-validation
label is warranted by successful state loading or moving actors alone.

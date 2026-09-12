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
Shaktool-specific wall bypass. These observations were captured before the
production reaction was implemented.

## Verified terrain fix

Both shared horizontal and vertical movers now consult the native spike/BTS
table. Nonzero entry15 allocates the existing room's PLM synchronously, clears
collision during setup, and returns no collision even if the native pool is full.
Zero entries remain solid. The PLM uses the cartridge instruction list for sound,
draws and deletion. The room owner is supplied only for the synchronous enemy
frame and is restored on exit/throw; it is not extra serialized emulated state.

The production-state regression failed before this fix. It now runs6000 neutral
frames, observes first digging at1046, and asserts the first block's full visual
sequence:0053 for4 frames,0054 for4,0055 for4,00FF thereafter. All216 original sand
blocks finish as00FF. Local screenshots of the first/last crumble stages were
inspected. A subsequent bounded Right/Jump input sequence moves Samus through
the cleared passage toX849, staying in the same room. No mid-replay actor or
terrain writes are used. The first-dig frame is a regression observation from
this capture, not a claim of a native full-encounter trajectory comparison.

Independent original-65816 probe: actual room data,32 BTS values (0..15 and
80..8F), empty/full40-slot pools, plus16 handler frames for each successful
breakable case. Its96 rows were captured twice with identical SHA256:
`5A74E32C57E7BEB4B6F2760E8D6F3367B7F6C09E68CB514F3EAD7B5877944FEC`.
Numeric-only archive: `movement-release/enemy-breakable-605.zip`.
The managed audit applies these controls through all four directional movement
paths (384 comparisons): collision result, immediate level word, allocation,
active cursor/timer, timed draw updates, sound library/ID/queue cap, and deletion.
Native stale instruction words after deletion are not compared to cleared host
slots; allocation failure does not pretend setup ran.

```text
sm.exe --diagnostic-enemy-breakable ROM NEW.csv
dotnet run --project csharp/src/SuperMetroid.DebugRunner -c Release -- --enemy-breakable-native ROM NEW.csv
```

Use `movement-release/native-enemy-breakable-entrypoint.patch` with the matching
probe header, rebuild native Release/x64, and restore/rebuild after capture.
Native source pin578f90b3cc49557bb70060ad033bb90b8cf8ac50; disassembly pin
362be646929cf8e483f692b73a6561cfc2dc1d0d; ROM SHA256
12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72.

Full bank-$80 verification and the pre-existing Shaktool movement/combat/attack
audit pass. Leave #605 open for player confirmation after the final build check.

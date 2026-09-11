# Botwoon wall identity and mixed-population audit (#576)

This was found in a developer audit on unchanged ff4b0a3b, not a new player
report. The original runtime assertion expected exactly one active PLM after
loading defeated Botwoon and none after its wall cleared. Pinned bank $8F:C79F
contains a grey door for both room states, so those total-count expectations
were obsolete after the general population loader was completed.

The expanded reproduction also exposed a production omission: TrySpawnBotwoonWall
set Active and the instruction list but left HeaderPointer zero. Native
SpawnHardcodedPlm ($84:83D7) writes the header before setup. The failing capture
reported active headers C848,0000 with correctly opened scrolls 1/1.

## Fix and exact assertions

Store the supplied header in the allocated wall slot. The runtime audit now
requires one native grey door plus one identified clear-wall actor at block
(15,4), with its expected instruction list and timer. It retains the nine
individual air-block assertions, and after deletion requires that only the
grey door survives in its original native slot. No door removal or terrain
shortcut was introduced to satisfy the old count.

Run DebugRunner `--botwoon-audit ROM`. The complete live encounter and defeated
room integration pass. Before the production fix, the new identity assertion
failed with C848,0000. A separate Verification test, `--botwoon-plm-identity`,
checks both clear/crumble headers, native descending allocation order, exact
placement/programs, delays 1/64, full-pool rejection with all slots unchanged,
and reset. It also runs in the full verification suite.

Related developer audit repairs: #574 now checks Sbug pending/admitted knockback
separately; #575 checks Draygon's transient burial music at the publication frame.
Those repairs change diagnostics only. This ticket additionally corrects the
wall's production identity bookkeeping; it does not claim a new visible boss fix.

Reference: pinned upstream-sm sm_84.c and upstream-disassembly bank_8F.asm,
using the project's existing NTSC ROM. No private ROM or capture data is bundled.

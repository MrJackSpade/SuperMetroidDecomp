# #402 X-Plasma parity

## Reproduced shared dispatcher defect

The focused `--x-plasma-timers` Verification fixture loads an ordinary Ripper
through the production room-enemy loader, then processes it with time frozen.
Before the fix, the first frame retained invincibility 10 instead of decrementing
to 9. Movement, instruction position, flash and Ice-freeze timers are asserted
independently throughout twelve calls. The test does not inject a timer reset on
X-ray activation or pretend an enemy is a boss.

Pinned `upstream-disassembly/src/bank_A0.asm`, EnemyMain $A0:9021-$902E,
decrements a tangible enemy's invincibility before its time-freeze branch.
The pinned C translation in `upstream-sm/src/sm_a0.c`, EnemyMain, agrees.
The port instead decremented at the end and only when time was not frozen.
Production now decrements before actor work, respects intangible property $0400,
and retains the entry invincibility value for the collision gate. The call which
decrements one to zero still skips collision; the following call admits contact.
Tests cover both this edge and intangible actors retaining their timer.

## Verification and limits

Run the standard Verification suite or its `--x-plasma-timers` selector.
The counter assertion failed before production changes and passes afterward.
The full suite passes with the shared dispatcher correction. The additional
expiration/contact boundary assertions pass in the focused selector.

## Original-CPU counter comparison

`movement-release/native-xplasma-timer-probe.h` runs original cartridge
EnemyMain ($A0:8FD4) through the existing bounded 65C816 harness. Its headless
entrypoint patch suppresses dialogs and runs before SDL initialization. The
loader restores original ROM bytes after harness initialization. No translated
C EnemyMain call is used as the oracle.

Ten cases cover tangible/intangible actors with starting invincibility
0, 1, 2, 10 and 65535; twelve consecutive calls per case produce 120 records.
Flash, Ice freeze, X and instruction position are compared as well as the
invincibility countdown. All 120 records match the production dispatcher:

```text
sm.exe --diagnostic-xplasma-timers "Super Metroid.smc" <new-private-output.csv>
Verification --native-x-plasma-timers <new-private-output.csv>
```

The native actor is an invisible, active-offscreen Ripper with time frozen,
not a substituted boss. The private trace is not committed. After capture the
temporary patch was reversed, unrelated native edits retained, and the normal
native executable rebuilt. The reusable probe and comparator are tracked.

This establishes a required timing mechanism, not completion of #402.
Required remaining work includes
normal controller X-ray activation/release with a retained penetrating shot,
per-hit health/position traces for Phantoon (including the initial non-Plasma
hit), Botwoon, Draygon, steel pirates and Hyper Beam, failed timing and
nonpenetrating controls, and matching native encounter sequences. Leave the
issue open and not awaiting player validation until those cases are verified.

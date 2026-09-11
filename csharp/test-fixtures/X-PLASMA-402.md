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

This establishes a required timing mechanism, not completion of #402. It is a
constructed production-dispatcher fixture compared to pinned native source,
not yet an independent original-CPU capture. Required remaining work includes
normal controller X-ray activation/release with a retained penetrating shot,
per-hit health/position traces for Phantoon (including the initial non-Plasma
hit), Botwoon, Draygon, steel pirates and Hyper Beam, failed timing and
nonpenetrating controls, and matching native encounter sequences. Leave the
issue open and not awaiting player validation until those cases are verified.

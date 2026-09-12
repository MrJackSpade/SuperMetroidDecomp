# Ridley pogo RNG parity (#595)

Native $A6:B90F SetRidleyPogoSpeeds loads RandomNumberSeed directly and masks it
with three. Pinned sm_a6.c Ridley_Func_29 likewise reads random_number; neither
advances the generator. The port incorrectly called _nextRandom().

Reproduced through the actual initializer with independent current-word and
advance callbacks: expected zero RNG advances, observed one. This is not an
inference from a no-crash test. Changed the call to RequireRandomNumber, using
the current-word service already supplied by both runtime room-load paths.

The regression covers all 65,536 random words, six health-stage inputs (including
the existing out-of-range clamp), and four previous horizontal speed/sign
boundaries: 1,572,864 actual calls. Each must read once, advance zero times and
produce the precise signed X/Y launch speed and asymmetric accelerations selected
by the pinned ROM's pointer tables, while preserving the caller timer and stage.

The same initializer's lookup records were compiled under #547. Its shared test
checks every authored table stage, including the two unused prefix stages, with
runtime ROM reads forbidden. These changes are verified together in one commit
because the regression exercises the complete selected-record/launch operation.

Full Release Verification, Windows build and the complete Norfair Ridley audit
pass: 360 reveal frames, 4,096 combat frames and 738 death frames. #595 remains
open for player confirmation; no claim is made that all Ridley RNG calls have
been audited by this routine-specific correction.

## Follow-up: remaining Norfair attack setup calls

Inspected every RNG call in RoomEnemySystem.Ridley.cs against the pinned native
functions. Two more sites incorrectly advanced RNG: pogo setup $A6:B5C4 and
fireball timer setup $A6:B6DD. The attack selector $A6:B321 genuinely invokes
NextRandom and is intentionally unchanged.

Added exact tests of both real setup paths for all 65,536 current RNG words.
Before each correction its assertion independently failed with expected zero
advances, observed one. After correction, pogo setup reads once and writes
(seed & 31) + 32; fireball setup reads twice (launch record and timer) without
advancing, writes (seed & 63) + 128, and retains its native phase handoff.
The non-spin pogo fixture isolates the timer write from the later decrement.

All 131,072 setup cases, the existing 1,572,864 launch cases, full Verification,
Windows build and complete Norfair Ridley audit pass. The encounter now traverses
ten combat states in its 4,096-frame run, with 360 reveal and 738 death frames.
This extends #595; it remains awaiting player confirmation. It does not claim
that other Ridley partials or unrelated enemy RNG ownership have been audited.

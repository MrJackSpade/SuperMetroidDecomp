# Damaged-pose bomb-jump reference (#413)

Status: native behavior captured, **not a completed C# parity fix**.

`native-hurt-bomb-probe.h` runs original CPU instructions after restoring the
unmodified cartridge bytes. It admits an enemy-projectile hit through the native
collision handler, then injects a centered bomb at its timer-eight collision seam.
This constructed bomb exercises real overlap and arbitration; it intentionally
does not establish normal placement, fuse, or bomb animation correctness.

The matrix contains both facings, ten bomb-contact frames (0–9), and sixteen
first-held Jump + opposite-direction frames (0–15), each traced for 40 frames:
320 cases / 12,800 rows. Equipment is Bombs + Morph Ball, cheats disabled, air,
99 initial health, integer position (128,160), zero initial subpixels/speeds.
The synthetic room is 16 by 32 blocks, with ceiling row zero, floor row sixteen,
and walls at columns zero/fifteen. Ceiling collision is intentionally possible;
do not mistake its termination of upward movement for reaching the natural apex.

## Observed reference case

For right-facing Samus, bomb contact at frame zero and boost input from frame 15:

| Frame | Pose | Hurt timer/direction | Bomb direction | Movement handler |
| --- | --- | --- | --- | --- |
| 0 | $53 | $0004 / $0001 | $0002 | $DF38 knockback |
| 4 | $53 | $0000 / $0001 | $0002 | $DF38 knockback |
| 5 | $53 | $0000 / $0000 | $0002 | $A337 normal |
| 6 | $53 | $0000 / $0000 | $0802 | $E025 bomb start |
| 7 | $53 | $0000 / $0000 | $0802 | $E032 bomb movement |
| 25 | $53 | $0000 / $0000 | $0802 | $E032 bomb movement |
| 26 | $50 | $0000 / $0000 | $0000 | $A337 normal |

Frame seven's upward speed is $0002.C000. The damaged pose survives launch and
the held input is admitted after bomb movement terminates. With input instead
starting at frame zero, pose $50 is selected immediately; the bomb never enters
its start/main handler in that case. These are fixture-local frame indices.

Pinned disassembly confirms the mechanism: `$90:DE78` is the bomb branch of the
post-movement hit-interruption routine, behind knockback-timer/direction handling;
`$90:E012` retains the current pose; `$91:EE80` installs the bomb-start handler and
locks the input handler. The managed runtime currently calls its setup method
before movement, so the next step is an exact managed comparison of this matrix,
not a speculative change to just that call site.

## Reproduction and preservation

Include `native-release-probe.h` then `native-hurt-bomb-probe.h` from `sm_rtl.c`
after `struct StateRecorder;`. Temporarily dispatch
`DiagnosticHurtBomb(romPath, outputCsvPath)` from `main` before SDL initialization.
Build the x64 Release native target, run the probe, and remove those hooks.
The output file must not already exist. Filter repetitive `RunAsmCode!` stdout
without terminating the process early. No GUI is launched.

`hurt-bomb-native-capture.zip` preserves the CSV, not ROM or player save data.
Two independent executions produced identical CSV SHA-256:
`F5077D8E9342DB7153C8A9C5EB15713DBA6E1F2274E435A116DACF0EA6498F00`.

ROM SHA-256: `12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72`.
Native C checkout: `578f90b3cc49557bb70060ad033bb90b8cf8ac50`.
Disassembly checkout: `362be646929cf8e483f692b73a6561cfc2dc1d0d`.

Remaining: compare the real C# frame path, fix observed deviations, add matching
regressions including adjacent timing failures, and verify an ordinary live bomb
placement/fuse handoff. Keep #413 open without the player-validation label until
those requirements are met.

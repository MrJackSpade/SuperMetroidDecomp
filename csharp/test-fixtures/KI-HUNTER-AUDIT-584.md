# Ki-Hunter detached-wing audit repair (#584)

The old one-frame motion assertion fails on baseline 5c3ec7a7 and on d16b1e6e.
An instrumented failing run reported flash=11, handler=0002, angle=E000,
speed=2A00 after the first detached frame. This is native hurt suppression,
not a quadratic-speed regression or a stuck wing.

Pinned upstream-sm 578f90b3cc49557bb70060ad033bb90b8cf8ac50, sm_a8.c
KiHunter_Shot ($A8:F701), copies body hurt timing to the wings before the
health threshold is reached. Its detachment branch does not clear that state.
The common enemy loop ($A0:9128) decrements flash after AI and clears hurt
only when the resulting timer is less than eight. The no-op hurt handler
therefore holds the wing through the call that reduces flash from eight to seven.

The diagnostic now asserts all five hold frames (12 down to 7), exact handler
retirement, stationary coordinates, angle and speed. The following call must
produce the exact native odd-word quadratic angle, signed truncated sine/cosine
coordinates, speed decrement, orbit state and flash=6. Reference expectations
read pinned ROM bytes independently of the compiled production math.

No production code changed. Release DebugRunner builds with zero warnings/errors.
The complete --ki-hunter-audit passes, covering 38 body/wing records, all variants,
patrol/swoop, ground behavior, animation callbacks, acid motion/damage, hurt/freeze
mirroring, cleared-wing aliases, detachment, resumed orbit, death and drawing.
This closes a developer diagnostic defect, not an unconfirmed player report.

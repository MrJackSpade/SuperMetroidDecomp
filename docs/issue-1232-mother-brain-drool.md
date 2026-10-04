# #1232: Mother Brain drool during recovery

Affected version: 0.4.15-smoke1231. The player reported drool starting too soon
after the baby Metroid released Mother Brain.

The rainbow sequence correctly disabled drool for low-power recovery, but the
live-room adapter never copied that flag to the head animation's shared state.
Consequently the real drool opcode kept spawning projectiles while the sequence
reported them disabled. Publish the sequence flag alongside the breath flag.
Also preserve the native low-power ordering: disable drool once both neck indices
are zero, even if the body has not yet returned to standing.

Reference: pinned NTSC J/U 1.0 disassembly
362be646929cf8e483f692b73a6561cfc2dc1d0d, bank_A9.asm:
$A9:BF56-BF68 clears drool before checking pose; $C059-C072 installs a $0300
timer, waits through zero, and enables drool on the 769th subsequent body call.

MotherBrainDroolTimingAudit loads the installed Mother Brain room, drives these
production sequence transitions, publishes them through the real live adapter,
and invokes the real head drool spawn handler. Before the fix it fails:
`Low power: expected drool enabled=False, live=True, sequence=False`.
After the fix it confirms no spawns during all 768 recovery calls, a spawn on
call 769, and suppression while the low-power body is still non-standing.
This focused fixture checks the reported timing contract, not a full boss battle.

Release DebugRunner build and `--mother-brain-drool-timing-audit <installation>`
passed. Awaiting player validation.

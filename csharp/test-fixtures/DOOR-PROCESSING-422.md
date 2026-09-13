# Sound-drain transition ownership (#422)

`--door-sound-wait-audit ROM` reproduces the formerly omitted enemy owner in
DoorTransitionState.WaitForSoundQueues and now guards its restored invocation.
It loads the real awakened Climb population, stages only the sound-wait phase,
and holds an unread sound request for eight calls. Camera alignment, room load,
Samus movement, real-time pacing and ordinary SPC drain duration are excluded.

Before the fix the constructed enemy frozen word remained120 during all eight wait calls.
Calling the existing enemy owner directly changes it to0 in the control.
The diagnostic checks that the control exercises a state change, then fails
because the wait stage omitted it. After the fix both paths agree. The initial assumption of a112 result was
incorrect for this actor/setup and is not used as a cartridge expectation.
This is an owner-invocation fixture, not a natural frozen-pirate technique or
an independent native CPU trace.

Pinned `upstream-sm/src/sm_82.c`, DoorTransitionFunction_WaitForSoundsToFinish
at `$82:E29E`, explicitly runs DetermineWhichEnemiesToProcess, EnemyMain,
DrawSamusEnemiesAndProjectiles and EnsureSamusDrawnEachFrame before testing
the three unread sound-ring counts. The former C# branch ran only NMI and the test.
It therefore could not reproduce enemy-driven sound production during this wait.

The correction reuses extracted actor drawing and the existing enemy phase,
without ordinary movement, terrain preparation, gameplay palette clocks or
room-main updates. It applies the native unconditional non-elevator Samus
draw afterward. Fresh enemy/music and post-draw Samus sounds are collected
before testing the queues; this does not increment a full gameplay publication
or replay old PLM/projectile requests. The diagnostic asserts unchanged Samus
16.16 position and animation timer, body drawing, and one real enemy sound
published exactly once through24 frontend wait calls. Its held acknowledgement
is constructed; it does not establish normal SPC drain duration.

Still required: compare exact cartridge timing and queue acknowledgement phases.
Selecting items, spin/Space Jump checks,
firing, landing, interrupted charge, attached enemies, looping liquids,
Power Bomb suppression and combined actions remain required by the ticket.

Reference: https://wiki.supermetroid.run/Processing (claims are investigation
inputs, not asserted timings). No private cartridge assets are published.

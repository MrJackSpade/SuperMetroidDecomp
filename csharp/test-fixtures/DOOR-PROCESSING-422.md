# Sound-drain transition ownership (#422)

`--door-sound-wait-audit ROM` is an intentionally failing diagnostic for the
currently omitted enemy owner in DoorTransitionState.WaitForSoundQueues.
It loads the real awakened Climb population, stages only the sound-wait phase,
and holds an unread sound request for eight calls. Camera alignment, room load,
Samus movement, real-time pacing and ordinary SPC drain duration are excluded.

The constructed enemy frozen word remains120 during all eight wait calls.
Calling the existing enemy owner directly changes it to0 in the control.
The diagnostic checks that the control exercises a state change, then fails
because the wait stage omitted it. The initial assumption of a112 result was
incorrect for this actor/setup and is not used as a cartridge expectation.
This is an owner-invocation fixture, not a natural frozen-pirate technique or
an independent native CPU trace.

Pinned `upstream-sm/src/sm_82.c`, DoorTransitionFunction_WaitForSoundsToFinish
at `$82:E29E`, explicitly runs DetermineWhichEnemiesToProcess, EnemyMain,
DrawSamusEnemiesAndProjectiles and EnsureSamusDrawnEachFrame before testing
the three unread sound-ring counts. The C# branch runs only NMI and the test.
It therefore cannot reproduce enemy-driven sound production during this wait.

Required correction: retain this stage's enemy/draw owners and publish their
fresh sounds without replaying stale Samus/PLM requests, advancing ordinary
Samus movement, or conflating camera alignment with sound drain. Then compare
the exact cartridge timing, including queue acknowledgement phases. No
production fix is included yet. Selecting items, spin/Space Jump checks,
firing, landing, interrupted charge, attached enemies, looping liquids,
Power Bomb suppression and combined actions remain required by the ticket.

Reference: https://wiki.supermetroid.run/Processing (claims are investigation
inputs, not asserted timings). No private cartridge assets are published.
